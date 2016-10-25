using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DefLab.common.math;

namespace DefLab.common.kriging
{
    public class GridInfo
    {
        public Extent Extent { get; set; }

        /// <summary>
        /// Шаг
        /// </summary>
        public double Step { get; set; }

        /// <summary>
        /// Количество строк
        /// </summary>
        public int NumberRows
        {
            get { return Convert.ToInt32(Math.Ceiling(Extent.Height / Step)); }
        }

        /// <summary>
        /// Количество колонок
        /// </summary>
        public int NumberColumns
        {
            get { return Convert.ToInt32(Math.Ceiling(Extent.Width / Step)); }
        }

        public GridInfo() { }

        public GridInfo(GridInfo other)
        {
            Extent = new Extent(other.Extent);
            Step = other.Step;
        }

        public bool Equals(GridInfo other)
        {
            if (other == null)
            {
                return false;
            }
            return Extent.Equals(other.Extent) &&
                   Step.Equals(other.Step);
        }
    }
}
