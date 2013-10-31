using System;
using System.Collections.Generic;

using More.OpenTK;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace More
{
    public class AISimulator : CustomGameWindow
    {
        float cameraSpinRotation, cameraOverheadRotation;
        TrinaryFloatControl cameraSpinRotationControl, cameraOverheadRotationControl;


        //
        // Render Options
        //
        UInt32 circleResolution = 16;


        readonly SquareBoundedPhysicalEntity[] squareBoundedEntities;
        readonly CircleBoundedPhysicalEntity[] circleBoundedEntities;

        public AISimulator(
            SquareBoundedPhysicalEntity[] squareBoundedEntities,
            CircleBoundedPhysicalEntity[] circleBoundedEntities
            )
            : base(800, 600, GraphicsMode.Default, "AI Simulator")
        {
            //
            // Camera Controls
            //
            cameraSpinRotation = 0;
            cameraSpinRotationControl = new TrinaryFloatControl(Key.Left, Key.Right, 1f, -45, 45);
            cameraOverheadRotation = 0;
            cameraOverheadRotationControl = new TrinaryFloatControl(Key.Down, Key.Up, 1f, 0, 80);

            //
            // Entities
            //
            this.squareBoundedEntities = squareBoundedEntities;
            this.circleBoundedEntities = circleBoundedEntities;
        }


        public override void Initialize(long nowMicros)
        {
            //
            // Initialize OpenGL
            //
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

        }
        public override bool Update(long nowMicros, int diffMicros)
        {
            //
            // Camera Controls
            //
            cameraSpinRotationControl.Update(Keyboard, ref cameraSpinRotation);
            cameraOverheadRotationControl.Update(Keyboard, ref cameraOverheadRotation);


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


            //
            // Draw entities
            //
            for (int i = 0; i < squareBoundedEntities.Length; i++)
            {
                SquareBoundedPhysicalEntity entity = squareBoundedEntities[i];
                entity.DrawOpenGLDouble();
            }
            for (int i = 0; i < circleBoundedEntities.Length; i++)
            {
                CircleBoundedPhysicalEntity entity = circleBoundedEntities[i];
                entity.DrawOpenGLDouble();
            }

            GL.PopMatrix();
        }
    }
}
