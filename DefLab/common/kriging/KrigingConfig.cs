using System;

namespace DefLab.common.kriging
{
    /// <summary>
    /// Конфигурация для интерполяции
    /// </summary>
    public class KrigingConfig : IEquatable<KrigingConfig>
    {
        /// <summary>
        /// Радиус поиска
        /// </summary>
        public double SearchRadius { get; set; }

        /// <summary>
        /// Количество секторов поиска
        /// </summary>
        public int SectorsCount { get; set; }

        /// <summary>
        /// Максимальное количество точек в секторе
        /// </summary>
        public int MaxPointsInSector { get; set; }

        /// <summary>
        /// значение "не определенному" значению
        /// </summary>
        public double? NotDefinedValue { get; set; }

        public KrigingConfig() { }

        public KrigingConfig(KrigingConfig other)
        {
            SearchRadius = other.SearchRadius;
            SectorsCount = other.SectorsCount;
            MaxPointsInSector = other.MaxPointsInSector;
            NotDefinedValue = other.NotDefinedValue;
        }

        public bool Equals(KrigingConfig other)
        {
            if (other == null)
            {
                return false;
            }
            return SearchRadius.Equals(other.SearchRadius) &&
                   SectorsCount.Equals(other.SectorsCount) &&
                   MaxPointsInSector.Equals(other.MaxPointsInSector) &&
                   NotDefinedValue.Equals(other.NotDefinedValue);
        }
    }
}