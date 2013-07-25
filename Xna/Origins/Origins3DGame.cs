using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Marler.Xna.Common;

namespace Marler.Xna.Origins
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Origins3DGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;


        //
        // Controls
        //
        CustomKeyboardMapping keyboardMapping;



        //
        // Matrices
        //
        readonly Matrix view, projection;

        //
        // World Grid
        //
        Grid worldGrid;


        //
        // Fonts
        //
        SpriteFont miramonte18Font;

        //
        // Primitives
        //
        VertexBuffer floorVertexBuffer;
        Int32 floorTriangleCount;
        BasicEffect floorBasicEffect;
        Matrix floorWorld;

        VertexBuffer octahedronVertexBuffer;
        Int32 octahedronTriangleCount;
        BasicEffect octahedronBasicEffect;
        Matrix octahedronWorld;

        VertexBuffer icosahedronVertexBuffer;
        IndexBuffer icosahedronIndexBuffer;
        Int32 icosahedronVertexCount, icosahedronTriangleCount;
        BasicEffect icosahedronBasicEffect;
        Matrix icosahedronWorldPosition;
        Matrix icosahedronWorldRotation;



        //
        // Models
        //
        Model spaceShip;
        Vector3 spaceShipPosition;

        Model helicopterModel;
        float mainRotorAngle, tailRotorAngle;
        Vector3 helicopterPosition;
        float helicopterAngle;
        Matrix helicopterWorld;
        float helicopterLeftRight, helicopterFowardBackward, helicopterUpDown;
        TrinaryFloatControl leftRightControl, forwardBackwardControl, upDownControl;


        public Origins3DGame()
        {
            IsMouseVisible = true;

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            
            
            //this.world = Matrix.CreateTranslation(0, 0, 0);
            this.view = Matrix.CreateLookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY);

            float aspectRatio = graphics.PreferredBackBufferWidth /
                                         graphics.PreferredBackBufferHeight;
            this.projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45),
                aspectRatio, .1f, 100f);




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
            // Load World Grid
            //
            this.worldGrid = new Grid(this);


            //
            // Load Fonts
            //
            this.miramonte18Font = Content.Load<SpriteFont>("Miramonte18");


            //
            // Load Textures
            //


            //
            // Load Primitives
            //
            VertexPositionColor[] floorVertices = new VertexPositionColor[] {
                new VertexPositionColor(Vector3.UnitY, Color.Red),
                new VertexPositionColor(new Vector3( .5f, 0, 0), Color.Green),
                new VertexPositionColor(new Vector3(-.5f, 0, 0), Color.Blue),
            };
            floorVertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), floorVertices.Length, BufferUsage.WriteOnly);
            floorVertexBuffer.SetData<VertexPositionColor>(floorVertices);
            floorTriangleCount = floorVertices.Length / 3;
            floorBasicEffect = new BasicEffect(GraphicsDevice);
            //floorBasicEffect.EnableDefaultLighting();
            floorWorld = Matrix.CreateTranslation(new Vector3(0, 1.5f, 0));

            
            VertexPositionColor[] octahedronVertices = new VertexPositionColor[] {
                new VertexPositionColor(new Vector3(1f, 0, 0), Color.Red),
                new VertexPositionColor(new Vector3(0, -1, 0), Color.Blue),
                new VertexPositionColor(new Vector3(0, 0, 1), Color.Green),
                new VertexPositionColor(new Vector3(1, 0, 0), Color.Blue),
                new VertexPositionColor(new Vector3(0, 1, 0), Color.Red),
                new VertexPositionColor(new Vector3(0, 0, -1), Color.Green),
                new VertexPositionColor(new Vector3(-1, 0, 0), Color.Red),
                new VertexPositionColor(new Vector3(0, 1, 0), Color.Blue),
                new VertexPositionColor(new Vector3(0, 0, 1), Color.Green),
                new VertexPositionColor(new Vector3(-1, 0, 0), Color.Blue),
                new VertexPositionColor(new Vector3(0, -1, 0), Color.Red),
                new VertexPositionColor(new Vector3(0, 0, -1), Color.Blue),
                new VertexPositionColor(new Vector3(0, 1, 0), Color.Green),
                new VertexPositionColor(new Vector3(1, 0, 0), Color.Blue),
                new VertexPositionColor(new Vector3(0, 0, 1), Color.Red),
                new VertexPositionColor(new Vector3(0, 1, 0), Color.Blue),
                new VertexPositionColor(new Vector3(-1, 0, 0), Color.Green),
                new VertexPositionColor(new Vector3(0, 0, -1), Color.Blue),
                new VertexPositionColor(new Vector3(0, -1, 0), Color.Red),
                new VertexPositionColor(new Vector3(-1, 0, 0), Color.Blue),
                new VertexPositionColor(new Vector3(0, 0, 1), Color.Green),
                new VertexPositionColor(new Vector3(0, -1, 0), Color.Blue),
                new VertexPositionColor(new Vector3(1, 0, 0), Color.Red),
                new VertexPositionColor(new Vector3(0, 0, -1), Color.Blue),
            };
            octahedronVertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), octahedronVertices.Length, BufferUsage.WriteOnly);
            octahedronVertexBuffer.SetData<VertexPositionColor>(octahedronVertices);
            octahedronTriangleCount = octahedronVertices.Length / 3;
            octahedronBasicEffect = new BasicEffect(GraphicsDevice);
            //octahedronrBasicEffect.EnableDefaultLighting();
            octahedronWorld = Matrix.CreateTranslation(new Vector3(0, 0, 0));




            VertexPositionColor[] icosahedronVertices = new VertexPositionColor[] {
                new VertexPositionColor(new Vector3(-0.26286500f, 0.0000000f, 0.42532500f), Color.Red),
                new VertexPositionColor(new Vector3(0.26286500f, 0.0000000f, 0.42532500f), Color.Orange),
                new VertexPositionColor(new Vector3(-0.26286500f, 0.0000000f, -0.42532500f), Color.Yellow),
                new VertexPositionColor(new Vector3(0.26286500f, 0.0000000f, -0.42532500f), Color.Green),
                new VertexPositionColor(new Vector3(0.0000000f, 0.42532500f, 0.26286500f), Color.Blue),
                new VertexPositionColor(new Vector3(0.0000000f, 0.42532500f, -0.26286500f), Color.Indigo),
                new VertexPositionColor(new Vector3(0.0000000f, -0.42532500f, 0.26286500f), Color.Purple),
                new VertexPositionColor(new Vector3(0.0000000f, -0.42532500f, -0.26286500f), Color.White),
                new VertexPositionColor(new Vector3(0.42532500f, 0.26286500f, 0.0000000f), Color.Cyan),
                new VertexPositionColor(new Vector3(-0.42532500f, 0.26286500f, 0.0000000f), Color.Black),
                new VertexPositionColor(new Vector3(0.42532500f, -0.26286500f, 0.0000000f), Color.DodgerBlue),
                new VertexPositionColor(new Vector3(-0.42532500f, -0.26286500f, 0.0000000f), Color.Crimson),
            };
            icosahedronVertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), icosahedronVertices.Length, BufferUsage.WriteOnly);
            icosahedronVertexBuffer.SetData<VertexPositionColor>(icosahedronVertices);
            icosahedronVertexCount = icosahedronVertices.Length;

            UInt16[] icosahedronIndices = new UInt16[] {
                0, 6, 1,
                0, 11, 6,
                1, 4, 0,
                1, 8, 4,
                1, 10, 8,
                2, 5, 3,
                2, 9, 5,
                2, 11, 9,
                3, 7, 2,
                3, 10, 7,
                4, 8, 5,
                4, 9, 0,
                5, 8, 3,
                5, 9, 4,
                6, 10, 1,
                6, 11, 7,
                7, 10, 6,
                7, 11, 2,
                8, 10, 3,
                9, 11, 0,
            };
            icosahedronIndexBuffer = new IndexBuffer(GraphicsDevice, typeof(UInt16), icosahedronIndices.Length, BufferUsage.WriteOnly);
            icosahedronIndexBuffer.SetData(icosahedronIndices);
            icosahedronTriangleCount = icosahedronVertices.Length / 3;
            icosahedronBasicEffect = new BasicEffect(GraphicsDevice);
            icosahedronWorldPosition = Matrix.CreateTranslation(new Vector3(-1, -1, 5));
            icosahedronWorldRotation = Matrix.Identity;


            //
            // Load Models
            //
            this.spaceShip = Content.Load<Model>("SpaceShip");
            this.spaceShipPosition = new Vector3(0, 0, 0);

            this.helicopterModel = Content.Load<Model>("Helicopter");
            this.leftRightControl = new TrinaryFloatControl(Keys.A, Keys.D, .2f, -100f, 100f);
            this.forwardBackwardControl = new TrinaryFloatControl(Keys.W, Keys.S, .2f, -100f, 100f);
            this.upDownControl = new TrinaryFloatControl(Keys.Down, Keys.Up, .2f, -100f, 100f);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            //
            // Update Grid
            //
            worldGrid.Update();


            //
            // Update animations
            //
            octahedronWorld *= Matrix.CreateRotationX(.05f) * Matrix.CreateRotationZ(.01f);

            icosahedronWorldRotation *= Matrix.CreateRotationX(.05f) * Matrix.CreateRotationZ(.01f);


            //
            // Get Controls
            //
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();





            //
            // Update helicopter
            //
            tailRotorAngle -= .15f;
            mainRotorAngle -= .15f;

            leftRightControl.Update(keyboardState, ref helicopterLeftRight);
            forwardBackwardControl.Update(keyboardState, ref helicopterFowardBackward);
            upDownControl.Update(keyboardState, ref helicopterUpDown);

        }

        void DrawModel(Model model, Matrix objectWorld, Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();

                    //effect.FogEnabled = true;
                    //effect.FogColor = Color.CornflowerBlue.ToVector3(); // For best results, ake this color whatever your background is.
                    //effect.FogStart = 9.75f;
                    //effect.FogEnd = 10.25f;

                    effect.World = mesh.ParentBone.Transform * objectWorld;
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }
        }
        private void DrawModel(Model model, Matrix objectWorld, Matrix[] meshWorldMatrices, Matrix view, Matrix projection)
        {
            for (int index = 0; index < model.Meshes.Count; index++)
            {
                ModelMesh mesh = model.Meshes[index];
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;

                    effect.World = mesh.ParentBone.Transform * meshWorldMatrices[index] * objectWorld;
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }
        }

        void DrawTriangles(BasicEffect effect, Matrix world, VertexBuffer vertexBuffer, Int32 triangleCount)
        {
            effect.World = world;
            effect.View = view;
            effect.Projection = projection;
            effect.VertexColorEnabled = true;

            GraphicsDevice.SetVertexBuffer(vertexBuffer);

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, triangleCount);
            }
        }

        void DrawIndexedTriangles(BasicEffect effect, Matrix world, VertexBuffer vertexBuffer, IndexBuffer indexBuffer, Int32 vertexCount, Int32 triangleCount)
        {
            effect.World = world;
            effect.View = view;
            effect.Projection = projection;
            effect.VertexColorEnabled = true;

            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, triangleCount);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);



            //
            // Draw Models
            //
            spriteBatch.Begin();

            worldGrid.Draw(gameTime, view, projection);


            DrawTriangles(floorBasicEffect, floorWorld, floorVertexBuffer, floorTriangleCount);

            DrawTriangles(octahedronBasicEffect, octahedronWorld, octahedronVertexBuffer, octahedronTriangleCount);

            DrawIndexedTriangles(icosahedronBasicEffect, icosahedronWorldRotation * icosahedronWorldPosition, icosahedronVertexBuffer,
                icosahedronIndexBuffer, icosahedronVertexCount, icosahedronTriangleCount);


            //DrawModel(spaceShip, Matrix.CreateTranslation(spaceShipPosition), view, projection);
            //DrawModel(spaceShip, world, view, projection);
            //DrawModel(spaceShip, Matrix.CreateRotationX(-spaceShipPosition.Y) * Matrix.CreateRotationY(spaceShipPosition.X), view, projection);

            //DrawModel(helicopter, Matrix.CreateRotationX(-spaceShipPosition.Y) * Matrix.CreateRotationY(spaceShipPosition.X), view, projection);
            //DrawModel(helicopter, world, view, projection);


            Matrix[] meshWorldMatrices = new Matrix[3];
            meshWorldMatrices[0] = Matrix.CreateTranslation(Vector3.Zero);
            meshWorldMatrices[1] = Matrix.CreateRotationY(mainRotorAngle);
            meshWorldMatrices[2] = Matrix.CreateTranslation(new Vector3(0, -0.25f, -3.4f)) *
                                                        Matrix.CreateRotationX(tailRotorAngle) *
                                                        Matrix.CreateTranslation(new Vector3(0, 0.25f, 3.4f));

            helicopterWorld = Matrix.CreateRotationY(helicopterAngle) * Matrix.CreateTranslation(helicopterPosition);
            DrawModel(helicopterModel, Matrix.CreateTranslation(new Vector3(helicopterLeftRight, helicopterUpDown, helicopterFowardBackward)),
                meshWorldMatrices, view, projection);
            
            
            
            
            
            
            
            
            
            spriteBatch.End();


            spriteBatch.Begin();

            spriteBatch.DrawString(miramonte18Font, "Hello", Vector2.Zero, Color.Black);

            spriteBatch.End();

        }
    }
}
