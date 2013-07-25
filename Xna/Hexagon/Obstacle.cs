using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Marler.Xna.Common;

namespace Marler.Xna
{
    public abstract class Obstacle
    {
        const Int32 obstacleStepOffsetFromCenter = 400;


        static readonly Random random = new Random();
        public static Obstacle Random(UInt32 regionCount)
        {
            Int32 randomValue = random.Next();

            switch(randomValue & 0x1)
            {
                case 0:
                    return new SimpleObstacle(100, regionCount, (UInt32)randomValue >> 3);
                case 1:
                    return new SimpleObstacle(100, regionCount, (UInt32)randomValue >> 3);
                    break;
            }
            throw new InvalidOperationException("Code bug...should never be here");
        }




        public static float gameWorldDistanceOverStepCounts = 1f / 10f;

        public readonly UInt32 totalStepHeight;

        public readonly UInt32 regionCount; // Note: at least 3 regions
        public readonly float regionAngle;
        public readonly float sinOfRegionAngleHalf;

        protected Obstacle(UInt32 totalStepHeight, UInt32 regionCount)
        {
            this.totalStepHeight = totalStepHeight;
            this.regionCount = regionCount;

            this.regionAngle = 2f * (float)Math.PI / (float)regionCount;
            this.sinOfRegionAngleHalf = (float)Math.Sin(Math.PI / (double)regionCount);
        }

        protected void DrawStraightBarrier(SpriteBatch spriteBatch, Matrix gameWorldMatrix,
            UInt32 stepDistanceToBarrier, UInt32 stepHeight, Byte region)
        {
            //
            // Get width of barrier from region count and entranceStopOffset
            //
            float gameDistanceToBarrier = (float)(obstacleStepOffsetFromCenter + stepDistanceToBarrier) * gameWorldDistanceOverStepCounts;
            float gameWorldHeight = (float)stepHeight * gameWorldDistanceOverStepCounts;

            float gameWorldWidth = 2 * gameDistanceToBarrier * sinOfRegionAngleHalf;


            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null,
                Matrix.CreateRotationZ((float)region * regionAngle) * gameWorldMatrix);
            
            


            //spriteBatch.FillRectangle(new Vector2(-gameWorldWidth / 2, -gameDistanceToBarrier - gameWorldHeight),
            //    new Vector2(gameWorldWidth, gameWorldHeight), Color.Black);
            
            /*
            spriteBatch.GraphicsDevice.FillQuad(
                new Vector2(-gameWorldWidth / 2, -gameDistanceToBarrier - gameWorldHeight),
                new Vector2(-gameWorldWidth / 2, -gameDistanceToBarrier                  ),
                new Vector2( gameWorldWidth / 2, -gameDistanceToBarrier                  ),
                new Vector2( gameWorldWidth / 2, -gameDistanceToBarrier - gameWorldHeight),
                Matrix.CreateRotationZ((float)region * regionAngle) * gameWorldMatrix,
                Color.Black);
            */

            //spriteBatch.DrawString(HexagonGame.miramonte18Font, String.Format("{0}", stepDistanceToBarrier),
            //    new Vector2(0, -gameDistanceToBarrier - 18), Color.White);
            
            
            spriteBatch.End();
        }

        public abstract void Draw(SpriteBatch spriteBatch, Matrix gameWorldMatrix, UInt32 stepDistanceToBarrier);
    }

    public class SimpleObstacle : Obstacle
    {
        readonly UInt32 barrierRegionFlags;
        public SimpleObstacle(UInt32 barrierStepLength, UInt32 regionCount, UInt32 barrierRegionFlags)
            : base(barrierStepLength, regionCount)
        {
            this.barrierRegionFlags = barrierRegionFlags;
        }
        public override void Draw(SpriteBatch spriteBatch, Matrix gameWorldMatrix, uint stepDistanceToBarrier)
        {
            UInt32 barrierRegionChecker = barrierRegionFlags;
            Byte region = 0;
            while (true)
            {
                if (barrierRegionChecker == 0 || region >= regionCount) return;
                if ((barrierRegionChecker & 0x1U) == 0x1U)
                {
                    DrawStraightBarrier(spriteBatch, gameWorldMatrix, stepDistanceToBarrier, totalStepHeight, region);
                }
                barrierRegionChecker >>= 1;
                region++;
            }
        }
    }



}
