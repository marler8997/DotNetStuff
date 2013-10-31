using System;  
using System.Collections.Generic;  
using System.Text;  
//using Microsoft.Xna.Framework;  
//using Microsoft.Xna.Framework.Graphics;  
/*
namespace More.OpenTK  
{
    //Grid Class V1.0.1  
	//Generates, calculates and draws basic informations for the  
	//orientation in 3D space.
	public enum GridKind  
	{  
	    Plane,  
	    Cicle,  
	}  
	 
	public enum GridDimesion  
	{  
	    Flat,  
	    Space,  
	}  
	 
	public enum SquarePosition
	{  
	    Top,  
	    TopRight,  
	    Right,  
	    BottomRight,  
	    Bottom,  
	    BottomLeft,  
	    Left,  
	    TopLeft,  
	}  
	 
	public class Grid
	{
        readonly Game game;

	    public Grid(Game game)
        {
            this.game = game;

            Kind = GridKind.Plane;
            Dimension = GridDimesion.Flat;

            drawMainAxis = true;
            drawScreenAxis = true;
            drawGrid = true;

            resolution = 1;
            scale = 10;

            axisScreenPosition = GetScreenPosition(new Vector2(20, 20), SquarePosition.BottomLeft);

            XColor = Color.Red;
            YColor = Color.Green;
            ZColor = Color.Blue;

            GridColor = Color.White;

            CreateAxis();
            CreateAxisArrow();
            CreateGroundGrid();

            //vertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionColor.VertexElements);
            basicEffect = new BasicEffect(game.GraphicsDevice);
            basicEffect.VertexColorEnabled = true; 
	    }  
	 
	    //VertexDeclaration vertexDeclaration;  
	    BasicEffect basicEffect;  
	          
	    public VertexPositionColor[] groundGrid;  
	    public VertexPositionColor[] axis;  
	    public VertexPositionColor[] axisArrow;  
	 
	    public GridKind Kind;  
	    public GridDimesion Dimension;

        static public bool drawMainAxis;
        static public bool drawScreenAxis;
        static public bool drawGrid;

	    public bool CameraGrid;  
	    public bool CameraAngles;  
	 
	    static public bool Refresh;  
	 
	    public float resolution;  
	    public int scale;

	    public Vector2 axisScreenPosition;  
	 
	    public Color XColor;  
	    public Color YColor;  
	    public Color ZColor;  
	 
	    public Color GridColor;

        public Vector2 GetScreenPosition(Vector2 offset, SquarePosition position)  
	    {  
            Int32 widthAsInt32 = game.GraphicsDevice.Viewport.Width;
            Int32 heightAsInt32 = game.GraphicsDevice.Viewport.Height;

            float width = (float)widthAsInt32;
            float height = (float)heightAsInt32;

            float halfWidth = widthAsInt32 / 2;
            float halfHeight = heightAsInt32 / 2;

            switch (position)  
	        {
                case SquarePosition.Top:
                    return new Vector2(halfWidth, offset.Y);
                case SquarePosition.TopRight:
                    return new Vector2(width - offset.X, offset.Y);
                case SquarePosition.Right:
                    return new Vector2(width - offset.X, halfHeight);
                case SquarePosition.BottomRight:
                    return new Vector2(width - offset.X, height - offset.Y);
                case SquarePosition.Bottom:
                    return new Vector2(halfWidth, height - offset.Y);
                case SquarePosition.BottomLeft:
                    return new Vector2(offset.X, height - offset.Y);
                case SquarePosition.Left:
                    return new Vector2(offset.X, halfHeight);
                case SquarePosition.TopLeft:  
	                return new Vector2(offset.X, offset.Y);
	        }
            throw new InvalidOperationException(String.Format("Unknown enum value for SquarePosition '{0}'", position));
	    }  
	 
	    public void CreateAxisArrow()  
	    {
            axisArrow = new VertexPositionColor[12];  
	 
	        //Axis Line:  
            axisArrow[0] = new VertexPositionColor(new Vector3(0, 0, 0), Color.White);
            axisArrow[1] = new VertexPositionColor(new Vector3(1, 0, 0), Color.White);  
	        //Arrow on the tip of the Line:  
            axisArrow[2] = new VertexPositionColor(new Vector3(0.8f, 0, 0.1f), Color.White);
            axisArrow[3] = new VertexPositionColor(new Vector3(0.8f, 0.1f, 0), Color.White);
            axisArrow[4] = new VertexPositionColor(new Vector3(1, 0, 0), Color.White);
            axisArrow[5] = new VertexPositionColor(new Vector3(0.8f, -0.1f, 0), Color.White);
            axisArrow[6] = new VertexPositionColor(new Vector3(0.8f, 0, -0.1f), Color.White);
            axisArrow[7] = new VertexPositionColor(new Vector3(0.8f, 0.1f, 0), Color.White);
            axisArrow[8] = new VertexPositionColor(new Vector3(0.8f, -0.1f, 0), Color.White);
            axisArrow[9] = new VertexPositionColor(new Vector3(0.8f, 0, 0.1f), Color.White);
            axisArrow[10] = new VertexPositionColor(new Vector3(0.8f, 0, -0.1f), Color.White);
            axisArrow[11] = new VertexPositionColor(new Vector3(1, 0, 0), Color.White);  
	    }  
	 
	    public void CreateAxis()  
	    {
            axis = new VertexPositionColor[6];

            axis[0] = new VertexPositionColor(new Vector3(-scale * resolution, 0, 0), XColor);
            axis[1] = new VertexPositionColor(new Vector3(scale * resolution, 0, 0), XColor);
            axis[2] = new VertexPositionColor(new Vector3(0, -scale * resolution, 0), YColor);
            axis[3] = new VertexPositionColor(new Vector3(0, scale * resolution, 0), YColor);
            axis[4] = new VertexPositionColor(new Vector3(0, 0, -scale * resolution), ZColor);
            axis[5] = new VertexPositionColor(new Vector3(0, 0, scale * resolution), ZColor);  
	    }  
	 
	    public void CreateGroundGrid()  
	    {  
	        int Number = (int)Math.Pow(scale, 2);

            groundGrid = new VertexPositionColor[Number];  
	 
	        int index = 0;  
	 
	        float length = scale*2 * resolution;  
	        float halfLength = length / 2;  
	 
	        for (int i = 0; i < (scale*2+1); i++)  
	        {  
	            if (i * resolution - halfLength != 0)  
	            {
                    groundGrid[index++] = new VertexPositionColor(new Vector3(  
	                            -halfLength, 0.0f, i * resolution - halfLength), GridColor);
                    groundGrid[index++] = new VertexPositionColor(new Vector3(  
	                    halfLength, 0.0f, i * resolution - halfLength), GridColor);
                    groundGrid[index++] = new VertexPositionColor(new Vector3(  
	                    i * resolution - halfLength, 0.0f, -halfLength), GridColor);
                    groundGrid[index++] = new VertexPositionColor(new Vector3(  
	                    i * resolution - halfLength, 0.0f, halfLength), GridColor);  
	            }  
	            else if (!drawMainAxis)  
	            {
                    groundGrid[index++] = new VertexPositionColor(new Vector3(  
	                            -halfLength, 0.0f, i * resolution - halfLength), GridColor);
                    groundGrid[index++] = new VertexPositionColor(new Vector3(  
	                    halfLength, 0.0f, i * resolution - halfLength), GridColor);
                    groundGrid[index++] = new VertexPositionColor(new Vector3(  
	                    i * resolution - halfLength, 0.0f, -halfLength), GridColor);
                    groundGrid[index++] = new VertexPositionColor(new Vector3(  
	                    i * resolution - halfLength, 0.0f, halfLength), GridColor);  
	            }  
	        }  
	              
	    }  
	 
	    public void Update()  
	    {  
	        if (Refresh)  
	        {  
	            CreateAxis();  
	            CreateAxisArrow();  
	            CreateGroundGrid();  
	            Refresh = false;  
	        }
	    }  
	 
	    public void Draw(GameTime gameTime, Matrix view, Matrix projection)  
	    {  
	        //GraphicsDevice.VertexDeclaration = vertexDeclaration;

            basicEffect.View = view;
            basicEffect.Projection = projection;

            if (drawGrid)  
	        {
	            if (Dimension == GridDimesion.Flat)  
	            {  
	                basicEffect.World = Matrix.Identity;  
	                DrawLineList(groundGrid);
	            }  
	            else 
	            {  
	                basicEffect.World = Matrix.CreateRotationX(MathHelper.PiOver2);
                    DrawLineList(groundGrid);  
	                basicEffect.World = Matrix.CreateRotationZ(MathHelper.PiOver2);
                    DrawLineList(groundGrid); ;  
	                basicEffect.World = Matrix.Identity;
                    DrawLineList(groundGrid);  
	            }   
	        }

            if (drawMainAxis)  
	        {  
	            DrawLineList(axis);  
	        }	 

	        if (drawScreenAxis)  
	        {  
	            //GraphicsDevice..DepthBufferEnable = false;  
	 
	            Vector3 NearSource = game.GraphicsDevice.Viewport.Unproject(new Vector3(axisScreenPosition, 0),  
	                projection, view, Matrix.Identity);
                Vector3 FarSource = game.GraphicsDevice.Viewport.Unproject(new Vector3(axisScreenPosition, 1),
                    projection, view, Matrix.Identity);  
	 
	            Vector3 Direction = FarSource - NearSource;  
	            Direction.Normalize();  
	            Vector3 Position = NearSource + (Direction * 20);  
	 
	            basicEffect.World = Matrix.CreateTranslation(Position);  
	            basicEffect.DiffuseColor = XColor.ToVector3();  
	            DrawLineStrip(axisArrow);  
	            basicEffect.DiffuseColor = YColor.ToVector3();  
	            basicEffect.World = Matrix.CreateRotationZ(MathHelper.PiOver2) * Matrix.CreateTranslation(Position);
                DrawLineStrip(axisArrow);  
	            basicEffect.DiffuseColor = ZColor.ToVector3();  
	            basicEffect.World = Matrix.CreateRotationY(-MathHelper.PiOver2) * Matrix.CreateTranslation(Position);
                DrawLineStrip(axisArrow);  
	 
	            basicEffect.DiffuseColor = Color.White.ToVector3();  
	            basicEffect.World = Matrix.Identity;  
	 
                // re-enable 3d rendering
                game.GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
	            //.GraphicsDevice.RenderState.DepthBufferEnable = true;  
	        }
	    }  
	 
	    void DrawLineList(VertexPositionColor[] vertices)
        {
            for (int i = 0; i < basicEffect.CurrentTechnique.Passes.Count; i++)
            {
                EffectPass pass = basicEffect.CurrentTechnique.Passes[i];
                pass.Apply();
                game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, vertices.Length / 2);
            }
	    }

        void DrawLineStrip(VertexPositionColor[] vertices)
        {
            for (int i = 0; i < basicEffect.CurrentTechnique.Passes.Count; i++)
            {
                EffectPass pass = basicEffect.CurrentTechnique.Passes[i];
                pass.Apply();
                game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, vertices, 0, vertices.Length - 1);
            }
	    }  
	}
}  
*/