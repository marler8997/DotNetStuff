using System;

using Marler.Xna.Common;

namespace Marler.Xna.Origins2D
{
    //
    // A dot is the smallest physical unit
    // It occupies space.
    class Dot
    {

        public Int32 x, y;

        public Int32 speedOffsetX, speedOffsetY;

        // SpeedInverse: Every x/8 updates, move one dot (value of [1,7] is invalid)
        // Dots Per Frame = 8 / SpeedInverse
        public Int32 updatesPer8DotsX, updatesPer8DotsY;

        public Dot(Int32 x, Int32 y)
        {
            this.x = x;
            this.y = y;

            this.speedOffsetX = 0;
            this.speedOffsetY = 0;
        }

        // speedInverseX/Y cannot be between 1 and 7 inclusive
        public Dot(Int32 x, Int32 y, Int32 updatesPer8DotsX, Int32 updatesPer8DotsY)
        {
            this.x = x;
            this.y = y;

            this.updatesPer8DotsX = updatesPer8DotsX;
            this.updatesPer8DotsY = updatesPer8DotsY;

            this.speedOffsetX = 0;
            this.speedOffsetY = 0;
        }


        // force of 1 = 1/8
        public void ChangeSpeedFromForce(Byte framesPer8DotsX, Byte framesPer8DotsY)
        {
            // The biggest change is from speedInverse = 2 to 1.


        }

        //
        // The minimum amount of speed inverse change is 1/8
        // speedInverseX/Y cannot be between 1 and 7 inclusive
        public void Update()
        {
            if (updatesPer8DotsX > 0)
            {
                speedOffsetX += 8;
                if (speedOffsetX >= updatesPer8DotsX)
                {
                    x++;
                    speedOffsetX = speedOffsetX - updatesPer8DotsX;
                }
            }
            else if (updatesPer8DotsX < 0)
            {
                speedOffsetX -= 8;
                if (speedOffsetX <= updatesPer8DotsX)
                {
                    x--;
                    speedOffsetX = speedOffsetX - updatesPer8DotsX;
                }
            }

            if (updatesPer8DotsY > 0)
            {
                speedOffsetY += 8;
                if (speedOffsetY >= updatesPer8DotsY)
                {
                    y++;
                    speedOffsetY = speedOffsetY - updatesPer8DotsY;
                }
            }
            else if (updatesPer8DotsY < 0)
            {
                speedOffsetY -= 8;
                if (speedOffsetY <= updatesPer8DotsY)
                {
                    y--;
                    speedOffsetY = speedOffsetY - updatesPer8DotsY;
                }
            }
        }
    }
}
