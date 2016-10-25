using System;
using System.Collections.Generic;
using System.Linq;
using DefLab.common.math;

namespace DefLab.common.kriging
{
    /// <summary>
    /// Класс полигона на основе массива точек и кривой содержащей эти точки
    /// Замкнутая кривая (многоугольник) произвольной формы (может быть выпуклым и вогнутым)
    /// </summary>
    public class GridPolygon
    {
        public Curve Line { get; set; }

        public Extent Extent { get; private set; }
        public GridPolygon(GridPoint[] points)
        {
            Line = new Curve(points.ToList());
            calcExtent();
        }

        public GridPolygon(List<GridPoint> points)
        {
            Line = new Curve(points);
            calcExtent();
        }

        private void calcExtent()
        {
            Extent = Line.calcExtent();
        }
        public bool isClosed()
        {
            return Line.isClosed();
        }

        /// <summary>
        /// Находится ли точка внутри габаритного контейнера данного полигона (грубая проверка)
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool isInsideExtent(Coordinate2D point)
        {
            return point.X > Extent.MinX && point.X < Extent.MaxX &&
                point.Y > Extent.MinY && point.Y < Extent.MaxY;
        }

        public bool isOnEdge(Coordinate2D c)
        {
            return Line.Segments.Any(segment => segment.contains(c));
        }

        /// <summary>
        /// Находится ли точка внутри данного ЗАМКНУТОГО (!) полигона 
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool isInside(Coordinate2D point)
        {
            return isClosed() && isPointInsideContourByWinding(point, true);
        }

        /// <summary>
        /// Проверяем как точка расположена по отношению к замкнутому полигону (внутри, снаружи, или на ребре)
        /// </summary>
        /// <returns></returns>
        public GridPolygonAreaType checkPointArea(Coordinate2D c, bool tracing = false)
        {
            if (isOnEdge(c))
            {
                return GridPolygonAreaType.onBorder;
            }

            if (tracing)
            {
                return checkPointInPolygonTracing(new GridPoint(c.X, c.Y));
            }

            if (isInside(c))
            {
                return GridPolygonAreaType.innerArea;
            }

            return GridPolygonAreaType.outerArea;
        }



        #region WINDING NUMBER METHOD
        //================================================================================
        //================================================================================
        //============             WINDING NUMBER METHOD          ========================
        //================================================================================
        //================================================================================

        public static double DEFAULT_WINDING_NUM_CONDITION = 1.9 * Math.PI;

        /// <summary>
        /// https://ru.wikipedia.org/wiki/%D0%97%D0%B0%D0%B4%D0%B0%D1%87%D0%B0_%D0%BE_%D0%BF%D1%80%D0%B8%D0%BD%D0%B0%D0%B4%D0%BB%D0%B5%D0%B6%D0%BD%D0%BE%D1%81%D1%82%D0%B8_%D1%82%D0%BE%D1%87%D0%BA%D0%B8_%D0%BC%D0%BD%D0%BE%D0%B3%D0%BE%D1%83%D0%B3%D0%BE%D0%BB%D1%8C%D0%BD%D0%B8%D0%BA%D1%83
        ///  
        /// https://en.wikipedia.org/wiki/Winding_number
        /// </summary>
        /// <param name="c"></param>
        /// <param name="useWindingNumDirection"></param>
        /// <returns></returns>
        private bool isPointInsideContourByWinding(Coordinate2D c, bool useWindingNumDirection)
        {
            if (!isInsideExtent(c))
                return false;

            var windingNum = calcWindingNumber(c, useWindingNumDirection);
            //return Math.Abs(windingNum) >= 2 * Math.PI;//это более строгое условие, не будем ег опока использовать. Надо много тестов напсиать
            return Math.Abs(windingNum) >= DEFAULT_WINDING_NUM_CONDITION;
        }


