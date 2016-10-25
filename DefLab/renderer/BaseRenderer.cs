﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ThreeCs.Renderers;

namespace DefLab.renderer
{
    public  class BaseRenderer
    {
        protected WebGLRenderer renderer;

        protected readonly Random random = new Random();

        protected readonly ColorConverter colorConvertor = new ColorConverter();

        protected readonly Stopwatch stopWatch = new Stopwatch();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        public virtual void Load(Control control)
        {
            Debug.Assert(null != control);

            this.renderer = new WebGLRenderer(control);

            stopWatch.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientSize"></param>
        public virtual void Resize(Size clientSize)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientSize"></param>
        /// <param name="here"></param>
        public virtual void MouseUp(Size clientSize, Point here)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientSize"></param>
        /// <param name="here"></param>
        /// <param name="delta"></param>
        public virtual void MouseWheel(Size clientSize, Point here, int delta)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientSize"></param>
        /// <param name="here"></param>
        public virtual void MouseMove(Size clientSize, Point here)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientSize"></param>
        /// <param name="here"></param>
        public virtual void MouseDown(Size clientSize, Point here)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Render()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Unload()
        {
            this.renderer.Dispose();
        }
    }
}
