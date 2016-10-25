using System;
using System.Collections.Generic;

namespace DefLab.common.kriging
{

    /// <summary>
    /// Тип учитываемой области полигона
    /// </summary>
    public enum GridPolygonAreaType
    {
        /// <summary>
        /// Учитывать внутреннюю область
        /// </summary>
        innerArea,
        /// <summary>
        /// Учитывать внешнюю область
        /// </summary>
        outerArea,
        /// <summary>
        /// Учитывать нахождение на границе области
        /// </summary>
        onBorder
    }

    /// <summary>
    /// Класс полигона учета области
    /// </summary>

    public class GridPolygonArea : GridPolygon
    {

        public GridPolygonAreaType Type { get; set; }

        public GridPolygonArea(List<GridPoint> gridPoints) : base(gridPoints)
        {
            Type = GridPolygonAreaType.innerArea;
        }
    }
}
