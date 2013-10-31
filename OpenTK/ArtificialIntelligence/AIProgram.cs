using System;

using More.OpenTK;

namespace More
{
    static class AIProgram
    {
        static void Main()
        {
            CachingFactorizer factorizer = new CachingFactorizer(new BruteForcePrimeFactorizer());


            /*
            PhysicalEntity3D electron = new PhysicalEntity3D(
                Sphere.Instance, new Rational(1), new Rational(1), new Location3D(1, 0, 0));


            PhysicalEntity3D nuetron = new PhysicalEntity3D(
                Sphere.Instance, new Rational(20), new Rational(1), new Location3D(

            */

            SquareBoundedPhysicalEntity squareElection = new SquareBoundedPhysicalEntity(
                FactoredRational.One, SquareBoundedSquareArea.Instance,
                new Location2D(FactoredRational.Zero, FactoredRational.Zero),
                new FactoredRational(2, factorizer));



            CircleBoundedPhysicalEntity circleElection1 = new CircleBoundedPhysicalEntity(
                FactoredRational.One, CircleBoundedCircleArea.Instance,
                new Location2D(new FactoredRational(3, factorizer), new FactoredRational(5, factorizer)),
                new FactoredRational(2, factorizer));

            CircleBoundedPhysicalEntity circleElection2 = new CircleBoundedPhysicalEntity(
                FactoredRational.One, CircleBoundedCircleArea.Instance,
                new Location2D(new FactoredRational(-6, factorizer), new FactoredRational(-3, factorizer)),
                new FactoredRational(2, factorizer));

            AISimulator simulator = new AISimulator(
                new SquareBoundedPhysicalEntity[] {
                    squareElection
                },
                new CircleBoundedPhysicalEntity[] {
                    circleElection1,
                    circleElection2,
                }
                );
            simulator.CustomGameLoop(null);
        }
    }
}
