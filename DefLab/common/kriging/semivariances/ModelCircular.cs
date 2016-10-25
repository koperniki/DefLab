// ******************************************************************************************************
// <copyright file="ModelCircular.cs" company="Caroso.inc">
//     Copyright (c) Carlos Osorio All rights reserved.
// </copyright>
// ******************************************************************************************************
// The contents of this file are subject to the Mozilla Public License Version 1.1 (the "License"); 
// you may not use this file except in compliance with the License. You may obtain a copy of the License at 
// http:// www.mozilla.org/MPL/ 
// Software distributed under the License is distributed on an "AS IS" basis, WITHOUT WARRANTY OF 
// ANY KIND, either express or implied. See the License for the specificlanguage governing rights and 
// limitations under the License. 
// 
// The Original Code is this . mapWindow 
// 
// The Initial Developer of this version of the Original Code is Carlos Osorio carosocali@gmail.com
// 
// Contributor(s): (Open source contributors should list themselves and their modifications here). 
// Change Log: 
// Date           Changed By      Notes
// 22 Nov 2010    Carlos Osorio   Inital upload 
// ******************************************************************************************************

using System;

namespace DefLab.common.kriging.semivariances {
    /// <summary>
    /// Spherical Model 
    /// </summary> 
    public class ModelCircular : BaseKrigingModel, IKrigingModel {
        /// <summary>
        /// Gets the name 
        /// </summary>
        public string Name
        {
            get { return "Circular"; }
        }

        /// <summary>
        /// Gets the name of the  model
        /// </summary>
        public KrigingModelType ModelType
        {
            get { return KrigingModelType.circular; }
        }


        /// <summary>
        /// Obtain the value of the interpolation
        /// </summary>
        /// <param name="distance">Distance to be evaluated in the model</param>
        /// <returns>Value interpolated</returns>
        public double getValue(double distance) {
            if (distance.Equals(0)) {
                return Nugget;
            }

            if (distance <= Range &&
                distance > 0) {
                return Nugget +
                       (((2*Sill)/Math.PI)*
                        (((distance/Range)*Math.Sqrt(1 - ((distance*distance)/(Range*Range)))) +
                         Math.Asin(distance/Range)));
            }
            return Nugget + Sill;
        }
    }
}