        /// <summary>
        /// Рассчитывает winding number точки с относительно границы многоугольника.
        /// Для всех точек вне конутра значение будет меньше 2 * Pi.
        /// </summary>
        /// <param name="c">проверяемая точка</param>
        /// <param name="useWindingNumDirection">Учитывать направление (как то связано с направлением обхода - требуется уточнить)</param>
        /// <returns></returns>
        private double calcWindingNumber(Coordinate2D c, bool useWindingNumDirection)
        {
            var windingNum = 0.0;

            if (!isClosed())
                return windingNum;

            Line.Segments.ForEach(delegate (LineSegment segment) {
                var v1C = new Vector2D(c, segment.A);
                var v2C = new Vector2D(c, segment.B);

                var angleRad = v1C.calcAngleRad(v2C);

                if (useWindingNumDirection)
                {
                    if (v1C.calcZofVectorProduction(v2C) < 0)
                        angleRad = -angleRad;
                }

                windingNum += angleRad;
            });


            return windingNum;
        }

        #endregion


        #region  method
        //================================================================================
        //================================================================================
        //==== METHOD (ODD NUMBER OF TRACING EDGES (horizontal traceing only))======
        //================================================================================
        //================================================================================

        /// <summary>
        /// определяет нахождение точки в области
        /// </summary>
        /// <param name="point"></param>
        /// <returns>  </returns>
        public GridPolygonAreaType checkPointInPolygonTracing(GridPoint point)
        {
            if (Line.Points.Count < 3)
            {
                return GridPolygonAreaType.outerArea;
            }

            GridPoint[] polygonPoints = Line.Points.ToArray();
            bool isInside = false;
            for (int idx = 0, j = polygonPoints.Length - 1; idx < polygonPoints.Length; j = idx++)
            {
                var pt1 = polygonPoints[idx];
                var pt2 = polygonPoints[j];

                if (isPointInSegment(pt1, pt2, point))
                {
                    return GridPolygonAreaType.onBorder;
                }

                bool insideY = ((point.Y >= pt1.Y) && (point.Y < pt2.Y)) || ((point.Y >= pt2.Y) && (point.Y < pt1.Y));
                if (insideY)
                {
                    bool xOnLeft = (point.X < (pt2.X - pt1.X) * (point.Y - pt1.Y) / (pt2.Y - pt1.Y) + pt1.X);
                    if (xOnLeft)
                    {
                        //инвертируем только если точка слева от ребра. Если инверсий бедет нечетное кол-во, значит точка внутри
                        // это похоже на упрощение метода обхода
                        isInside = !isInside;
                    }
                }
            }

            return isInside ? GridPolygonAreaType.innerArea : GridPolygonAreaType.outerArea;
        }

        /// <summary>
        /// Находится ли точка на отрезке
        /// </summary>
        /// <param name="segmentPoint1">точка начала отрезка</param>
        /// <param name="segmentPoint2">точка конца отрезка</param>
        /// <param name="point">интересующая точка</param>
        /// <returns>тру если находится</returns>
        private bool isPointInSegment(GridPoint segmentPoint1, GridPoint segmentPoint2, GridPoint point)
        {
            if (segmentPoint1.Equals(point))
            {
                return true;
            }
            if (segmentPoint2.Equals(point))
            {
                return true;
            }

            var a = new GridPoint(segmentPoint2.X - segmentPoint1.X, segmentPoint2.Y - segmentPoint1.Y);
            var b = new GridPoint(point.X - segmentPoint1.X, point.Y - segmentPoint1.Y);
            var sa = a.X * b.Y - b.X * a.Y;
            if (sa > 0 || sa < 0)
            {
                return false;
            }

            if (a.X * b.X < 0 || a.Y * b.Y < 0)
            {
                return false;
            }

            if (Math.Sqrt(a.X * a.X + a.Y * a.Y) < Math.Sqrt(b.X * b.X + b.Y * b.Y))
            {
                return false;
            }

            return true;
        }
        #endregion


        /// <summary>
        /// Замкнуть полигон
        /// </summary>
        public void close()
        {
            Line.close();
        }
    }
}
