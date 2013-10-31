using System;

using More.OpenTK;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace More
{
    public class Test3DEngineWindow : CustomGameWindow
    {
        static void Main()
        {
            Test3DEngineWindow game = new Test3DEngineWindow();
            game.CustomGameLoop(null);
        }

        public Test3DEngineWindow()
            : base(800, 600, GraphicsMode.Default, "Test 3D Engine")
        {
        }

        public override void Initialize(Int64 nowMicros)
        {
            //
            // Initialize OpenGL
            //
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        public override bool Update(Int64 nowMicros, Int32 diffMicros)
        {
            return false;
        }

        public override void Render()
        {
            GL.ClearColor(.7f, .9f, 1f, 1);

            // Clear the screen and the depth buffer
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //
            // Setup Game World
            //
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            //GL.Ortho(--ArenaUtils.glOrthoHalfWidth, ArenaUtils.glOrthoHalfWidth,
            //        -ArenaUtils.glOrthoHalfHeight, ArenaUtils.glOrthoHalfHeight,
            //        -ArenaUtils.glOrthoHalfWidth, ArenaUtils.glOrthoHalfWidth);
            GL.Ortho(-15, 15, -15, 15, -15, 15);


            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 modelview = Matrix4.LookAt(Vector3.Zero, new Vector3(0f, 0f, -1f)/*Vector3.UnitZ*/, Vector3.UnitY);
            GL.LoadMatrix(ref modelview);





            GL.PushMatrix();

            // Translate origin to center of arena for rotation
            GL.Rotate(cameraSpinRotation, 0, 1, 0);
            GL.Rotate(cameraOverheadRotation, 1, 0, 0);



            GL.PopMatrix();
        }
    }
}
