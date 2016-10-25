namespace DefLab.common.kriging.semivariances {
    public abstract class BaseKrigingModel {
        /// <summary>
        /// Gets or sets to define the Range
        /// </summary>
        public double Range { get; set; }

        /// <summary>
        /// Gets or sets the sill in the model
        /// </summary>
        public double Sill { get; set; }

        /// <summary>
        /// Gets or sets the nugget in the model
        /// </summary>
        public double Nugget { get; set; }

        /// <summary>
        /// Nugget + Sill
        /// </summary>
        public double Threshold {
            get { return Nugget + Sill; }
        }

        protected BaseKrigingModel() {
            Range = 1;
        }
    }
}
