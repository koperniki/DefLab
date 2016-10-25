using System;
using System.Collections.Generic;

namespace DefLab.common.math
{
    /// <summary>
    ///     Уравнение прямой, прох-ей ч/з 2 точки: (y - y1)/(y2 - y1) = (x - x1) / (x2 - x1)
    /// Содержит также вектор нормали и направляющий вектор
    /// </summary>
    public class Line2D
    {
        private const double EPSILON = 1e-6;
        private readonly Dictionary<Line2D, Coordinate2D> _intersectionPoints;
        private readonly string _name;

        public object UserData { get; set; }
        public Vector2D Normal { get; private set; }
        public double A { get; private set; } //X member in line equation
        public double B { get; private set; } //Y member in line equation
        public double C { get; private set; }

        /// <summary>
        ///     Координаты точки пересечения прямой со своей нормалью, которая, в свою очередь проходит через скважину.
        ///     Т.е. пересечение прямой и перпендикулярной ей прямой, проходящей через скважину
        /// </summary>
        public Coordinate2D BasicPoint { get; private set; }

        /// <summary>
        ///     True, если базовая точка лежит внутри ячейки воронова.
        ///     Т.е. прямая соединяющая скважины лежит на видимом ребре ячейки, а не на мнимом продолжении ребра.
        ///     Это важно для определения ближайшего соседа в модели поиска кригинга.
        /// </summary>
        public bool IsGoodBasicPoint { get; set; }

        /// <summary>
        ///     точка для касания кривой Безье
        /// </summary>
        private Coordinate2D BezieTangentPoint { get; set; }

        public long IntersectionsCount
        {
            get { return _intersectionPoints.Count; }
        }

        /// <summary>
        ///     Возвращает длинну нормали (кратчайшее расстояние до скважины)
        /// </summary>
        public double Distance
        {
            get { return Normal.Length; }
        }

        /// <summary>
        ///     Прямая, проведенная через точку, с заданным вектором нормали
        /// </summary>
        /// <param name="pointCoord">
        ///     точка лежащая на прямой (обычно точка пересечения с вектором нормали проведенным от прямой к
        ///     скважине)
        /// </param>
        /// <param name="normal">вектор нормали. Обычно ориентирован по направлению к скважине</param>
        /// <param name="name">имя прямой, для упрощения отладки</param>
        /// <param name="userData"></param>
        public Line2D(Coordinate2D pointCoord, Vector2D normal, string name, object userData)
        {
            UserData = userData;
            _name = name;
            Normal = normal;
            BasicPoint = pointCoord;
            BezieTangentPoint = pointCoord;
            A = normal.X;
            B = normal.Y;
            C = calcCforPoint(pointCoord, normal);
            _intersectionPoints = new Dictionary<Line2D, Coordinate2D>();
        }

        public Line2D(Coordinate2D basicCoord, Coordinate2D secondCoord, string name, object userData)
        {
            UserData = userData;
            _name = name;
            Normal = new Vector2D(basicCoord, secondCoord).getPerpendicularVector();
            BasicPoint = basicCoord;
            BezieTangentPoint = basicCoord;
            A = Normal.X;
            B = Normal.Y;
            C = calcCforPoint(basicCoord, Normal);
            _intersectionPoints = new Dictionary<Line2D, Coordinate2D>();
        }

        /// <summary>
        ///     Прямая заданная своим уравнением (коэффициенты A,B,C и вектор нормали)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="normalCopy"></param>
        /// <param name="basicPointCopy">координаты базовой точки на прямой (см описание basicPoint)</param>
        /// <param name="bezieTantengPointCopy"></param>
        /// <param name="userData"></param>
        private Line2D(string name, double a, double b, double c, Vector2D normalCopy, Coordinate2D basicPointCopy,
            Coordinate2D bezieTantengPointCopy, object userData)
        {
            UserData = userData;

            A = a;
            B = b;
            C = c;
            _name = name;
            Normal = normalCopy;
            BasicPoint = basicPointCopy;
            BezieTangentPoint = bezieTantengPointCopy;
            _intersectionPoints = new Dictionary<Line2D, Coordinate2D>();
        }


        /// <summary>
        ///     True, если на прямой задана точка касания для сегмента кривой Безье.
        /// </summary>
        /// <returns></returns>
        public bool hasTangentPoint()
        {
            return BezieTangentPoint != null;
        }

        public Vector2D getDirectVector()
        {
            return Normal.getPerpendicularVector();
        }

        public void addIntersectionPoint(Line2D line, Coordinate2D poinCoordinate2D)
        {
            if (_intersectionPoints.ContainsKey(line))
            {
                return;
            }

            _intersectionPoints.Add(line, poinCoordinate2D);
        }

