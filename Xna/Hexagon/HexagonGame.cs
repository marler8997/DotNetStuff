using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Marler.Common;
using Marler.Xna.Common;

namespace Marler.Xna
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class HexagonGame : Game
    {
        const Int32 PlayerPositionRange = 75; // Preferably power of 2 to help division
        const Int32 playerIconWidth = 30;
        const Int32 obstacleStepGenerationBuffer = 10000;


        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;


        FpsMonitor fpsMonitor = new FpsMonitor(32);


        //
        // Controls
        //
        CustomKeyboardMapping keyboardMapping;

        TrinaryInt32Control playerPositionControl;
        Int32 playerPosition;

        //
        // Fonts
        //
        SpriteFont miramonte18Font;



        //
        //
        //
        Int64 startTime;
        Int64 lastObstacleUpdateTime;

        //
        //
        //
        Color backgroundColor;


        //
        //
        //
        Int32 viewWidth, viewHeight, viewWidthHalf, viewHeightHalf;
        Int32 maxWidthOrHeight, maxWidthOrHeightHalf;
        Matrix windowCenterTranslationMatrix;

        Matrix gameWorldRotationMatrix = Matrix.Identity;
        Matrix gameWorldMatrix;

        //
        //
        //
        readonly List<Obstacle> obstacles = new List<Obstacle>();
        readonly List<UInt32> obstacleDistances = new List<UInt32>();

        public HexagonGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //
            // Set Maximum Fps
            //
            TargetElapsedTime = TimeSpan.FromSeconds(.01f);
            graphics.SynchronizeWithVerticalRetrace = false;
        }



        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);


            //
            //
            //
            startTime = Stopwatch.GetTimestamp();
            lastObstacleUpdateTime = startTime;


            //
            // Add first obstacles
            //
            GenerateObstacles();

            //
            // Load Controls
            //
            this.playerPositionControl = new TrinaryInt32Control(Keys.Left, Keys.Right, 1, 0, PlayerPositionRange - 1, true);

            //
            // Load Fonts
            //
            this.miramonte18Font = Content.Load<SpriteFont>("Miramonte18");
        }

        protected override void UnloadContent()
        {
        }



        protected override void Update(GameTime gameTime)
        {
            Int64 now = Stopwatch.GetTimestamp();

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            //
            // Update controls
            //
            KeyboardState keyboardState = Keyboard.GetState();

            playerPositionControl.Update(keyboardState, ref playerPosition);

            //
            // Update Background Color
            //
            backgroundColor = new Color((Int32)((gameTime.TotalGameTime.Seconds * 100) & 0xFF), 100, 100);
            
            //
            // Create Center Translation Matrix
            //
            if (viewWidth != GraphicsDevice.Viewport.Width || viewHeightHalf != GraphicsDevice.Viewport.Height)
            {
                // Window size changed
                viewWidth = GraphicsDevice.Viewport.Width;
                viewWidthHalf = viewWidth / 2;

                viewHeight = GraphicsDevice.Viewport.Height;
                viewHeightHalf = viewHeight / 2;

                maxWidthOrHeight = (viewWidth >= viewHeight) ? viewWidth : viewHeight;
                maxWidthOrHeightHalf = maxWidthOrHeight / 2;

                windowCenterTranslationMatrix = Matrix.CreateTranslation(viewWidthHalf, viewHeightHalf, 0);
            }

            //
            // Update Game World rotation
            //            
            //gameWorldRotationMatrix = gameWorldRotationMatrix * Matrix.CreateRotationZ((float)gameTime.ElapsedGameTime.Ticks * .00000001f);

            gameWorldMatrix = gameWorldRotationMatrix * windowCenterTranslationMatrix;






            //
            // Add new Update Obstacles
            //
            while (lastObstacleUpdateTime < now)
            {
                for (int i = 0; i < obstacles.Count; i++)
                {
                    obstacleDistances[i] -= 1;
                }

                //
                // Check if first obstacle must be removed
                //
                if (obstacles.Count > 0 && obstacleDistances[0] <= 0)
                {
                    obstacles.RemoveAt(0);
                    obstacleDistances.RemoveAt(0);
                }

                //
                // Keep adding obstacles till distance is good
                //
                GenerateObstacles();

                lastObstacleUpdateTime += StopwatchExtensions.MillisToStopwatchTicks(1);
            }

        }


        void GenerateObstacles()
        {
            while (true)                
            {
                UInt32 furthestObstacleStepDistance;
                if (obstacleDistances.Count <= 0)
                {
                    furthestObstacleStepDistance = 0;
                }
                else
                {
                    furthestObstacleStepDistance = obstacleDistances[obstacleDistances.Count - 1];
                    if (furthestObstacleStepDistance > obstacleStepGenerationBuffer) return;
                }

                obstacles.Add(Obstacle.Random(8));
                obstacleDistances.Add(furthestObstacleStepDistance + 500);
            }

        }


        protected override void Draw(GameTime gameTime)
        {



            //
            // Create Center Translation Matrix
            //
            Int32 viewWidth = GraphicsDevice.Viewport.Width;
            Int32 viewWidthHalf = viewWidth / 2;

            Int32 viewHeight = GraphicsDevice.Viewport.Height;
            Int32 viewHeightHalf = viewHeight / 2;

            //Matrix centerTranslationMatrix = Matrix.CreateTranslation(viewWidthHalf, viewHeightHalf, 0);
            

            fpsMonitor.Frame();

            GraphicsDevice.Clear(backgroundColor);

            
            //
            // Draw Lines
            //
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, gameWorldMatrix);
            spriteBatch.DrawLine(-maxWidthOrHeight, 0, maxWidthOrHeight, 0, Color.White); // Horizontal Line
            spriteBatch.DrawLine(0, -maxWidthOrHeight, 0, maxWidthOrHeight, Color.White);  // Vertical Line
            spriteBatch.End();
            

            //
            // Draw Obstacles
            //
            for(int i = 0; i < obstacles.Count; i++)
            {
                Obstacle obstacle = obstacles[i];
                UInt32 stepDistanceToObstacle = obstacleDistances[i];

                obstacle.Draw(spriteBatch, gameWorldMatrix, stepDistanceToObstacle);
            }            
            
            //
            // Draw Player Position
            //

            // Get player angle
            float angle = (float)(playerPosition * 2 * Math.PI) / (float)PlayerPositionRange;
            
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null,
                 Matrix.CreateTranslation(-playerIconWidth / 2, -85, 0) * Matrix.CreateRotationZ(angle) * gameWorldMatrix);
            spriteBatch.FillRectangle(new Vector2(0, 0), new Vector2(playerIconWidth, 20), Color.White, 0);
            spriteBatch.End();

            /*
            spriteBatch.GraphicsDevice.FillQuad(
                new Vector2(0, 0),
                new Vector2(0, 10),
                new Vector2(10, 10),
                new Vector2(10, 0),
                Color.White);
            */
            spriteBatch.GraphicsDevice.FillQuad(
                new Vector2(-.2f, -.2f),
                new Vector2(-.2f, 0),
                new Vector2(0, 0),
                new Vector2(0, -.2f), Matrix.CreateRotationZ(angle),
                Color.White);
            
            //
            // Draw Debug Messages
            //
            spriteBatch.Begin();
            spriteBatch.DrawString(miramonte18Font, String.Format("FPS {0} PlayerPosition {1,-3} Angle {2} BackColor {3}",
                fpsMonitor.CalculateFps(), playerPosition, angle, backgroundColor),
                new Vector2(10, 20), Color.Black);
            spriteBatch.End();
        }
    }
}
