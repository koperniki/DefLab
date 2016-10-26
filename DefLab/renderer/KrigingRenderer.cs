using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ThreeCs;
using ThreeCs.Cameras;
using ThreeCs.Core;
using ThreeCs.Extras.Geometries;
using ThreeCs.Extras.Objects;
using ThreeCs.Materials;
using ThreeCs.Math;
using ThreeCs.Objects;
using ThreeCs.Scenes;
using Three = ThreeCs.Three;

namespace DefLab.renderer
{
    public class KrigingRenderer : BaseRenderer
    {

        public List<List<Vector3>> dataList;

        private PerspectiveCamera camera;

        private Scene scene;

        private Object3D mesh;

        private CamControl controls;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        public override void Load(Control control)
        {
            base.Load(control);

            //

            camera = new PerspectiveCamera(50, control.Width / (float)control.Height, 1, 4000);
           // new OrthographicCamera(-control.Width/20, control.Width / 20, -control.Height / 20, control.Height / 20);//
            this.camera.Position.Z = 50;
         
            camera.LookAt(new Vector3(0,0,0));

            controls = new CamControl(control, camera);

            controls.RotateSpeed = 5;
            controls.ZoomSpeed = 5;
            controls.PanSpeed = 2;

          

            controls.StaticMoving = true;
            controls.DynamicDampingFactor = 0.4f;

            scene = new Scene();
            scene.Add(camera);

            var material = new LineBasicMaterial { VertexColors = ThreeCs.Three.VertexColors };

            if (dataList != null)
            {
                foreach (var data in dataList)
                {

                    int segments = data.Count;
                    var geometry = new BufferGeometry();
                    var positions = new float[segments*3];
                    var colors = new float[segments*3];

                    for (int i = 0; i < segments; i++)
                    {
                        positions[i*3 + 0] = data[i].X;
                        positions[i*3 + 1] = data[i].Y;
                        positions[i*3 + 2] = data[i].Z*5;

                        colors[i*3 + 0] = 0.5f;
                        colors[i*3 + 1] = 0.5f;
                        colors[i*3 + 2] = 0.5f;
                    }

                    geometry.AddAttribute("position", new BufferAttribute<float>(positions, 3));
                    geometry.AddAttribute("color", new BufferAttribute<float>(colors, 3));

                    geometry.ComputeBoundingSphere();

                    mesh = new Line(geometry, material);
                    scene.Add(mesh);
                }
            }


           

            renderer.gammaInput = true;
            renderer.gammaOutput = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientSize"></param>
        public override void Resize(Size clientSize)
        {
            Debug.Assert(null != this.camera);
            Debug.Assert(null != this.renderer);

            this.camera.Aspect = clientSize.Width / (float)clientSize.Height;
            this.camera.UpdateProjectionMatrix();

            this.renderer.Size = clientSize;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Render()
        {
            controls.Update();

            renderer.Render(scene, camera);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Unload()
        {
            this.scene.Dispose();
            this.camera.Dispose();

            base.Unload();
        }
    }
}
