using DefLab.common.kriging;

namespace DefLab.common.math
{

    /// <summary>
    /// Отрезок
    /// </summary>
    public class LineSegment
    {

        /// <summary>
        /// Начальная тока отрезка
        /// </summary>
        public Coordinate2D A { get; private set; }

        /// <summary>
        /// Конечная точка отрезка
        /// </summary>
        public Coordinate2D B { get; private set; }

        /// <summary>
        /// Направляющая прямая отрезка
        /// </summary>
        public Line2D directLine { get; private set; }

        public LineSegment(Coordinate2D p1, Coordinate2D p2)
        {
            A = p1;
            B = p2;
            directLine = new Line2D(A, B, "", null);
        }

        public LineSegment(GridPoint p1, GridPoint p2) : this(p1.coord2D, p2.coord2D)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="point"></param>
        /// <returns>TRUE если точка лежит на данном отрезке</returns>
        public bool contains(Coordinate2D point)
        {
            return isHorizontallyIncluded(point) && isVerticallyIncluded(point) && directLine.isPointBelongs(point);
        }

        /// <summary>
        /// </summary>
        /// <param name="point"></param>
        /// <returns>TRUE если точка лежит на данном отрезке</returns>
        public bool contains(GridPoint point)
        {
            return contains(point.coord2D);
        }

        /// <summary>
        /// Проверяет что ордината точки  point между ординатами конца отрезка
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private bool isVerticallyIncluded(Coordinate2D point)
        {
            return (point.Y >= A.Y && point.Y <= B.Y) || ((point.Y <= A.Y && point.Y >= B.Y));
        }

        /// <summary>
        /// Проверяет что абсцисса точки  point между абсциссами конца отрезка
        /// </summary>
        /// <param name="testPoint"></param>
        /// <returns></returns>
        private bool isHorizontallyIncluded(Coordinate2D testPoint)
        {
            return (testPoint.X >= A.X && testPoint.X <= B.X) || ((testPoint.X <= A.X && testPoint.X >= B.X));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segment"></param>
        /// <returns>TRUE если отрезки пересекаются (именно отрезки, ограниченные своими точками, а не просто прмяые)</returns>
        public bool intersects(LineSegment segment)
        {
            //проверяем в какой полуплоскости точки второго отрезка относительно текущего
            bool b1 = directLine.isPointBelongsToPositiveHalfPlane(segment.A);
            bool b2 = directLine.isPointBelongsToPositiveHalfPlane(segment.B);
            //проверяем в какой полуплоскости точки текущего отрезка относительно второго
            bool b3 = segment.directLine.isPointBelongsToPositiveHalfPlane(A);
            bool b4 = segment.directLine.isPointBelongsToPositiveHalfPlane(B);
            //(условие наличия пересечения) у каждого отрезка точки другого отрезка должны лежать в разных полуплоскостях
            return b1 ^ b2 && b3 ^ b4;
        }

        public Coordinate2D intersect(LineSegment segment)
        {
            if (!intersects(segment))
            {
                return null;
            }

            return directLine.intersect(segment.directLine);
        }

    }
}
