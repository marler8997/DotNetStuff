using System;
using System.Threading;

using Marler.OpenTK.Common;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace DataVis
{
    public class DataVisWindow : CustomApplicationWindow
    {
        DataFollower dataFollower;
        public DataVisWindow(DataFollower dataFollower)
            : base(dataFollower.DataEvent, 800, 600, GraphicsMode.Default, "DataVis")
        {
            this.dataFollower = dataFollower;
        }
        public override void Initialize()
        {
        }

        public override void Update()
        {
        }

        public override void Draw()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            //GL.Ortho(0, ClientRectangle.Width, 0, ClientRectangle.Height, -100, 100);
            GL.Ortho(0, 800, 0, 600, -100, 100);

            GL.Begin(BeginMode.Quads);
            GL.Color4(0f, 0f, 0f, .6f);
            GL.Vertex2(5, 5);
            GL.Vertex2(5, 28);
            //GL.Vertex2(ClientRectangle.Width - 5, 28);
            //GL.Vertex2(ClientRectangle.Width - 5, 5);
            GL.Vertex2(795, 28);
            GL.Vertex2(795, 5);
            GL.End();
        }
    }
}
