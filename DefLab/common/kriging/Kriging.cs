using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DefLab.common.math;

namespace DefLab.common.kriging
{
    public class Kriging
    {
        


        public static GridField getGrid(GridInfo gridInfo, bool setGridBorders, KrigingConfig krigingConfig,
           Func<double, double> semivarianseDelegate, GridPoint[] unregularPoints,
           List<GridPolygon> sourceContours, List<GridPolygonArea> blankContours)
        {

            //TODO refactor blanks

            if (setGridBorders)
            {
                var maxX = Double.MinValue;
                var minX = Double.MaxValue;
                var maxY = Double.MinValue;
                var minY = Double.MaxValue;

                //TODO use Extent here
                Action<GridPoint[]> calcMinMax = array => {
                    if (array != null &&
                        array.Any())
                    {
                        foreach (var p in array)
                        {
                            if (p.X > maxX)
                            {
                                maxX = p.X;
                            }
                            if (p.X < minX)
                            {
                                minX = p.X;
                            }
                            if (p.Y > maxY)
                            {
                                maxY = p.Y;
                            }
                            if (p.Y < minY)
                            {
                                minY = p.Y;
                            }
                        }
                    }
                };

                //находим максимальные габариты контуров
                Action<GridPolygon> calcMinMaxContours = polygon => {
                    if (polygon != null)
                    {
                        if (polygon.Extent.MaxX > maxX)
                        {
                            maxX = polygon.Extent.MaxX;
                        }
                        if (polygon.Extent.MinX < minX)
                        {
                            minX = polygon.Extent.MinX;
                        }
                        if (polygon.Extent.MaxY > maxY)
                        {
                            maxY = polygon.Extent.MaxY;
                        }
                        if (polygon.Extent.MinY < minY)
                        {
                            minY = polygon.Extent.MinY;
                        }
                    }
                };

                calcMinMax(unregularPoints);

                if (sourceContours != null)
                {
                    sourceContours.ForEach(calcMinMaxContours);
                }

                if (blankContours != null)
                {
                    blankContours.ForEach(calcMinMaxContours);
                }

                gridInfo.Extent = new Extent(minX, maxX, minY, maxY);
            }

            return KrigingInterpolator.doInterpolate(gridInfo, krigingConfig, semivarianseDelegate, unregularPoints, sourceContours, blankContours);
        }

    }
}
