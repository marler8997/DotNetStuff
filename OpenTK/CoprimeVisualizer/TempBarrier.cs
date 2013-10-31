using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

namespace Marler.Games.Hexagon
{
    public class Barrier
    {
        const Int32 obstacleStepOffsetFromCenter = 160;

        public readonly byte region, regionCount;
        public readonly float height;

        public float distanceTo;
        public System.Boolean highlight;

        public readonly float degreesPerRegion;
        public readonly float tangentOfRegionHalfAngle;

        public Barrier(byte region, byte regionCount, float height, float distanceTo)
        {
            this.region = region;
            this.regionCount = regionCount;
            this.height = height;

            this.distanceTo = distanceTo;
            this.highlight = false;

            //
            //
            //
            this.degreesPerRegion = 360f / (float)regionCount;
            this.tangentOfRegionHalfAngle = (float)Math.Tan(Math.PI / (double)regionCount);
        }
        public void Draw(Color4 color)
        {
            float visualDistanceToBarrier = (float)(obstacleStepOffsetFromCenter + distanceTo);

            float barrierTopHalfWidth = (visualDistanceToBarrier + height) * tangentOfRegionHalfAngle;
            float barrierBottomHalfWidth = visualDistanceToBarrier * tangentOfRegionHalfAngle;

            GL.PushMatrix();

            GL.Rotate(region * degreesPerRegion, Vector3d.UnitZ);

            GL.Begin(BeginMode.Quads);

            if (highlight) GL.Color3(1f, 0f, 0f); else GL.Color4(color);

            GL.Vertex2(-barrierBottomHalfWidth, visualDistanceToBarrier);
            GL.Vertex2(-barrierTopHalfWidth, visualDistanceToBarrier + height);
            GL.Vertex2(barrierTopHalfWidth, visualDistanceToBarrier + height);
            GL.Vertex2(barrierBottomHalfWidth, visualDistanceToBarrier);

            GL.End();
            GL.PopMatrix();
        }
    }
    public static class BarriersExtensions
    {
        public static float FarthestDistance(this List<Barrier> barriers)
        {
            if (barriers.Count <= 0) return 0;
            Barrier lastBarrier = barriers[barriers.Count - 1];
            return lastBarrier.distanceTo + lastBarrier.height;
        }

    }
    public static class BarrierGeneration
    {
        static readonly Random random = new Random();

        static readonly Byte[] RandomBytes = new Byte[32];
        static Int32 nextRandomByteIndex = 0;
        static Byte NextRandomByte()
        {
            Int32 index = nextRandomByteIndex;
            nextRandomByteIndex++;
            if (index == 0)
            {
                random.NextBytes(RandomBytes);
            }
            else
            {
                if (nextRandomByteIndex >= RandomBytes.Length)
                {
                    nextRandomByteIndex = 0;
                }
            }
            return RandomBytes[index];
        }

        static Int32 NextRandom(Int32 range)
        {
            if (range <= 0xFF)
            {
                return NextRandomByte() % range;
            }
            else if (range <= 0xFFFF)
            {
                return ((0xFF00 & (NextRandomByte() << 8)) | (0x00FF & NextRandomByte())) % range;
            }
            throw new NotImplementedException();
        }

        public static Byte NextRandomPercentage()
        {
            return (Byte)(((Int32)NextRandomByte() * 100) >> 8);
        }
        static Byte CustomProbabilityRandom(params Byte[] percentages)
        {
            Int32 randomValue = NextRandomPercentage();

            Byte i = 0;
            Int32 compare = percentages[0] - 1;
            while (true)
            {
                //Console.WriteLine("i={0} randomValue={1} sum={2}", i, randomValue, sum);
                if (randomValue <= compare) return i;
                i++;
                if (i >= percentages.Length)
                {
                    Console.WriteLine("here");
                    return (Byte)(i - 1);
                }
                compare += percentages[i];
            }
        }


        static float ScaledRandom(Int32 units, float scale)
        {
            return NextRandom(units) * scale;
        }
        static float ScaledRandomRange(Int32 minUnits, Int32 maxUnits, float scale)
        {
            return (minUnits + NextRandom(maxUnits - minUnits + 1)) * scale;
        }


        static Byte NextRegion(Byte regionCount, Byte region)
        {
            return (region >= regionCount) ? (Byte)0 : (Byte)(region + 1);
        }
        static Byte PrevRegion(Byte regionCount, Byte region)
        {
            return (region > 0) ? (Byte)(region - 1) : (Byte)(regionCount - 1);
        }
        static Byte RegionDistance(Byte regionCount, Byte region1, Byte region2)
        {
            byte max, min;
            if (region2 >= region1)
            {
                max = region2;
                min = region1;
            }
            else
            {
                max = region1;
                min = region2;
            }

            byte diff = (Byte)(max - min);
            if (diff > regionCount / 2)
            {
                return (Byte)(min + regionCount - max);
            }
            return diff;
        }


        static Byte LinearDecreasingProbabilityRandom(Byte range)
        {
            Int32 randomRange = (Int32)range * ((Int32)range + 1) / 2;
            Int32 randomValue = NextRandom(randomRange);

            Int32 compare = range - 1;
            for (byte i = 0; i < range; i++)
            {
                if (randomValue <= compare) return i;
                compare += (range - i - 1);
            }
            return (Byte)(range - 1);
        }