        public void removeIntersectionPoint(Line2D line)
        {
            if (line != null &&
                _intersectionPoints.ContainsKey(line))
            {
                _intersectionPoints.Remove(line);
            }
        }

        public Coordinate2D intersectAndAdd(Line2D line)
        {
            var intersectionPoint = intersect(line);
            if (intersectionPoint != null)
            {
                addIntersectionPoint(line, intersectionPoint);
                line.addIntersectionPoint(this, intersectionPoint);
            }

            return intersectionPoint;
        }

        public Coordinate2D intersect(Line2D line)
        {
            try
            {
                var x = (C * line.B - line.C * B) / (line.A * B - A * line.B);
                if (double.IsNaN(x) || double.IsInfinity(x))
                {//a1 = a; b1 = b - parallel lines
                    return null;
                }

                var y = calcYbyX(x, line);
                if (double.IsNaN(y) || double.IsInfinity(y))
                {
                    return null;
                }

                return new Coordinate2D(x, y);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Возвращает точку на прямой, являющуюся проекцией заданной
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Coordinate2D getProjectionPoint(Coordinate2D p)
        {
            if (isContains(p))
            {
                return new Coordinate2D(p.X, p.Y);
            }
            return intersect(new Line2D(p, Normal.getPerpendicularVector(), null, null));
        }

        public double calcYbyX(double x, Line2D line)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (B == 0)
            {
                return (-line.C - line.A * x) / line.B;
            }

            return (-C - A * x) / B;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public double solveForPoint(Coordinate2D p)
        {
            return A * p.X + B * p.Y + C;
        }

        /// <summary>
        ///     Показывает, что проверяемая точка лежит в положительной полуплоскости данной прямой.
        ///     Положительная полуплоскость образована прямой и вектором нормали.
        ///     Т.е. точка должна лежать с той же стороны, в которую направлен вектор нормали данной прямой.
        ///     Т.о. точка будет иметь положительное решение если она лежит в той полуплоскости куда направлен вектор норали.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool isPointBelongsToPositiveHalfPlane(Coordinate2D point)
        {
            var equatiorResult = solveForPoint(point);
            return equatiorResult > 0 || Math.Abs(equatiorResult) < EPSILON;
        }

        /// <summary>
        ///     Показывает, что проверяемая точка лежит в положительной полуплоскости данной прямой.
        ///     Положительная полуплоскость образована прямой и вектором нормали.
        ///     Т.е. точка должна лежать с той же стороны, в которую направлен вектор нормали данной прямой.
        ///     Т.о. точка будет иметь положительное решение если она лежит в той полуплоскости куда направлен вектор норали.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool isPointBelongs(Coordinate2D point)
        {
            return Math.Abs(distanceToPoint(point)) < EPSILON;
        }

        public double distanceToPoint(Coordinate2D p)
        {
            return solveForPoint(p) / Math.Sqrt(A * A + B * B);
        }

        private bool isContains(Coordinate2D point)
        {
            foreach (var intersectionPoint in _intersectionPoints.Values)
            {
                return intersectionPoint.Equals(point);
            }

            return false;
        }

        private static double calcCforPoint(Coordinate2D p, Vector2D normal)
        {
            var a = normal.X;
            var b = normal.Y;
            var c = -a * p.X - b * p.Y;
            return c;
        }

        public IEnumerable<Coordinate2D> getIntersectionPoints()
        {
            return new List<Coordinate2D>(_intersectionPoints.Values);
        }

        /// <summary>
        ///     Calculates distance of the orthogonal projection for point
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public double calcDistanceToPoint(Coordinate2D coordinate)
        {
            return (A * coordinate.X + B * coordinate.Y + C) / (Math.Sqrt(A * A + B * B));
        }

        /// <summary>
        ///     копирование БЕЗ точек пересечения
        /// </summary>
        /// <returns></returns>
        public Line2D copy()
        {
            var basicPointCopy = new Coordinate2D(BasicPoint.X, BasicPoint.Y);
            var bezieTantengPointCopy = new Coordinate2D(BezieTangentPoint.X, BezieTangentPoint.Y);
            var normalCopy = Normal.copy();

            return new Line2D(_name + "_COPY", A, B, C, normalCopy, basicPointCopy, bezieTantengPointCopy, UserData);
        }

        public string getEquationStr()
        {
            return string.Format("{0}*x + {1}*y + {2}", A, B, C);
        }

        public bool hasIntersectionFor(Line2D l)
        {
            return _intersectionPoints.ContainsKey(l);
        }
    }
}