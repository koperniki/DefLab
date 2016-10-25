using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;
using sibir.oilsys.math.mapoperation.surfaces.interpolation.@internal;

namespace DefLab.common.kriging
{
    public static class KrigingInterpolator
    {
        /// <summary>
        /// Значение ячейки, которую не нужно расчитывать
        /// </summary>
        public const double MASK_CELL_VALUE = double.NegativeInfinity;

        /// <summary>
        /// Выполнить интерполяцию Кригинга
        /// </summary>
        /// <param name="gridInfo">Информация о регулярной сетке</param>
        /// <param name="config">Конфигурация интерполятора</param>
        /// <param name="semivarianseDelegate">Вариограмма</param>
        /// <param name="irregularPoints">Массив исходных точек для интерполяции</param>
        /// <param name="sourcePolygons">Дополнительные массивы точек для интерполяции, взятые с контуров</param>
        /// <param name="blankPolygons">Контуры бланкования, в которых интерполяция не производится</param>
        /// <returns>Регулярную сетку значений</returns>
        public static GridField doInterpolate(GridInfo gridInfo, KrigingConfig config, Func<double, double> semivarianseDelegate,
            GridPoint[] irregularPoints, List<GridPolygon> sourcePolygons = null, List<GridPolygonArea> blankPolygons = null)
        {

            if (irregularPoints == null || irregularPoints.Length == 0 || config == null)
            {
                return null;
            }
            if (gridInfo == null || gridInfo.Extent.Width < gridInfo.Step || gridInfo.Extent.Height < gridInfo.Step)
            {
                return null;
            }

            /*if (sourcePolygons == null || sourcePolygons.Count == 0) {
                sourcePolygons = getSourcePoligon(gridInfo);
            }*/

            //            if (blankPolygons == null || blankPolygons.Count == 0) {
            //                blankPolygons = getBlankPoligon(gridInfo);
            //            }

            var allPoints = irregularPoints;
            if (sourcePolygons != null && sourcePolygons.Any())
            {
                var allPointsList = irregularPoints.ToList();
                foreach (var gridPolygon in sourcePolygons)
                {
                    if (gridPolygon.Line.Points != null && gridPolygon.Line.Points.Any())
                    {
                        allPointsList.AddRange(gridPolygon.Line.Points);
                    }
                }
                allPoints = allPointsList.ToArray();
            }

            var grid = new GridField(gridInfo);

            //хранилище для значений X на основании которых будем затем восстанавливать сигнал бикубическим сплайном
            var xBag = new ConcurrentBag<KeyValuePair<double, int>>();
            //хранилище для значений Y на основании которых будем затем восстанавливать сигнал бикубическим сплайном
            var yBag = new ConcurrentBag<KeyValuePair<double, int>>();


            Parallel.For(0, Environment.ProcessorCount,
                workerId => calculate(config, grid, semivarianseDelegate, workerId, allPoints, xBag, yBag, 0));


            try
            {
                alglib.spline2dinterpolant interpolant = getInterpolant(grid, xBag, yBag);
                if (blankPolygons != null && blankPolygons.Any())
                {
                    grid.fillNotCalculatedCell(blankPolygons, MASK_CELL_VALUE);
                }

                Parallel.For(0, Environment.ProcessorCount,
                    workerId => restoreGridCells(grid, interpolant, workerId));

            }
            catch (Exception e)
            {

                var t = e;
            }






            grid.fillDataNotNullable(config.NotDefinedValue);

            return grid;
        }

        /* TODO delete
                private static List<GridPolygon> getSourcePoligon(GridInfo gridInfo) {
                    var width = gridInfo.Width;
                    var height = gridInfo.Height;

                    var poligon = new GridPolygon();
                    var points = new List<GridPoint>();

                    for (double i = gridInfo.MinY; i < gridInfo.MaxY; i += gridInfo.Step) {
                        points.Add(new GridPoint(gridInfo.MinX, i));
                    }

                    for (double i = gridInfo.MinX; i < gridInfo.MaxX; i += gridInfo.Step) {
                        points.Add(new GridPoint(i, gridInfo.MaxY));
                    }

                    for (double i = gridInfo.MaxY; i > gridInfo.MinY; i -= gridInfo.Step) {
                        points.Add(new GridPoint(gridInfo.MaxX, i));
                    }

                    for (double i = gridInfo.MaxX; i > gridInfo.MinX; i -= gridInfo.Step) {
                        points.Add(new GridPoint(i, gridInfo.MinY));
                    }

                    poligon.Points = points.ToArray();
                    return new List<GridPolygon> { poligon };
                }
         */

