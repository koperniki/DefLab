namespace DefLab.common.kriging.spatial {
    public struct Covariance {
        Stat vi;
        Stat vj;
        double value;

        public Covariance(bool v) {
            vi = new Stat(false);
            vj = new Stat(false);
            value = 0;
        }

        public Covariance(double vvi, double vvj) {
            value = vvi * vvj;
            vi = new Stat(false);
            vj = new Stat(false);
            vi += new Stat(vvi);
            vj += new Stat(vvj);
        }

        Covariance(Covariance cc1, double v1, Covariance cc2, double v2) {
            vi = cc1.vi + cc2.vi;
            vj = cc1.vj + cc2.vj;
            value = v1 + v2;
        }

        public static Covariance operator +(Covariance c1, Covariance c2) {
            return new Covariance(c1, c1.value, c2, c2.value);
        }

        public double result() {
            return value - vi.Mean * vj.Mean * vi.num;
        }
    }
}
