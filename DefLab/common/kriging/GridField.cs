using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefLab.common.kriging
{
    public class GridField
    {
        public static double OUT_OF_RANGE_VALUE = Double.NaN;

        private readonly GridInfo _info;

        public double?[,] GridData { get; private set; }

        public double[,] GridDataNotNullable { get; set; }

        public GridInfo GridInfo
        {
            get { return _info; }
        }


   

       public GridField(GridInfo info)
        {
            _info = info;
            GridData = new double?[info.NumberColumns, info.NumberRows];
            GridDataNotNullable = new double[info.NumberColumns, info.NumberRows];
        }

        public double[] getXCoords()
        {
            var ret = new double[_info.NumberColumns];
            for (int idx = 0; idx < _info.NumberColumns; idx++)
            {
                ret[idx] = getXCoord(idx);
            }
            return ret;
        }

        public double[] getYCoords()
        {
            var ret = new double[_info.NumberRows];
            for (int idx = 0; idx < _info.NumberRows; idx++)
            {
                ret[idx] = getYCoord(idx);
            }
            return ret;
        }

        public int getColumn(double xCoord)
        {
            var colsBefore = (xCoord - _info.Extent.MinX) / _info.Step;
            return (int)Math.Floor(colsBefore);
        }

        public int getRow(double yCoord)
        {
            var rowsBefore = (yCoord - _info.Extent.MinY) / _info.Step;
            return (int)Math.Floor(rowsBefore);
        }

        public double? getGridValue(double xCoord, double yCoord)
        {
            int row = getRow(yCoord);
            int col = getRow(xCoord);
            bool isInvalidIndexes = row < 0 || col < 0;
            bool isOutOfRange = GridData.GetLength(0) <= row || GridData.GetLength(1) <= col;
            if (isInvalidIndexes || isOutOfRange)
            {
                return OUT_OF_RANGE_VALUE;
            }
            return GridData[row, col];
        }

        public void fillDataNotNullable(double? notDefinedValue)
        {
            for (int i = 0; i < _info.NumberColumns; i++)
            {
                for (int j = 0; j < _info.NumberRows; j++)
                {
                    var val = GridData[i, j];
                    if (val != null && !double.IsNegativeInfinity(val.Value))
                    {
                        GridDataNotNullable[i, j] = val == notDefinedValue ? KrigingInterpolator.MASK_CELL_VALUE : val.Value;
                    }
                    else
                    {
                        GridDataNotNullable[i, j] = KrigingInterpolator.MASK_CELL_VALUE;
                    }

                }
            }
        }

        public double getXCoord(int column)
        {
            return _info.Extent.MinX + column * _info.Step;
        }

        public double getYCoord(int row)
        {
            return _info.Extent.MinY + row * _info.Step;
        }

        public GridPoint createPoint(int column, int row)
        {
            if (column < 0 || column >= GridInfo.NumberColumns || row < 0 || row >= GridInfo.NumberRows)
            {
                return null;
            }
            return new GridPoint(getXCoord(column), getYCoord(row), GridData[column, row]);
        }

        /// <summary>
        /// Вычислить и заполнить нерасчитываемые ячейки
        /// </summary>
        /// <param name="blankPolygons">многоугольники для бланкования</param>
        /// <param name="badValue">Плохое значение, которое отвечает за не расчитываюмую ячейку</param>
        public void fillNotCalculatedCell(List<GridPolygonArea> blankPolygons, double badValue)
        {
            int countX = _info.NumberColumns;
            int countY = _info.NumberRows;
            int procCount = Environment.ProcessorCount;
            Parallel.For(0, procCount, workerId => {
                var max = countX * (workerId + 1) / procCount;
                for (var column = countX * workerId / procCount; column < max; ++column)
                {
                    for (var row = 0; row < countY; ++row)
                    {
                        var point = createPoint(column, row);

                        //TODO надо набирать маски для бланкования и внутренностей только один раз (пока не поменялись контура) а не при каждом вычислении кригинга
                        if (GridHelper.isPointInBlankArea(point, blankPolygons))
                        {
                            GridData[column, row] = badValue;
                        }
                    }
                }
            });
        }

        public void saveToXyzFile(string fileName)
        {
            using (var sw = new StreamWriter(fileName))
            {
                for (int x = 0; x < _info.NumberColumns; x++)
                {
                    for (int y = 0; y < _info.NumberRows; y++)
                    {
                        sw.WriteLine("{0}\t{1}\t{2}",
                            getXCoord(x).ToString(System.Globalization.CultureInfo.InvariantCulture),
                            getYCoord(y).ToString(System.Globalization.CultureInfo.InvariantCulture),
                            GridData[x, y].GetValueOrDefault(Double.NaN)
                                .ToString(System.Globalization.CultureInfo.InvariantCulture));
                    }
                }
            }
        }
    }
}
