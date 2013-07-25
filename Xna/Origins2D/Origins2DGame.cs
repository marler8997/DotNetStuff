using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Marler.Common;
using Marler.Xna.Common;

namespace Marler.Xna.Origins2D
{
    class Origins2DGame : Game
    {
        public static readonly Random random = new Random();

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        FpsMonitor fpsMonitor = new FpsMonitor(32);
        FpsMonitor updatesPerSecondMonitor = new FpsMonitor(32);

        //
        // Controls
        //
        CustomKeyboardMapping keyboardMapping;

        TrinaryInt32Control cameraXControl, cameraYControl;

        //
        // Matrices
        //

        //
        // Universe Parameters
        //
        Int32 minX, maxX, width, minY, maxY, height;
        enum UniverseEdgeRules
        {
            Reflect,
            WrapAround,
        }
        UniverseEdgeRules universeEdgeRule;


        Int32 updatesPerFrame;

        Int32 cameraX, cameraY;
        Int32 pixelsPerDot;



        //
        // World Grid
        //
        Int32 gridWidth, gridHeight;

        //
        // Fonts
        //
        SpriteFont miramonte18Font;

        //
        // Primitives
        //
        List<Dot> dots = new List<Dot>();


        //
        // Models
        //

        public Origins2DGame()
        {
            IsMouseVisible = true;

            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {



            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Viewport viewport = GraphicsDevice.Viewport;
            Int32 viewWidth = viewport.Width;
            Int32 viewHeight = viewport.Height;

            //
            // Universe Parameters
            //
            minX = -375;
            maxX = 375;
            width = maxX - minX;
            minY = -220;
            maxY = 220;
            height = maxY - minY;
            universeEdgeRule = UniverseEdgeRules.Reflect;
            
            updatesPerFrame = 100;


            cameraX = -390;
            cameraY = -235;
            pixelsPerDot = 1;

            
            
            //
            // Controls
            //
            this.cameraXControl = new TrinaryInt32Control(Keys.A, Keys.D, 1, (Int32)minX - 50, (Int32)maxX + 50);
            this.cameraYControl = new TrinaryInt32Control(Keys.W, Keys.S, 1, (Int32)minY - 50, (Int32)maxY + 50);


            //
            // Load World Grid
            //
            gridWidth = 20;
            gridHeight = 20;

            //
            // Load Fonts
            //
            this.miramonte18Font = Content.Load<SpriteFont>("Miramonte18");

            
            //
            // Create random dots
            //
            for (int i = 0; i < 10; i++)
            {
                dots.Add(new Dot(random.Next((Int32)width) + minX, random.Next((Int32)height) + minY,
                    (random.Next(3)-1) * 800,(random.Next(3)-1) * 800));
            }


            //
            // Load Textures
            //


            //
            // Load Primitives
            //


            //
            // Load Models
            //



        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            updatesPerSecondMonitor.Frame();

            //
            // Get Controls
            //
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();


            //
            // Update Camera Controls
            //
            cameraXControl.Update(keyboardState, ref cameraX);
            cameraYControl.Update(keyboardState, ref cameraY);

            //
            // Update Grid
            //




            for (int updateCount = 0; updateCount < updatesPerFrame; updateCount++)
            {

                //
                // Update Velocities from gravity
                //
                for (int i = 0; i < dots.Count; i++)
                {
                    Dot dot = dots[i];

                    for (int j = 0; j < dots.Count; j++)
                    {
                        if (j == i) continue;

                        Dot compare = dots[j];

                        Int32 xDiff = compare.x - dot.x;
                        Int32 yDiff = compare.y - dot.y;
                        Int32 distanceSquared = xDiff * xDiff + yDiff * yDiff;

                        Int32 force = 50 / distanceSquared;

                        Int32 xForce = xDiff * force;
                        Int32 yForce = yDiff * force;
                        /*
                        if (xForce != 0)
                        {
                            Console.WriteLine("xForce = {0}", xForce);
                            compare.speedInverseX -= xForce;
                            dot.speedInverseX += xForce;
                        }

                        compare.speedInverseY -= yForce;
                        dot.speedInverseY += yForce;
                        */
                    }
                }

                //
                // Update Positions
                //
                for (int i = 0; i < dots.Count; i++)
                {
                    Dot dot = dots[i];

                    dot.Update();

                    //
                    // Fix Edge of Universe
                    //
                    if (dot.x < minX)
                    {
                        if (universeEdgeRule == UniverseEdgeRules.Reflect)
                        {
                            dot.x = 2 * minX - dot.x;
                            dot.updatesPer8DotsX *= -1;
                        }
                        else
                        {
                            throw new InvalidOperationException(String.Format("Unhandled Universe Edge Rule '{0}'", universeEdgeRule));
                        }
                    }
                    else if (dot.x > maxX)
                    {
                        if (universeEdgeRule == UniverseEdgeRules.Reflect)
                        {
                            dot.x = 2 * maxX - dot.x;
                            dot.updatesPer8DotsX *= -1;
                        }
                        else
                        {
                            throw new InvalidOperationException(String.Format("Unhandled Universe Edge Rule '{0}'", universeEdgeRule));
                        }
                    }

                    if (dot.y < minY)
                    {
                        if (universeEdgeRule == UniverseEdgeRules.Reflect)
                        {
                            dot.y = 2 * minY - dot.y;
                            dot.updatesPer8DotsY *= -1;
                        }
                        else
                        {
                            throw new InvalidOperationException(String.Format("Unhandled Universe Edge Rule '{0}'", universeEdgeRule));
                        }
                    }
                    else if (dot.y > maxY)
                    {
                        if (universeEdgeRule == UniverseEdgeRules.Reflect)
                        {
                            dot.y = 2 * maxY - dot.y;
                            dot.updatesPer8DotsY *= -1;
                        }
                        else
                        {
                            throw new InvalidOperationException(String.Format("Unhandled Universe Edge Rule '{0}'", universeEdgeRule));
                        }
                    }
                }

            }

            //
            // Update animations
            //
        

        }






        protected override void Draw(GameTime gameTime)
        {
            fpsMonitor.Frame();
            GraphicsDevice.Clear(Color.CornflowerBlue);


            Viewport viewport = GraphicsDevice.Viewport;
            Int32 viewWidth = viewport.Width;
            Int32 viewHeight = viewport.Height;








            //
            // Draw edges of the universe
            //
            spriteBatch.Begin();

            // left
            spriteBatch.FillRectangle(
                new Vector2((minX - 5 - cameraX) * pixelsPerDot, (minY - 5 - cameraY) * pixelsPerDot),
                new Vector2(5 * pixelsPerDot, (height + 10) * pixelsPerDot),
                Color.Gray);
            // right
            spriteBatch.FillRectangle(
                new Vector2((maxX - cameraX) * pixelsPerDot, (minY - 5 - cameraY) * pixelsPerDot),
                new Vector2(5 * pixelsPerDot, (height + 10) * pixelsPerDot),
                Color.Gray);
            // top
            spriteBatch.FillRectangle(
                new Vector2((minX - cameraX) * pixelsPerDot, (minY - 5 - cameraY) * pixelsPerDot),
                new Vector2(width * pixelsPerDot, 5 * pixelsPerDot),
                Color.Gray);
            // bottom
            spriteBatch.FillRectangle(
                new Vector2((minX - cameraX) * pixelsPerDot, (maxY - cameraY) * pixelsPerDot),
                new Vector2(width * pixelsPerDot, 5 * pixelsPerDot),
                Color.Gray);
            spriteBatch.End();



            spriteBatch.Begin();

            // draw grid
            Vector2 one,two;
            one.Y = (minY - cameraY) * pixelsPerDot;
            two.Y = (maxY - cameraY) * pixelsPerDot;
            for (int i = (int)minX; i <= (int)maxX; i += gridWidth)
            {
                one.X = (i - cameraX) * pixelsPerDot;
                two.X = (i - cameraX) * pixelsPerDot;
                spriteBatch.DrawLine(one, two, Color.Black);
            }

            one.X = (minX - cameraX) * pixelsPerDot;
            two.X = (maxX - cameraX) * pixelsPerDot;
            for (int i = (int)minY; i <= (int)maxY; i += gridHeight)
            {
                one.Y = (i - cameraY) * pixelsPerDot;
                two.Y = (i - cameraY) * pixelsPerDot;
                spriteBatch.DrawLine(one, two, Color.Black);
            }

            spriteBatch.End();


            spriteBatch.Begin();

            if (pixelsPerDot == 1)
            {
                Vector2 vector;
                for (int i = 0; i < dots.Count; i++)
                {
                    Dot dot = dots[i];
                    vector.X = (dot.x - cameraX) * pixelsPerDot;
                    vector.Y = (dot.y - cameraY) * pixelsPerDot;
                    spriteBatch.Draw(Primitives2D.pixel, vector, Color.White);
                }
            }
            else if (pixelsPerDot > 1)
            {
                Rectangle rectangle;
                for (int i = 0; i < dots.Count; i++)
                {
                    Dot dot = dots[i];
                    rectangle.X = ((Int32)dot.x - cameraX) * pixelsPerDot;
                    rectangle.Y = ((Int32)dot.y - cameraY) * pixelsPerDot;
                    rectangle.Width = pixelsPerDot;
                    rectangle.Height = pixelsPerDot;
                    spriteBatch.FillRectangle(rectangle, Color.White);
                }
            }

            spriteBatch.End();

            spriteBatch.Begin();
            spriteBatch.DrawString(miramonte18Font, String.Format("Camera {0}x{1} FPS {2} UPS {3}", cameraX, cameraY,
                fpsMonitor.CalculateFps(), updatesPerSecondMonitor.CalculateFps()),new Vector2(0, 0), Color.Black);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
 }
