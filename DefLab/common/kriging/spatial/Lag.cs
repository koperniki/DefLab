using System;

namespace DefLab.common.kriging.spatial {
    public class Lag {
        public int idOrigin;
        public int idTarget;
        public double distance;
        public double dx;
        public double dy;
        public double azimuth;
        public double semivariance;

        public double tolerance;
        public double lagSize;
        public int numLags;

        public Lag(int numberOfLags, double lagSize, double tolerance) {
            this.numLags = numberOfLags;
            this.lagSize = lagSize;
            this.tolerance = tolerance;
        }

        public int[] cells(int origin, int target, double dx, double dy, double azimuth) {
            idOrigin = origin;
            idTarget = target;
            this.dx = dx;
            this.dy = dy;
            distance = Math.Sqrt((dx*dx) + (dy*dy));
            this.azimuth = azimuth;
            return cells();
        }


        private int[] cells() {
            int[] indexer = new int[4];

            int idx1 = getId(dx + (tolerance*lagSize), true);
            int idx2 = getId(dx - (tolerance*lagSize), true);
            int idy1 = getId(dy + (tolerance*lagSize), false);
            int idy2 = getId(dy - (tolerance*lagSize), false);

            indexer[0] = idx1;

            if (idx1 != idx2) {
                indexer[1] = idx2;
            } else {
                indexer[1] = int.MinValue;
            }

            indexer[2] = idy1;

            if (idy1 != idy2) {
                indexer[3] = idy2;
            } else {
                indexer[3] = int.MinValue;
            }

            return indexer;
        }

        private int getId(double dat, bool axc) {
            if (axc) {
                if (dat >= 0) {
                    return (numLags - 1) + Convert.ToInt32(Math.Ceiling(dat/lagSize));
                }
                return (numLags) + Convert.ToInt32(Math.Floor(dat/lagSize));
            }
            if (dat >= 0) {
                return numLags - Convert.ToInt32(Math.Ceiling(dat/lagSize));
            }
            return (numLags - 1) - Convert.ToInt32(Math.Floor(dat/lagSize));
        }
    }
}