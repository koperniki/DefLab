using System;

namespace DefLab.common.math
{
    /// <summary>
    ///     Координата со значениями удвоенной точности
    /// </summary>
    public class Coordinate2D : IEquatable<Coordinate2D>
    {
        
        public Coordinate2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; set; }
        public double Y { get; set; }

        public double distance(Coordinate2D point)
        {
            var dx = X - point.X;
            var dy = Y - point.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        public bool Equals(Coordinate2D point)
        {
            return point != null &&
                  Math.Abs(X - point.X) < Constants.ACCURACY &&
                  Math.Abs(Y - point.Y) < Constants.ACCURACY;
        }
        public override string ToString()
        {
            return String.Format("({0:0.###}; {1:0.###})", X, Y);
        }
    }
}
