using System;

namespace DefLab.common.kriging.spatial {
    public struct Stat {
        public double value;
        public long num;
        public double quad;
        private double max;
        private double min;
        private long posMin;
        private long posMax;

        public long PosMin
        {
            get { return posMin; }
        }

        public long PosMax
        {
            get { return posMax; }
        }


        public double Min
        {
            get { return min; }
        }


        public double Max
        {
            get { return max; }
        }

        public double Mean
        {
            get
            {
                if (num > 0) {
                    return value/num;
                }
                return 0;
            }
        }

        public double StdDevP
        {
            get
            {
                if (num > 0) {
                    return Math.Sqrt((quad - (2*value*Mean) + (num*Mean*Mean))/num);
                }
                return 0;
            }
        }

        public double StdDevS
        {
            get
            {
                if (num > 2) {
                    return Math.Sqrt((quad - (2*value*Mean) + (num*Mean*Mean))/(num - 1));
                } else {
                    return 0;
                }
            }
        }

        public Stat(bool v) {
            value = 0;
            quad = 0;
            min = double.MaxValue;
            max = double.MinValue;
            num = 0;
            posMax = 0;
            posMin = 0;
        }


        public Stat(double value) {
            this.value = value;
            quad = value*value;
            min = value;
            max = value;
            num = 1;
            posMax = 1;
            posMin = 1;
        }

        private Stat(Stat c, double value1, long v) {
            value = c.value + value1;
            quad = c.quad;
            quad += value1*value1;
            num = v;

            if (c.Min > value1) {
                min = value1;
                posMin = v;
            } else {
                min = c.Min;
                posMin = c.posMin;
            }

            if (c.Max < value1) {
                max = value1;
                posMax = v;
            } else {
                max = c.Max;
                posMax = c.posMax;
            }
        }

        public static Stat operator +(Stat c1, Stat c2) {
            return new Stat(c1, c2.value, c1.num + 1);
        }
    }
}