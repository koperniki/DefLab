using System;
using DefLab.common.math;

namespace DefLab.common.kriging
{
    /// <summary>
    /// Точка со значением на сетке
    /// </summary>
    public class GridPoint : IEquatable<GridPoint>
    {

        public Coordinate2D coord2D { get; set; }
        public double X { get { return coord2D.X; } set { coord2D.X = value; } }
        public double Y { get { return coord2D.Y; } set { coord2D.Y = value; } }
        public double? Z { get; set; }

        public GridPoint(double x, double y)
        {
            coord2D = new Coordinate2D(x, y);
        }

        public GridPoint(double x, double y, double z)
            : this(x, y)
        {
            Z = z;
        }

        public GridPoint(double x, double y, double? z) : this(x, y)
        {
            Z = z;
        }

        public GridPoint(GridPoint otherPoint) : this(otherPoint.X, otherPoint.Y, otherPoint.Z) { }

        public double distanceToPoint2D(GridPoint other)
        {
            return coord2D.distance(other.coord2D);
        }

        public bool Equals(GridPoint otherPoint)
        {
            if (this == otherPoint)
            {
                return true;
            }

            if (otherPoint == null)
            {
                return false;
            }

            if (Z.isBad() && !otherPoint.Z.isBad())
            {
                return false;
            }

            if (!Z.isBad() && otherPoint.Z.isBad())
            {
                return false;
            }

            if (!coord2D.Equals(otherPoint.coord2D))
            {
                return false;
            }

            // ReSharper disable once PossibleInvalidOperationException (смотри проверки выше)
            return (Z.isBad() && otherPoint.Z.isBad()) || Math.Abs(Z.Value - otherPoint.Z.Value) < Constants.ACCURACY;
        }

        public override string ToString()
        {
            return String.Format("[{0}: {1:0.###}]", coord2D, Z);
        }
    }

}