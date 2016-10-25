using System;
using DefLab.common.kriging.semivariances;

namespace DefLab.common.kriging
{
    /// <summary>
    /// Конфигурация вариограммы
    /// </summary>
    public class KrigingModelConfig : IEquatable<KrigingModelConfig>
    {
        /// <summary>
        /// Тип модели вариограммы
        /// </summary>
        public KrigingModelType ModelType { get; set; }
        /// <summary>
        /// Диапазон
        /// </summary>
        public double Range { get; set; }
        /// <summary>
        /// Порог
        /// </summary>
        public double Sill { get; set; }
        /// <summary>
        /// Самородок
        /// </summary>
        public double Nugget { get; set; }

        public KrigingModelConfig() { }

        public KrigingModelConfig(KrigingModelConfig other)
        {
            ModelType = other.ModelType;
            Range = other.Range;
            Sill = other.Sill;
            Nugget = other.Nugget;
        }

        public bool Equals(KrigingModelConfig other)
        {
            if (other == null)
            {
                return false;
            }
            return ModelType == other.ModelType &&
                   Range.Equals(other.Range) &&
                   Sill.Equals(other.Sill) &&
                   Nugget.Equals(other.Nugget);
        }
    }
}
