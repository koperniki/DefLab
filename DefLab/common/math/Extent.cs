using System;

namespace DefLab.common.math
{

    /// <summary>
    /// Габариты объекта
    /// </summary>
    public class Extent : IEquatable<Extent>
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public Coordinate2D UpLeftCoord { get; set; }
        public Coordinate2D DownRightCoord { get; set; }
        public Coordinate2D CenterCoord { get; set; }

        public double MinX
        {
            get { return UpLeftCoord.X; }
        }

        public double MaxX
        {
            get { return DownRightCoord.X; }
        }

        public double MinY
        {
            get { return DownRightCoord.Y; }
        }

        public double MaxY
        {
            get { return UpLeftCoord.Y; }
        }

        public Extent(Coordinate2D upLeftCoord, Coordinate2D downRightCoord)
        {
            UpLeftCoord = upLeftCoord;
            DownRightCoord = downRightCoord;
            calcSizes();
            calcCenter();
        }

        public Extent(double minX, double maxX, double minY, double maxY) :
            this(new Coordinate2D(minX, maxY), new Coordinate2D(maxX, minY))
        {
        }

        public Extent(Extent extent) :
            this(new Coordinate2D(extent.MinX, extent.MaxY), new Coordinate2D(extent.MaxX, extent.MinY))
        {

        }

        private void calcSizes()
        {
            Width = Math.Abs(UpLeftCoord.X - DownRightCoord.X);
            Height = Math.Abs(UpLeftCoord.Y - DownRightCoord.Y);
        }
        private void calcCenter()
        {
            double centerX = (DownRightCoord.X - UpLeftCoord.X) / 2;
            double centerY = (UpLeftCoord.Y - DownRightCoord.Y) / 2;
            CenterCoord = new Coordinate2D(centerX, centerY);
        }

        public bool Equals(Extent other)
        {
            return other != null &&
                   MaxX.Equals(other.MaxX) &&
                   MaxY.Equals(other.MaxY) &&
                   MinX.Equals(other.MinX) &&
                   MinY.Equals(other.MinY);
        }
    }
}
