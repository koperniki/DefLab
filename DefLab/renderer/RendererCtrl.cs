using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NHibernate.Criterion;

namespace DefLab.renderer
{
    public partial class RendererCtrl : UserControl
    {
        private BaseRenderer _renderer;
        private int windowHalfX;
        private int windowHalfY;
        private int mouseX;
        private int mouseY;

        public RendererCtrl()
        {
            InitializeComponent();
            glControl.MouseWheel += onGlMouseWheel;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void RunSample(BaseRenderer renderer)
        {
            if (null != _renderer)
            {
                _renderer.Unload();
                }

            Application.Idle -= ApplicationIdle;

            _renderer = renderer;
            if (null != _renderer)
            {
                _renderer.Load(this.glControl);
                _renderer.Resize(glControl.ClientSize);
                Application.Idle += ApplicationIdle;
            }
        }

        private void ApplicationIdle(object sender, EventArgs e)
        {
            while (glControl.IsIdle)
            {
                Render();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private void Render()
        {
            if (null != _renderer)
                _renderer.Render();


            this.glControl.SwapBuffers();
        }

        private void onGlPaint(object sender, PaintEventArgs e)
        {
            Render();
        }

        private void onGlResize(object sender, EventArgs e)
        {
            var control = sender as Control;

            if (control.ClientSize.Height == 0)
                control.ClientSize = new Size(control.ClientSize.Width, 1);

            if (null != _renderer)
                _renderer.Resize(control.ClientSize);
        }

        private void onGlMouseMove(object sender, MouseEventArgs e)
        {
            var control = sender as Control;

            mouseX = (e.X - windowHalfX);
            mouseY = (e.Y - windowHalfY);

            if (null != _renderer)
                _renderer.MouseMove(control.ClientSize, new Point(mouseX, mouseY));
        }

        private void onGlMouseDown(object sender, MouseEventArgs e)
        {
            var control = sender as Control;

            mouseX = (e.X - windowHalfX);
            mouseY = (e.Y - windowHalfY);

            if (null != _renderer)
                _renderer.MouseDown(control.ClientSize, new Point(mouseX, mouseY));
        }

        private void onGlMouseUp(object sender, MouseEventArgs e)
        {
            var control = sender as Control;

            mouseX = (e.X - windowHalfX);
            mouseY = (e.Y - windowHalfY);

            if (null != _renderer)
                _renderer.MouseUp(control.ClientSize, new Point(mouseX, mouseY));
        }

        private void onGlMouseWheel(object sender, MouseEventArgs e)
        {
            var control = sender as Control;

            mouseX = (e.X - windowHalfX);
            mouseY = (e.Y - windowHalfY);

            if (null != _renderer)
                _renderer.MouseWheel(control.ClientSize, new Point(mouseX, mouseY), e.Delta);
        }
    }
}
