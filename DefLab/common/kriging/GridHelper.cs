using System;
using System.Collections.Generic;

namespace DefLab.common.kriging
{

    [Obsolete("будет заменен")]
    public static class GridHelper
    {

        /// <summary>
        /// Определяет находится ли точка в области бланкования
        /// </summary>
        /// <param name="point"></param>
        /// <param name="blankPolygons"></param>
        /// <returns></returns>
        public static bool isPointInBlankArea(GridPoint point, List<GridPolygonArea> blankPolygons)
        {
            foreach (var blankPolygon in blankPolygons)
            {
                if (blankPolygon.Type == blankPolygon.checkPointArea(point.coord2D, false))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
