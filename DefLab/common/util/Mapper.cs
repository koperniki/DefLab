using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DefLab.common.kriging;
using ThreeCs.Math;

namespace DefLab.common.util
{
    public static class Mapper
    {
        public static List<List<Vector3>> gridFieldToVector3List(GridField field)
        {
            var ret = new List<List<Vector3>>();
            var data = field.GridDataNotNullable;
            var xc = field.getXCoords();
            var yc = field.getYCoords();
            for (int i = 0; i < field.GridInfo.NumberColumns-1; i++)
            {
                for (int j = 0; j < field.GridInfo.NumberRows - 1; j++)
                {
                    ret.Add( new List<Vector3>(5)
                    {
                        new Vector3((float)xc[i], (float)yc[j], (float)data[i,j]),
                        new Vector3((float)xc[i+1], (float)yc[j], (float)data[i+1,j]),
                        new Vector3((float)xc[i+1], (float)yc[j+1], (float)data[i+1,j+1]),
                        new Vector3((float)xc[i], (float)yc[j+1], (float)data[i,j+1]),
                        new Vector3((float)xc[i], (float)yc[j], (float)data[i,j]),
                    });
                }
            }

            return ret;
        }
    }
}
