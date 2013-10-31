using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using More.OpenTK;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

namespace More
{
    public class Thing
    {
    }


    public class Shape
    {

    }

    public class Shape2D : Shape
    {
    }
    public class Shape3D : Shape
    {
    }
    public class Circle : Shape2D
    {
    }
    public class Sphere : Shape3D
    {
        static Sphere instance;
        public static Sphere Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Sphere();
                }
                return instance;
            }
        }
        private Sphere()
        {
        }
    }

    public class Location
    {

    }
    public struct Location2DOpenGLDouble
    {
        public Double x, y;
    }
    public struct Location2D
    {
        public FactoredRational x, y;
        public Location2D(FactoredRational x, FactoredRational y)
        {
            this.x = x;
            this.y = y;
        }
    }
    public class Location3D : Location
    {
        public FactoredRational x, y, z;
        public Location3D(FactoredRational x, FactoredRational y, FactoredRational z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }


    public interface ICubeBoundedArea
    {
        void DrawOpenGLDouble(Rational CubeSize, Location3D translation);
    }
    public class CubeBoundedCubeArea : ICubeBoundedArea
    {
        static CubeBoundedCubeArea instance;
        public static CubeBoundedCubeArea Instance
        {
            get { if (instance == null) instance = new CubeBoundedCubeArea(); return instance; }
        }
        private CubeBoundedCubeArea() { }
        public void DrawOpenGLDouble(Rational cubeSize, Location3D translation)
        {
            GL.Begin(BeginMode.QuadStrip);
            
            // Find corners




            //GL.Vertex3(translation.x.value.ConvertToDouble() 


            GL.End();
        }
    }
    public class CubeBoundedSphereArea : ICubeBoundedArea
    {
        static CubeBoundedSphereArea instance;
        public static CubeBoundedSphereArea Instance
        {
            get { if (instance == null) instance = new CubeBoundedSphereArea(); return instance; }
        }
        private CubeBoundedSphereArea() { }
        public void DrawOpenGLDouble(Rational cubeSize, Location3D translation)
        {
            throw new NotImplementedException();
        }
    }




    public interface I2DBoundedArea
    {
        void DrawOpenGLDouble(Double left, Double bottom, Double size);
    }

    public interface ISquareBoundedArea : I2DBoundedArea
    {
    }
    public class SquareBoundedSquareArea : ISquareBoundedArea
    {
        static SquareBoundedSquareArea instance;
        public static SquareBoundedSquareArea Instance
        {
            get { if (instance == null) instance = new SquareBoundedSquareArea(); return instance; }
        }
        private SquareBoundedSquareArea() { }
        public void DrawOpenGLDouble(Double left, Double bottom, Double size)
        {
            GL.Begin(BeginMode.Quads);

            Double right = left + size;
            Double top = bottom + size;

            GL.Vertex2(left , bottom);
            GL.Vertex2(left , top);
            GL.Vertex2(right, top);
            GL.Vertex2(right, bottom);

            GL.End();
        }
    }
    public interface ICircleBoundedArea : I2DBoundedArea
    {
    }
    public class CircleBoundedCircleArea : ICircleBoundedArea
    {
        static CircleBoundedCircleArea instance;
        public static CircleBoundedCircleArea Instance
        {
            get { if (instance == null) instance = new CircleBoundedCircleArea(); return instance; }
        }
        private CircleBoundedCircleArea() { }
        public void DrawOpenGLDouble(Double left, Double bottom, Double scale)
        {
            GL.Begin(BeginMode.TriangleFan);

            /*
            for (UInt32 i = 0; i < circleResolution; i++)
            {
                Double angle = 2.0 * Math.PI * (Double)i / (Double)circleResolution;

                Double x = scale * Math.Cos(angle);
                Double y = scale * Math.Sin(angle);
                GL.Vertex2(left + x, bottom + y);
            }
            */

            //
            // Faster Circle Drawing Algorith!
            //
            Double angle = 2.0 * Math.PI / (Double)OpenGLOptions.CircleResolution;
            Double cos = Math.Cos(angle);
            Double sin = Math.Sin(angle);
            Double t, x = scale, y = 0;
            for (UInt32 i = 0; i < OpenGLOptions.CircleResolution; i++)
            {
                GL.Vertex2(left + x, bottom + y);
                t = x;
                x = cos * x - sin * y;
                y = sin * t + cos * y;
            }

            GL.End();
        }
    }






    //
    // A Physical Entity is a physical object that has
    //    1. Mass
    //    2. Area
    //
    public class SquareBoundedPhysicalEntity
    {
        public FactoredRational mass;

        public ISquareBoundedArea area;
        public Location2D areaTranslation;
        public FactoredRational areaScale;

        public SquareBoundedPhysicalEntity(FactoredRational mass,
            ISquareBoundedArea area, Location2D areaTranslation, FactoredRational areaScale)
        {
            this.mass = mass;
            this.area = area;
            this.areaTranslation = areaTranslation;
            this.areaScale = areaScale;
        }
        public void DrawOpenGLDouble()
        {
            Double x = areaTranslation.x.ConvertToDouble();
            Double y = areaTranslation.y.ConvertToDouble();
            Double scale = areaScale.ConvertToDouble();

            area.DrawOpenGLDouble(x, y, scale);
        }
    }

    public class CircleBoundedPhysicalEntity
    {
        public FactoredRational mass;

        public ICircleBoundedArea area;
        public Location2D areaTranslation;
        public FactoredRational areaScale;

        public CircleBoundedPhysicalEntity(FactoredRational mass,
            ICircleBoundedArea area, Location2D areaTranslation, FactoredRational areaScale)
        {
            this.mass = mass;
            this.area = area;
            this.areaTranslation = areaTranslation;
            this.areaScale = areaScale;
        }
        public void DrawOpenGLDouble()
        {
            Double x = areaTranslation.x.ConvertToDouble();
            Double y = areaTranslation.y.ConvertToDouble();
            Double scale = areaScale.ConvertToDouble();

            area.DrawOpenGLDouble(x, y, scale);
        }
    }

    public class Static3DPhysicalEntity
    {

    }


    public class MoveablePhysicalEntity
    {


    }






    /*
    public interface IRotatingCubeBoundedArea3D
    {
        public abstract void Draw(IGraphics3D graphics, Rational CubeSize, Location3D translation);
    }
    */









    public class PhysicalEntity3D : Thing
    {
        public Shape3D shape;
        public Rational shapeSize;
        public Rational shapeDensity;

        public Location3D location;

        public PhysicalEntity3D(Shape3D shape, Rational shapeSize, Rational shapeDensity, Location3D location)
        {
            this.shape = shape;
            this.shapeSize = shapeSize;
            this.shapeDensity = shapeDensity;
            this.location = location;
        }
    }









    public class Relationship
    {

    }


    // State is a relationship between things
    public class RelationshipState
    {
        public readonly Thing a, b;
        public readonly Relationship relationship;
    }
    public class LocationState
    {
        public readonly Thing a;
        public Int32 x, y, z;
    }










    public class Observer
    {



    }




    public class DecisionTree
    {


    }
}
