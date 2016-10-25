// ******************************************************************************************************
// <copyright file="TypeKrigingModels.cs" company="Caroso.inc">
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

namespace DefLab.common.kriging.semivariances {
    /// <summary>
    /// Define model to be used by Kriging Method
    /// </summary>
    public interface IKrigingModel {
        /// <summary>
        /// Gets or sets to esblish the value of range in the model
        /// </summary>
        double Range { get; set; }

        /// <summary>
        /// Gets or sets to esblish the value of Sill in the model
        /// </summary>
        double Sill { get; set; }

        /// <summary>
        /// Gets or sets to esblish the value of nugget in the model
        /// </summary>
        double Nugget { get; set; }

        /// <summary>
        /// Nugget + Sill
        /// </summary>
        double Threshold { get; }

        /// <summary>
        /// Gets the name of the  model
        /// </summary>
        KrigingModelType ModelType { get; }

        /// <summary>
        /// Gets the name of the  model
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Calculate the value of the semivariance depend on distance
        /// </summary>
        /// <param name="distance">Distanve to be evaluated</param>
        /// <returns>The theoretical value</returns>
        double getValue(double distance);
    }
}