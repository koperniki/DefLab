using System;
using System.Collections.Generic;
using System.Linq;
using DefLab.common.kriging;

namespace sibir.oilsys.math.mapoperation.surfaces.interpolation.@internal
{
    internal class SimpleSortedList
    {
        private readonly double[] _dist;
        private readonly GridPoint[] _points;
        private readonly int _count;

        public SimpleSortedList(int count)
        {
            _dist = new double[count];
            for (int i = 0; i < count; i++)
            {
                _dist[i] = Double.MaxValue;
            }
            _points = new GridPoint[count];
            _count = count;
        }

        public void add(GridPoint point, double distance)
        {
            if (distance > _dist[_count - 1])
            {
                return;
            }

            var idxNextMax = getMaxIndex();

            if (distance < _dist[0])
            {
                var tempD = _dist[0];
                var tempP = _points[0];
                _dist[0] = distance;
                _points[0] = point;

                _dist[_count - 1] = _dist[idxNextMax];
                _points[_count - 1] = _points[idxNextMax];

                _dist[idxNextMax] = tempD;
                _points[idxNextMax] = tempP;
            }
            else
            {
                _dist[_count - 1] = _dist[idxNextMax];
                _points[_count - 1] = _points[idxNextMax];
                _dist[idxNextMax] = distance;
                _points[idxNextMax] = point;
            }
        }

        private int getMaxIndex()
        {
            double max = _dist[1];
            int idx = 1;
            for (int i = 2; i < _count - 1; i++)
            {
                if (_dist[i] > max)
                {
                    max = _dist[i];
                    idx = i;
                }
            }

            return idx;
        }

        public IEnumerable<GridPoint> toList()
        {
            return _points
                .Where(t => t != null)
                .ToList();
        }
    }
}