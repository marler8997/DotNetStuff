using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Marler.OpenTK.Common;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Marler.Tank
{
    public class TankGame : CustomGameWindow
    {
        float cameraSpinRotation, cameraOverheadRotation;
        TrinaryFloatControl cameraSpinRotationControl, cameraOverheadRotationControl;

        //
        // Fonts
        //
        static readonly CharacterRenderMapFont smallFont = new CharacterRenderMapFont(
            CharacterRenderSetFactory.VariableLengthSegmentRenderers, 7, 14, 3);

        //
        // Render Components
        //
        readonly ArenaGLRenderer arenaRenderer;

        public TankGame(TankLevel level)
            : base(800, 600, GraphicsMode.Default, "Tank")
        {
            cameraSpinRotation = 0;
            cameraSpinRotationControl = new TrinaryFloatControl(Key.Left, Key.Right, 1f, -45, 45);
            cameraOverheadRotation = 0;
            cameraOverheadRotationControl = new TrinaryFloatControl(Key.Down, Key.Up, 1f, 0, 80);

            //
            // Render Components
            //
            ArenaUtils.windowWidth = ClientRectangle.Width;
            ArenaUtils.windowHeight = ClientRectangle.Height;
            ArenaUtils.arenaWidth = level.width;
            ArenaUtils.arenaHeight = level.height;

            ArenaUtils.glOrthoHalfWidth  = (float)ArenaUtils.windowWidth / 2f;
            ArenaUtils.glOrthoHalfHeight = (float)ArenaUtils.windowHeight / 2f;
            ArenaUtils.glOrthoWidth      = ArenaUtils.windowWidth;
            ArenaUtils.glOrthoHeight     = ArenaUtils.windowHeight;
            ArenaUtils.glArenaHalfWidth  = ArenaUtils.glOrthoWidth * .45f;
            ArenaUtils.glArenaHalfHeight = ArenaUtils.glOrthoHeight * .45f;
            ArenaUtils.glArenaWidth      = ArenaUtils.glOrthoWidth * .9f;
            ArenaUtils.glArenaHeight     = ArenaUtils.glOrthoHeight * .9f;

            arenaRenderer = new ArenaGLRenderer(this);
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
            GL.Ortho(-ArenaUtils.glOrthoHalfWidth, ArenaUtils.glOrthoHalfWidth,
                    -ArenaUtils.glOrthoHalfHeight, ArenaUtils.glOrthoHalfHeight,
                    -ArenaUtils.glOrthoHalfWidth, ArenaUtils.glOrthoHalfWidth);


            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 modelview = Matrix4.LookAt(Vector3.Zero, new Vector3(0f, 0f, -1f)/*Vector3.UnitZ*/, Vector3.UnitY);
            GL.LoadMatrix(ref modelview);





            GL.PushMatrix();

            // Translate origin to center of arena for rotation
            GL.Rotate(cameraSpinRotation, 0, 1, 0);
            GL.Rotate(cameraOverheadRotation, 1, 0, 0);

            // Draw the floor
            /*
            GL.Enable(GL_TEXTURE_2D);
            GL.BindTexture(GL_TEXTURE_2D, floorTexture.textureID);
            GL.Begin(GL_QUADS);
            GL.Color3f(1, 1, 1);
            GL.TexCoord2f(0, 0); GL.Vertex3f(-ArenaUtils.windowWidth / 2 - 100, -ArenaUtils.windowHeight / 2 - 100, 0);
            GL.TexCoord2f(0, 1); GL.Vertex3f(-ArenaUtils.windowWidth / 2 - 100, ArenaUtils.windowHeight / 2 + 100, 0);
            GL.TexCoord2f(1, 1); GL.Vertex3f(ArenaUtils.windowWidth / 2 + 100, ArenaUtils.windowHeight / 2 + 100, 0); GL.Color3f(.9f, .9f, .9f);
            GL.TexCoord2f(1, 0); GL.Vertex3f(ArenaUtils.windowWidth / 2 + 100, -ArenaUtils.windowHeight / 2 - 100, 0);
            GL.End();
            GL.Disable(GL_TEXTURE_2D);
            */


            // Draw the bottom/back of the arena
            arenaRenderer.Draw();

            // Draw the puck(s)
            /*
            for (int i = 0; i < clients.length; i++)
            {
                clients[i].puck.GL.Draw();
            }
            
            //
            // Draw Last Click
            //
            GL.Begin(GL_QUADS);

            GL.Color4f(.8f, 0, 0, .4f);
            float glX = ArenaUtils.arenaToGLX(this.lastClickArenaX);
            float glY = ArenaUtils.arenaToGLY(this.lastClickArenaY);
            GL.Vertex3f(glX - 5, glY + 5, Settings.puckGLHeight);
            GL.Vertex3f(glX + 5, glY + 5, Settings.puckGLHeight);
            GL.Vertex3f(glX + 5, glY - 5, Settings.puckGLHeight);
            GL.Vertex3f(glX - 5, glY - 5, Settings.puckGLHeight);
            GL.End();


            //
            // Draw mouse position
            //		
            Vector2f mouseGLXY = mouseToGLXY(Mouse.getX(), Mouse.getY());
            GL.Begin(GL_QUADS);
            GL.Color4f(.8f, 0, 0, .4f);
            GL.Vertex3f(mouseGLXY.x - 11, mouseGLXY.y + 7, Settings.puckGLHeight);
            GL.Vertex3f(mouseGLXY.x - 7, mouseGLXY.y + 11, Settings.puckGLHeight);
            GL.Vertex3f(mouseGLXY.x + 11, mouseGLXY.y - 7, Settings.puckGLHeight);
            GL.Vertex3f(mouseGLXY.x + 7, mouseGLXY.y - 11, Settings.puckGLHeight);

            GL.Vertex3f(mouseGLXY.x + 7, mouseGLXY.y + 11, Settings.puckGLHeight);
            GL.Vertex3f(mouseGLXY.x + 11, mouseGLXY.y + 7, Settings.puckGLHeight);
            GL.Vertex3f(mouseGLXY.x - 7, mouseGLXY.y - 11, Settings.puckGLHeight);
            GL.Vertex3f(mouseGLXY.x - 11, mouseGLXY.y - 7, Settings.puckGLHeight);

            float myPuckGLX = ArenaUtils.arenaToGLX(myPuck.tankArenaObject.getMiddleX());
            float myPuckGLY = ArenaUtils.arenaToGLY(myPuck.tankArenaObject.getMiddleY());

            float changeInX = myPuckGLX - mouseGLXY.x;
            float changeInY = myPuckGLY - mouseGLXY.y;
            float vectorLength = (float)Math.sqrt(changeInX * changeInX + changeInY * changeInY);

            changeInX /= vectorLength;
            changeInY /= vectorLength;

            float side1AtTankX = myPuckGLX - changeInY * Settings.tankToCursorLineWidth;
            float side1AtTankY = myPuckGLY + changeInX * Settings.tankToCursorLineWidth;
            float side2AtTankX = myPuckGLX + changeInY * Settings.tankToCursorLineWidth;
            float side2AtTankY = myPuckGLY - changeInX * Settings.tankToCursorLineWidth;

            float side1AtMouseX = mouseGLXY.x - changeInY * Settings.tankToCursorLineWidth;
            float side1AtMouseY = mouseGLXY.y + changeInX * Settings.tankToCursorLineWidth;
            float side2AtMouseX = mouseGLXY.x + changeInY * Settings.tankToCursorLineWidth;
            float side2AtMouseY = mouseGLXY.y - changeInX * Settings.tankToCursorLineWidth;

            GL.Vertex3f(side1AtTankX, side1AtTankY, Settings.puckGLHeight);
            GL.Vertex3f(side2AtTankX, side2AtTankY, Settings.puckGLHeight);
            GL.Vertex3f(side2AtMouseX, side2AtMouseY, Settings.puckGLHeight);
            GL.Vertex3f(side1AtMouseX, side1AtMouseY, Settings.puckGLHeight);

            GL.End();
            */

            GL.PopMatrix();

            //
            // Draw Debug Information
            //
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Ortho(0, ClientRectangle.Width, 0, ClientRectangle.Height, -100, 100);

            GL.Begin(BeginMode.Quads);
            GL.Color4(0f, 0f, 0f, .6f);
            GL.Vertex2(5, 5);
            GL.Vertex2(5, 28);
            GL.Vertex2(ClientRectangle.Width - 5, 28);
            GL.Vertex2(ClientRectangle.Width - 5, 5);
            GL.End();

            averageUpdateTimeDiffMicros.CalulateAverage();
            long averageUpdateTimeDiffMicrosValue = (long)averageUpdateTimeDiffMicros.LastAverageCalculated;

            
            GL.Color3(1f, 1f, 1f);
            smallFont.Draw(String.Format("FPS Max {0:F1} Avg:{1:F1}",
                1000f / (float)minMillisPerFrame,
                1000000f / (float)averageUpdateTimeDiffMicrosValue), 10, 10);
        }
    }
}
