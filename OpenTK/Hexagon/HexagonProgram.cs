using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

using Marler.Audio;
using More;
using More.OpenTK;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Input;
using System.IO;
using System.Collections.Generic;

namespace Marler.Games.Hexagon
{
    class HexagonGame : CustomGameWindow
    {
        public static readonly Random random = new Random();

        //const Int32 PlayerAngleMinimumDegreeChange = 8;
        const float DegreesPerMicrosecond = .00046f;
        const float MinimumBarrierGenerationBuffer = 1500;


        const float BarrierCollisionHeight = 56;

        const float standardBarrierDistancePerMicrosecond = .0008f;
        const float maximumBarrierDistancePerMicrosecond = .0015f; // Calculated by knowing how long it takes for a barrier to enter view and reach
                                                                   // the player...the player must be able to turn 180 degrees in that time.

        float barrierDistancePerMicrosecond = .0010f;


        //
        // User Interface HUD Component
        //
        Label gameStatsLabel;
        Component userInterfaceHud;


        //
        // Fonts
        //
        static readonly CharacterRenderMapFont gameFont = new CharacterRenderMapFont(
            CharacterRenderSetFactory.VariableLengthSegmentRenderers, 21, 42, 6);
        static readonly CharacterRenderMapFont smallFont = new CharacterRenderMapFont(
            CharacterRenderSetFactory.VariableLengthSegmentRenderers, 7, 14, 3);

        //
        // Background Color
        //
        ColorTimeChanger backgroundColorChanger = new ColorTimeChanger(new Color4(.5f, .2f, .1f, 1f));
        Color4 currentBackgroundColor, currentBarrierColor;

        //
        // World Shif Variables
        //
        float worldRotationX, worldRotationY;

        LinearPulser worldZoomPulser;
        public float worldZoom;

        float worldSpinAngle;
        float currentWorldSpinRotationsPerSecond;
        public float microsLeftOfCurrentWorldSpinVelocity;

        //
        // Controls
        //
        readonly TrinaryControl playerAngleControl;
        float playerAngle;
        byte currentPlayerRegion;

        //
        //
        //
        System.Boolean playerCollision;

        //
        //
        //
        //readonly TrinaryInt32Control adControl;
        //Int32 adValue;
        //readonly TrinaryInt32Control wsControl;
        //Int32 wsValue;
        TrinaryControl minMillisPerFrameControl;


        //
        // Barriers
        //
        Byte currentRegionCount = 7;
        readonly List<Barrier> barriers = new List<Barrier>();

        //
        //
        //
        Int64 lastNonCollisionTimeMicros;
        Int64 nonCollisionMicros;
        Int64 maxNonCollisionMicros;



        /// <summary>Creates a 800x600 window with the specified title.</summary>
        public HexagonGame()
            : base(800, 700, GraphicsMode.Default, "Hexagon")
        {
            //
            // Enable Transparency
            //
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            //VSync = VSyncMode.On;

            this.playerAngleControl = new TrinaryControl(Key.Left, Key.Right);
            //this.adControl = new TrinaryInt32Control(Key.A, Key.D, 0, 100, false);
            //this.wsControl = new TrinaryInt32Control(Key.S, Key.W, 1, 200, false);
            this.minMillisPerFrameControl = new TrinaryControl(Key.W, Key.S);


            //
            // User Interface HUD Component
            //
            gameStatsLabel = new Label(smallFont);
            gameStatsLabel.SetComponentLocation(10, 10);
            gameStatsLabel.fontColor = Color.White;

            userInterfaceHud = gameStatsLabel;


            //
            //
            //
            this.playerCollision = false;


            GenerateBarriers();


            GLException.ThrowOnError();            
        }