        /// <summary>
        /// Восстановление значений в ячейках грида с помощью бикубической интерполяции, вызывается в параллельных потоках
        /// </summary>
        /// <param name="grid">Грид</param>
        /// <param name="interpolant">Функция для восстановления значений</param>
        /// <param name="workerId">идентификатор рабочего процесса</param>
        private static void restoreGridCells(GridField grid, alglib.spline2dinterpolant interpolant, int workerId)
        {
            int procCount = Environment.ProcessorCount;
            int countX = grid.GridInfo.NumberColumns;
            int countY = grid.GridInfo.NumberRows;
            var max = countX * (workerId + 1) / procCount;
            for (var column = countX * workerId / procCount; column < max; ++column)
            {
                for (var row = 0; row < countY; ++row)
                {
                    double? v = grid.GridData[column, row];
                    if (v == null || !double.IsNegativeInfinity(v.Value))
                    {
                        var point = grid.createPoint(column, row);
                        grid.GridData[column, row] = alglib.spline2dcalc(interpolant, point.X, point.Y);
                    }
                }
            }
        }

        /// <summary>
        /// Получение восстановленной функции, которой будем восстанавливать поверхность
        /// </summary>
        /// <param name="grid">Регулярная сетка</param>
        /// <param name="xBag">хранилище для значений X на основании которых будем затем восстанавливать сигнал</param>
        /// <param name="yBag">хранилище для значений Y на основании которых будем затем восстанавливать сигнал</param>
        /// <returns></returns>
        private static alglib.spline2dinterpolant getInterpolant(GridField grid, IEnumerable<KeyValuePair<double, int>> xBag, IEnumerable<KeyValuePair<double, int>> yBag)
        {
            List<KeyValuePair<double, int>> listValueIndexX = xBag.ToList().OrderBy(t => t.Key).ToList();
            //значения по X для восстановления сигнала
            double[] xVals = listValueIndexX.Select(t => t.Key).ToArray();

            List<KeyValuePair<double, int>> listValueIndexY = yBag.ToList().OrderBy(t => t.Key).ToList();
            //значения по Y для восстановления сигнала
            double[] yVals = listValueIndexY.Select(t => t.Key).ToArray();

            //значения в узлах для восстановления сигнала
            var f = new double[xVals.Length * yVals.Length];

            for (var idxX = 0; idxX < listValueIndexX.Count; ++idxX)
            {
                for (var idxY = 0; idxY < listValueIndexY.Count; ++idxY)
                {
                    double? v = grid.GridData[listValueIndexX[idxX].Value, listValueIndexY[idxY].Value];
                    // ReSharper disable once PossibleInvalidOperationException
                    //TODO подумать как избавиться от нулей
                    f[idxX + idxY * listValueIndexX.Count] = v.isBad() ? 0 : v.Value;
                }
            }
            alglib.spline2dinterpolant interpolant;
            // build spline
            alglib.spline2dbuildbicubicv(xVals, xVals.Length, yVals, yVals.Length, f, 1, out interpolant);
            return interpolant;
        }

        /// <summary>
        /// Расчет ячеек грида кригингом, вызывается в параллельных потоках
        /// </summary>
        /// <param name="config">Настройки для расчёта</param>
        /// <param name="grid">Рассчитываемый грид</param>
        /// <param name="semivarianseDelegate">Вариограмма</param>
        /// <param name="workerId">идентификатор рабочего процесса</param>
        /// <param name="points">точки из которых набираем значения для расчёта</param>
        /// <param name="xvals">
        /// Хранилище куда складываем насчитанные значения по X<br/>
        /// В дальнейшем используется для кубического сплайна
        /// </param>
        /// <param name="yvals">
        /// Хранилище куда складываем насчитанные значения по Y<br/>
        /// В дальнейшем используется для кубического сплайна
        /// </param>
        /// <param name="step">
        /// Шаг по которому будем вычислять в каких ячейках насчитать значения<br/>
        /// Если передано значение &lt;= 0, тогда возьмем 5
        /// </param>
        private static void calculate(KrigingConfig config, GridField grid, Func<double, double> semivarianseDelegate, int workerId, GridPoint[] points,
            ConcurrentBag<KeyValuePair<double, int>> xvals, ConcurrentBag<KeyValuePair<double, int>> yvals, int step)
        {
            if (grid == null || grid.GridData == null)
            {
                return;
            }

            if (step <= 0)
            {
                step = 10;
            }
            int countX = grid.GridInfo.NumberColumns;
            int countY = grid.GridInfo.NumberRows;
            var degreeOfParallelism = Environment.ProcessorCount;
            var max = countX * (workerId + 1) / degreeOfParallelism;
            //нужно ли заполнять множество Y значениями
            bool needFillYValues = workerId == 0;

            for (int column = countX * workerId / degreeOfParallelism; column < max; column += step)
            {
                for (var row = 0; row < countY; row += step)
                {
                    var point = grid.createPoint(column, row);
                    double? v = grid.GridData[column, row];
                    if (v == null || !double.IsNegativeInfinity(v.Value))
                    {
                        var nearestPoints = getNearestPoints(point, points, config.SearchRadius, config.SectorsCount, config.MaxPointsInSector);
                        grid.GridData[column, row] = (nearestPoints.Length != 0)
                                                         ? interpolatePoint(point, nearestPoints, semivarianseDelegate)
                                                         : config.NotDefinedValue;
                    }
                    if (needFillYValues)
                    {
                        yvals.Add(new KeyValuePair<double, int>(point.Y, row));
                    }
                }
                needFillYValues = false;
                xvals.Add(new KeyValuePair<double, int>(grid.getXCoord(column), column));
            }
        }