        public static void GenerateBarriers(List<Barrier> barriers, Byte regionCount, float barrierDistancePerMicrosecond)
        {
            //
            // From game tests, with a constant player speed, around the minimum distance required for the player to travel 180 degrees
            // is about barrierDistancePerMicrosecond * 400,000.  So that means every section should probably be that distance apart
            //
            float minimumDistanceBetweenSections = barrierDistancePerMicrosecond * 400000f;
            float standardBarrierWidth = barrierDistancePerMicrosecond * 150000f;

            float randomSpaceToNextSection = minimumDistanceBetweenSections + barrierDistancePerMicrosecond * ScaledRandomRange(10, 10, 50000);

            byte option;
            byte lastRegionExit;
            //
            // Select Barrier Type
            //
            byte firstRandomByte = NextRandomByte();
            //firstRandomByte = 1;
            switch (firstRandomByte & 0x3)
            {
                case 0: // Consecutive Opposite One Space Rings

                    GenerateCRing(barriers, regionCount, barriers.FarthestDistance() + randomSpaceToNextSection, standardBarrierWidth, 0, (Byte)(regionCount - 1));
                    GenerateCRing(barriers, regionCount, barriers.FarthestDistance() + minimumDistanceBetweenSections * 1.3f, standardBarrierWidth, (Byte)(regionCount / 2), (Byte)(regionCount - 1));

                    option = (Byte)((firstRandomByte >> 6) & 0x3);
                    if (option > 1)
                    {
                        GenerateCRing(barriers, regionCount, barriers.FarthestDistance() + minimumDistanceBetweenSections * 1.3f, standardBarrierWidth, 0, (Byte)(regionCount - 1));
                        if (option > 2)
                        {
                            GenerateCRing(barriers, regionCount, barriers.FarthestDistance() + minimumDistanceBetweenSections * 1.3f, standardBarrierWidth, (Byte)(regionCount / 2), (Byte)(regionCount - 1));
                        }
                    }


                    break;
                case 1: // Consecutive Rings

                    //GenerateCRing(barriers, regionCount, barriers.FarthestDistance() + randomSpaceToNextSection, 90, 0, (Byte)(regionCount - 1));

                    GenerateCRing(barriers, regionCount, barriers.FarthestDistance() + randomSpaceToNextSection, standardBarrierWidth,
                        (Byte)(NextRandomByte() % regionCount), (Byte)(regionCount - 1));
                    option = LinearDecreasingProbabilityRandom(4);
                    for (int i = 0; i <= option; i++)
                    {
                        GenerateCRing(barriers, regionCount, barriers.FarthestDistance() + minimumDistanceBetweenSections * 1.2f, standardBarrierWidth,
                            (Byte)(NextRandomByte() % regionCount), (Byte)(regionCount - 1));
                    }
                    break;
                case 2: // Some random rings

                    GenerateRandomRing(barriers, regionCount, barriers.FarthestDistance() + randomSpaceToNextSection, standardBarrierWidth);
                    option = LinearDecreasingProbabilityRandom(4);
                    for (int i = 0; i <= option; i++)
                    {
                        GenerateRandomRing(barriers, regionCount, barriers.FarthestDistance() + minimumDistanceBetweenSections * 1.4f, standardBarrierWidth);
                    }

                    break;
                case 3: // Spiral

                    option = (Byte)(NextRandomByte() % regionCount);
                    GenerateSpiral(barriers, regionCount, barriers.FarthestDistance() + randomSpaceToNextSection, minimumDistanceBetweenSections,
                        option, (Byte)(regionCount + (regionCount / 2) * LinearDecreasingProbabilityRandom(3)), (firstRandomByte & 0x80) == 0x80);

                    break;
                default:
                    throw new InvalidOperationException(String.Format("Code Bug: Mod did not match number of random cases"));
            }


        }

        static void GenerateRandomRing(List<Barrier> barriers, Byte regionCount, float distanceTo, float height)
        {
            Byte randomByte = NextRandomByte();
            Byte region = 0;

            while (true)
            {
                if (randomByte == 0 || region >= regionCount - 1) return;
                if ((randomByte & 0x1U) == 0x1U)
                {
                    barriers.Add(new Barrier(region, regionCount, height, distanceTo));
                }
                randomByte >>= 1;
                region++;
            }
        }

        static void GenerateCRing(List<Barrier> barriers, Byte regionCount, float distanceTo, float height, byte barrierRegionStart, byte barrierCount)
        {
            byte region = barrierRegionStart;
            for (byte i = 0; i < barrierCount; i++)
            {
                barriers.Add(new Barrier(region, regionCount, height, distanceTo));
                region++;
                if (region >= regionCount) region = 0;
            }

        }


        static void GenerateSpiral(List<Barrier> barriers, Byte regionCount, float distanceTo, float minimumDistanceBetweenSections, byte entranceRegion, byte spiralLength, System.Boolean clockwise)
        {
            float regionDistanceToDiff = minimumDistanceBetweenSections / regionCount * 2;


            //
            // Make the cring to start the spiral
            //
            GenerateCRing(barriers, regionCount, distanceTo, regionDistanceToDiff, NextRegion(regionCount, entranceRegion), (Byte)(regionCount - 1));

            //
            //Make the wall to enter the spiral
            //
            barriers.Add(new Barrier(clockwise ? PrevRegion(regionCount, entranceRegion) : NextRegion(regionCount, entranceRegion), regionCount, regionDistanceToDiff * 3, distanceTo + regionDistanceToDiff));

            distanceTo += regionDistanceToDiff * 3;
            byte currentRegion = entranceRegion;
            for (byte i = 0; i < spiralLength; i++)
            {

                barriers.Add(new Barrier(currentRegion, regionCount, 2f * regionDistanceToDiff, distanceTo));

                distanceTo += regionDistanceToDiff;
                if (clockwise)
                {
                    currentRegion = NextRegion(regionCount, currentRegion);
                }
                else
                {
                    currentRegion = PrevRegion(regionCount, currentRegion);
                }
            }
        }



    }
}