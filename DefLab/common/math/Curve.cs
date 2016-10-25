using System;
using System.Collections.Generic;
using System.Linq;
using DefLab.common.kriging;

namespace DefLab.common.math
{
    /// <summary>
    /// Кривая
    /// </summary>
    public class Curve
    {

        /// <summary>
        /// Точки кривой
        /// </summary>
        public List<GridPoint> Points { get; private set; }

        /// <summary>
        /// Отрезки кривой
        /// </summary>
        public List<LineSegment> Segments { get; private set; }

        public Curve(List<GridPoint> points = null)
        {
            Points = new List<GridPoint>();
            Segments = new List<LineSegment>();
            if (points != null)
            {
                Points.AddRange(points);
            }

            calcSegments();
        }

        public void add(GridPoint point)
        {
            if (Points.Count > 0)
            {
                Segments.Add(new LineSegment(Points.Last(), point));
            }
            Points.Add(point);
        }

        public void calcSegments()
        {
            Segments.Clear();
            if (Points.Count < 2)
            {
                return;
            }

            var prevPoint = Points.First();
            Points.Skip(1).ToList().ForEach(delegate (GridPoint point)
            {
                Segments.Add(new LineSegment(prevPoint, point));
                prevPoint = point;
            });
        }

        public Extent calcExtent()
        {
            double maxX = Double.NegativeInfinity;
            double maxY = Double.NegativeInfinity;
            double minX = Double.PositiveInfinity;
            double minY = Double.PositiveInfinity;
            Points.ToList().ForEach(delegate (GridPoint point)
            {
                if (point.X < minX)
                {
                    minX = point.X;
                }
                if (point.Y < minY)
                {
                    minY = point.Y;
                }
                if (point.X > maxX)
                {
                    maxX = point.X;
                }
                if (point.Y > maxY)
                {
                    maxY = point.Y;
                }
            });

            return new Extent(minX, maxX, minY, maxY);
        }

        /// <summary>
        /// замкнута ли кривая (проверка только по равенству первой и последней точки)
        /// </summary>
        /// <returns></returns>
        public bool isClosed()
        {
            return Points != null && Points.Count > 1 && Points.First().coord2D.Equals(Points.Last().coord2D);
        }

        /// <summary>
        /// Замкнуть кривую (совмещаем конец с началом)
        /// </summary>
        public void close()
        {
            if (isClosed() || Points.Count < 3)
            {
                return;
            }

            var first = Points.First();
            add(new GridPoint(first.X, first.Y));
        }
    }
}