        /* TODO delete
         private static List<GridPolygonArea> getBlankPoligon(GridInfo gridInfo) {

                 var width = gridInfo.Width;
                 var height = gridInfo.Height;

                 gridInfo.MinX -= width * 0.2;
                 gridInfo.MaxX += width * 0.2;

                 gridInfo.MinY -= height * 0.2;
                 gridInfo.MaxY += height * 0.2;

                 var poligon = new GridPolygonArea {Type = GridPolygonAreaType.outerArea};

                 var points = new List<GridPoint>();

                 points.Add(new GridPoint(gridInfo.MinX, gridInfo.MinY));
                 points.Add(new GridPoint(gridInfo.MinX, gridInfo.MaxY));
                 points.Add(new GridPoint(gridInfo.MaxX, gridInfo.MaxY));
                 points.Add(new GridPoint(gridInfo.MaxX, gridInfo.MinY));

                 poligon.Points = points.ToArray();
                 return new List<GridPolygonArea> { poligon };
         }*/

        private static GridPoint[] getNearestPoints(GridPoint regPoint, GridPoint[] irregularPoints, double radius, int sectorsCount, int maxNumOfPointsInSector)
        {
            var points = new SimpleSortedList[sectorsCount];

            var sectorPointLength = sectorsCount - 1;

            for (int idx = 0; idx < sectorsCount; ++idx)
            {
                points[idx] = new SimpleSortedList(maxNumOfPointsInSector);
            }

            for (int idx = 0; idx < irregularPoints.Length; ++idx)
            {
                var irregularPoint = irregularPoints[idx];
                var distance = irregularPoint.distanceToPoint2D(regPoint);
                if (distance <= radius)
                {
                    var angle = 180 + Math.Atan2(irregularPoint.Y - regPoint.Y, irregularPoint.X - regPoint.X) * 180 / Math.PI;
                    if (angle >= 360)
                    {
                        points[sectorPointLength].add(irregularPoint, distance);
                        continue;
                    }
                    if (angle <= 0)
                    {
                        points[0].add(irregularPoint, distance);
                        continue;
                    }
                    var sect = (int)(angle / (360f / sectorsCount));
                    points[sect].add(irregularPoint, distance);
                }
            }

            var result = new List<GridPoint>();
            for (int idx = 0; idx < sectorsCount; ++idx)
            {
                result.AddRange(points[idx].toList());
            }

            return result.ToArray();
        }

        private static double interpolatePoint(GridPoint regularPoint, GridPoint[] nearestPoints, Func<double, double> semivarianseDelegate)
        {
            var size = nearestPoints.Length;
            var matrixArr = new DenseMatrix(size + 1, size + 1);
            var vectorArr = new DenseVector(size + 1);
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    matrixArr[i, j] = semivarianseDelegate(nearestPoints[i].distanceToPoint2D(nearestPoints[j]));
                }
                matrixArr[i, size] = 1;
                matrixArr[size, i] = 1;
                vectorArr[i] = semivarianseDelegate(nearestPoints[i].distanceToPoint2D(regularPoint));
            }
            matrixArr[size, size] = 0;
            vectorArr[size] = 1;

            var coefs = matrixArr.Solve(vectorArr);

            if (coefs.Any(c => c.isBad()))
            {
                return 0; //TODO fix this
            }

            //Выяснить надо ли нормировать коэффициенты, чтобы их сумма была - 1
            //            var coefSum = coefs.Take(coefs.Count - 1).Sum();
            //            for (int i = 0; i < coefs.Count-1; i++) {
            //                coefs[i] = coefs[i]/coefSum;
            //            }

            return coefs.Take(coefs.Count - 1).Select((x, i) => x * nearestPoints[i].Z.GetValueOrDefault()).Sum();
        }
    }
}