        public void GenerateBarriers()
        {
            //
            // Generate Barriers if needed
            //
            while (true)
            {
                if (barriers.FarthestDistance() >= MinimumBarrierGenerationBuffer)
                {
                    break;
                }
                BarrierGeneration.GenerateBarriers(barriers, currentRegionCount, barrierDistancePerMicrosecond);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            //base.OnResize(e);
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            //gameStatsLabel.SetY(ClientRectangle.Height - 10);
        }


        public override void Initialize(long nowMicros)
        {
            //
            // Initialize Collision Variables
            //
            this.lastNonCollisionTimeMicros = nowMicros;



            //
            // Initialize World Shift Variables
            //
            //this.worldZoomPulser = new LinearPulser(500 * 1000, 100 * 1000);
            this.worldZoomPulser = LinearPulser.CreatePulsesPerMinute(142, 100 * 1000);
            this.worldZoomPulser.PulseValue(250000); // Set initial offset to sync with music
            this.worldZoom = 0;

            this.worldSpinAngle = 0;
            this.microsLeftOfCurrentWorldSpinVelocity = 0;
        }

        // return true to exit
        public override System.Boolean Update(Int64 nowMicros, Int32 diffMicros)
        {
            if (Keyboard[Key.Escape]) return true;
            //
            // Controls
            //
            Int32 minMillisPerFrameControlValue = minMillisPerFrameControl.GetControl(Keyboard);
            if (minMillisPerFrameControlValue < 0)
            {
                minMillisPerFrame--;
                if (minMillisPerFrame <= 1)
                {
                    minMillisPerFrame = 1;
                }

            }
            else if (minMillisPerFrameControlValue > 0)
            {
                minMillisPerFrame++;
                if (minMillisPerFrame >= 200)
                {
                    minMillisPerFrame = 200;
                }
            }
            

            Int32 playerAngleControlValue = playerAngleControl.GetControl(Keyboard);
            if (playerAngleControlValue != 0)
            {
                if (playerAngleControlValue > 0)
                {
                    playerAngle -= diffMicros * DegreesPerMicrosecond;
                    if (playerAngle < 0) playerAngle += 360;
                }
                else if (playerAngleControlValue < 0)
                {
                    playerAngle += diffMicros * DegreesPerMicrosecond;
                    if (playerAngle >= 360) playerAngle -= 360;
                }
                //
                // Update player region
                //
                Int32 oldPlayerRegion = currentPlayerRegion;

                currentPlayerRegion = (Byte)Math.Floor((float)playerAngle * (float)currentRegionCount / 360f + .5f);
                if (currentPlayerRegion >= currentRegionCount) currentPlayerRegion = 0;

                if (currentPlayerRegion != oldPlayerRegion)
                {
                    //Console.WriteLine("Player Region {0} (Angle {1})", currentPlayerRegion, playerAngle);
                }
            }


            //
            // Update Barriers
            //
            System.Boolean oldPlayerCollision = playerCollision;
            playerCollision = false;
            for (int i = 0; i < barriers.Count; i++)
            {
                Barrier barrier = barriers[i];
                barrier.distanceTo -= barrierDistancePerMicrosecond * diffMicros;

                if (barrier.distanceTo <= BarrierCollisionHeight)
                {
                    // Check for collision
                    if (barrier.region == currentPlayerRegion && barrier.distanceTo + barrier.height >= BarrierCollisionHeight)
                    {
                        barrier.highlight = true;
                        playerCollision = true;
                    }
                    else
                    {
                        barrier.highlight = false;
                    }


                    //
                    // Check if barrier should be removed
                    //
                    if (barrier.distanceTo + barrier.height <= 0)
                    {
                        barriers.RemoveAt(i);
                        i--;
                    }
                }
            }

            // Update NonCollision Times
            if (playerCollision != oldPlayerCollision)
            {
                if (playerCollision)
                {
                    nonCollisionMicros = 0;
                }
                else
                {
                    lastNonCollisionTimeMicros = nowMicros;
                }
            }
            if (!playerCollision)
            {
                nonCollisionMicros = nowMicros - lastNonCollisionTimeMicros;
                if (nonCollisionMicros > maxNonCollisionMicros)
                {
                    maxNonCollisionMicros = nonCollisionMicros;
                }
            }

            //
            // Generate Barriers if needed
            //
            GenerateBarriers();

            //
            // Update World Shift Variables
            //
            // World Zoom
            worldZoom = .1f * worldZoomPulser.PulseValue(diffMicros);

            // World Spin Angle
            microsLeftOfCurrentWorldSpinVelocity = microsLeftOfCurrentWorldSpinVelocity - (float)diffMicros;
            if (microsLeftOfCurrentWorldSpinVelocity <= 0)
            {
                microsLeftOfCurrentWorldSpinVelocity = 1000000 * (3 + random.Next() % 3);

                Int32 randomValue = random.Next();
                currentWorldSpinRotationsPerSecond = .1f + .1f * (float)(randomValue % 3);
                if ((randomValue & 1) == 1) currentWorldSpinRotationsPerSecond *= -1;
                //Console.WriteLine("New Spin {0}", currentWorldSpinRotationsPerSecond);
            }
            worldSpinAngle += currentWorldSpinRotationsPerSecond * 360f * ((float)diffMicros / 1000000f);


            //
            // Update Background Color
            //
            if (playerCollision)
            {
                GL.ClearColor(Color.Red);
            }
            else
            {
                if (backgroundColorChanger.InColorChange())
                {
                    currentBackgroundColor = backgroundColorChanger.GetColor(nowMicros);
                    currentBarrierColor = new Color4(
                        currentBackgroundColor.R + .75f,
                        currentBackgroundColor.G + .75f,
                        currentBackgroundColor.B + .75f,
                        1.0f);
                    GL.ClearColor(currentBackgroundColor);
                }
                else
                {
                    Int32 randomColor = random.Next();
                    Color4 nextRandomColor = new Color4(
                        (float)((randomColor & 0xFF0000) >> 16) / 256 / 4,
                        (float)((randomColor & 0x00FF00) >> 8) / 256 / 4,
                        (float)((randomColor & 0x0000FF)) / 256 / 4,
                        1.0f);
                    backgroundColorChanger.SetColorChange(nowMicros + 1000 * (random.Next() % 1024), 1000000, nextRandomColor);
                }
            }
            return false;
        }

        UInt32 updateCount = 0;

        //
        // returns time it took to swap buffers
        //
        public override void Render()
        {
            updateCount++;


            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            //
            // Setup Game World
            //
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Ortho(-1000, 1000, -1000, 1000, -1000, 1000);


            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 modelview = Matrix4.LookAt(Vector3.Zero, new Vector3(0f, 0f, -1f)/*Vector3.UnitZ*/, Vector3.UnitY);
            GL.LoadMatrix(ref modelview);


            //
            // Rotate the game world
            //
            GL.PushMatrix();
                GL.Rotate(worldSpinAngle, Vector3d.UnitZ);
                GL.Scale(1 + worldZoom, 1 + worldZoom, 1);

                //
                // Draw Regions
                //
                float degreesPerRegion = 360f / (float)currentRegionCount;
                float tangentOfRegionHalfAngle = (float)Math.Tan(Math.PI / (double)currentRegionCount);

                float regionHeight = 1500;
                float regionHalfWidth = regionHeight * tangentOfRegionHalfAngle;

                for (int i = 0; i < currentRegionCount; i++)
                {
                    if (i % 2 == 0 && i != currentPlayerRegion) continue;


                    GL.PushMatrix();
                        GL.Rotate(i * degreesPerRegion, Vector3d.UnitZ);

                        GL.Begin(BeginMode.Quads);

                        if (i == currentPlayerRegion) GL.Color4(1f, 1f, 1f, .05f); else GL.Color4(0f, 0f, 0f, .3f);
                        GL.Vertex2(0, 0);
                        GL.Vertex2(-regionHalfWidth, regionHeight);
                        GL.Vertex2(regionHalfWidth, regionHeight);
                        GL.Vertex2(0, 0);

                        GL.End();
                    GL.PopMatrix();
                }                

                //
                // Draw the barriers
                //
                for (int i = barriers.Count - 1; i >= 0; i--)
                {
                    Barrier barrier = barriers[i];
                    barrier.Draw(currentBarrierColor);
                }

                //
                // Draw the center piece
                //
                float centerpieceHeight = 150;
                float centerpieceHalfWidth = centerpieceHeight * tangentOfRegionHalfAngle;

                if (playerCollision) GL.Color4(1f, 0f, 0f, 1f); else GL.Color4(0f, 0f, 0f, 1f); 
                for (int i = 0; i < currentRegionCount; i++)
                {
                    GL.PushMatrix();
                        GL.Rotate(i * degreesPerRegion, Vector3d.UnitZ);
                        GL.Begin(BeginMode.Quads);
                            GL.Vertex2(0, 0);
                            GL.Vertex2(-centerpieceHalfWidth, centerpieceHeight);
                            GL.Vertex2(centerpieceHalfWidth, centerpieceHeight);
                            GL.Vertex2(0, 0);
                        GL.End();
                    GL.PopMatrix();
                }  
                


                //
                // Draw Player Triangle
                //
                GL.PushMatrix();
                    GL.Rotate(playerAngle, Vector3d.UnitZ);

                    GL.Begin(BeginMode.Triangles);
                        GL.Color4(1.0f, 1.0f, 0.0f, .3f); GL.Vertex2(-20, 206);
                        GL.Color4(1.0f, 0.0f, 0.0f, .3f); GL.Vertex2(0, 240);
                        GL.Color4(0.2f, 0.9f, 1.0f, .3f); GL.Vertex2(20, 206);
                    GL.End();

                    GL.Begin(BeginMode.Quads);
                        GL.Color4(1f, 1f, 1f, .5f);
                        GL.Vertex2(-20, 206);
                        GL.Vertex2(-33, 206);
                        GL.Vertex2(  0, 256);
                        GL.Vertex2(  0, 240);
                
                        GL.Vertex2( 20, 206);
                        GL.Vertex2( 33, 206);
                        GL.Vertex2(  0, 256);
                        GL.Vertex2(  0, 240);

                        GL.Vertex2(-35, 197);
                        GL.Vertex2(-33, 206);
                        GL.Vertex2( 33, 206);
                        GL.Vertex2( 35, 197);

                        // Color the player point
                        if (playerCollision) GL.Color3(1f, 0f, 0f); else GL.Color3(1f, 1f, 1f);
                        GL.Vertex2(-5, 210);
                        GL.Vertex2(-5, 220);
                        GL.Vertex2(5, 220);
                        GL.Vertex2(5, 210);

                    GL.End();
                GL.PopMatrix();


            GL.PopMatrix();

            //
            // Draw Current Time
            //
            GL.Color4(1f, 1f, 1f, 1f);
            String currentSecondsString = (nonCollisionMicros / 1000000f).ToString("F2");
            gameFont.Draw(currentSecondsString, - gameFont.GetWidth(currentSecondsString.Length) / 2 , 8);
            gameFont.Draw("seconds", -gameFont.GetWidth("seconds".Length) / 2, -gameFont.charHeight - 8);


            //
            // Setup HUD
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


            gameStatsLabel.SetText(String.Format("Angle {0,3:F0} Region {1} FPS Max {2:F1} Avg:{3:F1}",
                playerAngle,
                currentPlayerRegion,
                1000f / (float)minMillisPerFrame,
                1000000f / (float)averageUpdateTimeDiffMicrosValue));
            userInterfaceHud.DrawComponent();


            //GL.Color4(1f, 1f, 1f, 1f);
            //smallFont.Draw((nonCollisionMicros / 1000000f).ToString("F2") + " seconds", ClientRectangle.Width / 2 - 100, ClientRectangle.Height / 2 - 30);
            smallFont.Draw("Best: " + (maxNonCollisionMicros / 1000000f).ToString("F2") + " seconds", ClientRectangle.Width / 2 - 100, ClientRectangle.Height - 30);
        }




        public static ALFormat GetSoundFormat(WaveFormat waveFormat)
        {
            switch (waveFormat.channelCount)
            {
                case 1: return waveFormat.bitsPerSample == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return waveFormat.bitsPerSample == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // The 'using' idiom guarantees proper resource cleanup.
            // We request 30 UpdateFrame events per second, and unlimited
            // RenderFrame events (as fast as the computer can handle).

            AudioContext audioContext = null;
            StreamWriter statsFileWriter = null;
            HexagonGame game = null;


            try
            {

                try
                {
                    audioContext = new AudioContext();

                    //
                    // Load Audio
                    //
                    int audioBufferHandle = AL.GenBuffer();
                    int audioSourceHandle = AL.GenSource();
                    //int state;

                    WaveFormat soundFileFormat;
                    Byte[] soundData = Wave.LoadWaveFile("CitadelKDrew.wav", out soundFileFormat);
                    Console.WriteLine(soundFileFormat);
                    AL.BufferData(audioBufferHandle, GetSoundFormat(soundFileFormat), soundData, soundData.Length, (int)soundFileFormat.samplesPerSecond);

                    AL.Source(audioSourceHandle, ALSourcei.Buffer, audioBufferHandle);
                    //AL.Source(audioSourceHandle, ALSourcei.ByteOffset, 0);
                    AL.SourcePlay(audioSourceHandle);

                    /*
                    // Query the source to find out when it stops playing.
                    do
                    {
                        Thread.Sleep(250);
                        Trace.Write(".");
                        AL.GetSource(source, ALGetSourcei.SourceState, out state);
                    }
                    while ((ALSourceState)state == ALSourceState.Playing);

                    Trace.WriteLine("");

                    AL.SourceStop(source);
                    AL.DeleteSource(source);
                    AL.DeleteBuffer(buffer);
                    */
                }
                catch (Exception e)
                {
                    Console.WriteLine("Sound failed: " + e.ToString());                    
                }

                String statisticsFileName = @"C:\temp\hexagon.stats";
                try
                {
                    statsFileWriter = new StreamWriter(new FileStream(statisticsFileName, FileMode.Create));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to open statistics file '{0}'", statisticsFileName);
                }

                game = new HexagonGame();
                game.CustomGameLoop(statsFileWriter);

            }
            finally
            {
                if(game != null) game.Dispose();
                if(statsFileWriter != null) statsFileWriter.Dispose();
                if(audioContext != null) audioContext.Dispose();
            }
        }
    }
}