using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Marler.Tank
{
    public class ArenaUtils {
    	
	    public static int windowWidth,windowHeight;
    	
	    public static int arenaWidth,arenaHeight;
    	
	    public static float glOrthoWidth,glOrthoHeight;
	    public static float glOrthoHalfWidth,glOrthoHalfHeight;
    	
	    public static float glArenaWidth,glArenaHeight;
	    public static float glArenaHalfWidth,glArenaHalfHeight;
    	
	    // TODO: [Performance] [Cache] faster by caching arenaToGLX(0) and arenaToGLY(0)
	    public static float arenaToGLWidth(int width) {
		    return ArenaUtils.arenaToGLX(width) - ArenaUtils.arenaToGLX(0);
	    }
	    public static float arenaToGLHeight(int height) {
		    return ArenaUtils.arenaToGLY(height) - ArenaUtils.arenaToGLY(0);
	    }
    	
    	
    	
	    public static float arenaToGLX(int arenaX) {
		    return ((float)(2*arenaX) - (float)arenaWidth) * glArenaHalfWidth / arenaWidth;
	    }
	    public static float arenaToGLY(int arenaY) {
		    return ((float)(2*arenaY) - (float)arenaHeight) * glArenaHalfHeight / arenaHeight;
	    }
    	
	    public static int glToArenaX(float glX) {
		    return (int)(((float)arenaWidth) * (.5f + glX / glArenaWidth));
	    }
	    public static int glToArenaY(float glY) {
		    return (int)(((float)arenaHeight) * (.5f + glY / glArenaHeight));
	    }
    }

    public class ArenaGLRenderer
    {
        private TankGame game;

        public float gridHalfGLThickness;
        public Vector3 gridColor;

        public ArenaGLRenderer(TankGame game)
        {
            this.game = game;
            this.gridHalfGLThickness = 0;
            this.gridColor = new Vector3(0, 1, 0);
        }

        public void Draw()
        {
            float glWidthHalf = ArenaUtils.glArenaHalfWidth;
            float glHeightHalf = ArenaUtils.glArenaHalfHeight;

            GL.Begin(BeginMode.Quads);

            // Arena Floor Grid
            if (gridHalfGLThickness > 0)
            {
                GL.Color3(gridColor.X, gridColor.Y, gridColor.Z);

                float columnGLYBottom = ArenaUtils.arenaToGLY(0);
                float columnGLYTop = ArenaUtils.arenaToGLY(ArenaUtils.arenaHeight);
                for (int i = 0; i <= ArenaUtils.arenaWidth; i += 15)
                {

                    float columnGlX = ArenaUtils.arenaToGLX(i);
                    float columnGLXLeft = columnGlX - gridHalfGLThickness;
                    float columnGLXRight = columnGlX + gridHalfGLThickness;

                    for (int j = 0; j < ArenaUtils.arenaHeight; j += 10)
                    {
                        GL.Vertex3(columnGLXLeft, columnGLYTop, 0);
                        GL.Vertex3(columnGLXRight, columnGLYTop, 0);
                        GL.Vertex3(columnGLXRight, columnGLYBottom, 0);
                        GL.Vertex3(columnGLXLeft, columnGLYBottom, 0);
                    }
                }

                float rowGLXLeft = ArenaUtils.arenaToGLX(0);
                float rowGLXRight = ArenaUtils.arenaToGLX(ArenaUtils.arenaWidth);
                for (int i = 0; i <= ArenaUtils.arenaHeight; i += 10)
                {
                    float rowGlY = ArenaUtils.arenaToGLY(i);
                    float rowGLXTop = rowGlY + gridHalfGLThickness;
                    float rowGLXBottom = rowGlY - gridHalfGLThickness;

                    for (int j = 0; j < ArenaUtils.arenaHeight; j += 10)
                    {
                        GL.Vertex3(rowGLXLeft, rowGLXTop, 0);
                        GL.Vertex3(rowGLXRight, rowGLXTop, 0);
                        GL.Vertex3(rowGLXRight, rowGLXBottom, 0);
                        GL.Vertex3(rowGLXLeft, rowGLXBottom, 0);
                    }

                }
            }

            // Walls			
            GL.Color3(.3f, .3f, .3f);

            // Top Wall
            GL.Vertex3(-glWidthHalf, glHeightHalf, 0);
            GL.Vertex3(-glWidthHalf, glHeightHalf, Settings.glWallHeight);
            GL.Vertex3(glWidthHalf, glHeightHalf, Settings.glWallHeight);
            GL.Vertex3(glWidthHalf, glHeightHalf, 0);

            // Left Wall
            GL.Vertex3(-glWidthHalf, -glHeightHalf, 0);
            GL.Vertex3(-glWidthHalf, -glHeightHalf, Settings.glWallHeight);
            GL.Vertex3(-glWidthHalf, glHeightHalf, Settings.glWallHeight);
            GL.Vertex3(-glWidthHalf, glHeightHalf, 0);

            // Right Wall
            GL.Vertex3(glWidthHalf, -glHeightHalf, 0);
            GL.Vertex3(glWidthHalf, -glHeightHalf, Settings.glWallHeight);
            GL.Vertex3(glWidthHalf, glHeightHalf, Settings.glWallHeight);
            GL.Vertex3(glWidthHalf, glHeightHalf, 0);

            // Wall Caps
            GL.Color3(.4f, .4f, .4f);

            // Top Cap
            GL.Vertex3(-glWidthHalf, glHeightHalf, Settings.glWallHeight);
            GL.Vertex3(-glWidthHalf, glHeightHalf + Settings.glBoundaryWallWidth, Settings.glWallHeight);
            GL.Vertex3(glWidthHalf, glHeightHalf + Settings.glBoundaryWallWidth, Settings.glWallHeight);
            GL.Vertex3(glWidthHalf, glHeightHalf, Settings.glWallHeight);


            // Bottom Inner Wall
            GL.Color3(.3f, .3f, .3f);
            GL.Vertex3(-glWidthHalf, -glHeightHalf, 0);
            GL.Vertex3(-glWidthHalf, -glHeightHalf, Settings.glWallHeight);
            GL.Vertex3(glWidthHalf, -glHeightHalf, Settings.glWallHeight);
            GL.Vertex3(glWidthHalf, -glHeightHalf, 0);

            // Left Outer Wall
            GL.Vertex3(-glWidthHalf - Settings.glBoundaryWallWidth, -glHeightHalf - Settings.glBoundaryWallWidth, 0);
            GL.Vertex3(-glWidthHalf - Settings.glBoundaryWallWidth, -glHeightHalf - Settings.glBoundaryWallWidth, Settings.glWallHeight);
            GL.Vertex3(-glWidthHalf - Settings.glBoundaryWallWidth, glHeightHalf + Settings.glBoundaryWallWidth, Settings.glWallHeight);
            GL.Vertex3(-glWidthHalf - Settings.glBoundaryWallWidth, glHeightHalf + Settings.glBoundaryWallWidth, 0);

            // Right Outer Wall
            GL.Vertex3(glWidthHalf + Settings.glBoundaryWallWidth, -glHeightHalf - Settings.glBoundaryWallWidth, 0);
            GL.Vertex3(glWidthHalf + Settings.glBoundaryWallWidth, -glHeightHalf - Settings.glBoundaryWallWidth, Settings.glWallHeight);
            GL.Vertex3(glWidthHalf + Settings.glBoundaryWallWidth, glHeightHalf + Settings.glBoundaryWallWidth, Settings.glWallHeight);
            GL.Vertex3(glWidthHalf + Settings.glBoundaryWallWidth, glHeightHalf + Settings.glBoundaryWallWidth, 0);

            // Front Cover Wall
            GL.Vertex3(-glWidthHalf - Settings.glBoundaryWallWidth, -glHeightHalf - Settings.glBoundaryWallWidth, 0);
            GL.Vertex3(-glWidthHalf - Settings.glBoundaryWallWidth, -glHeightHalf - Settings.glBoundaryWallWidth, Settings.glWallHeight);
            GL.Vertex3(glWidthHalf + Settings.glBoundaryWallWidth, -glHeightHalf - Settings.glBoundaryWallWidth, Settings.glWallHeight);
            GL.Vertex3(glWidthHalf + Settings.glBoundaryWallWidth, -glHeightHalf - Settings.glBoundaryWallWidth, 0);


            GL.Color3(.4f, .4f, .4f);
            // Left Cap
            GL.Vertex3(-glWidthHalf, -glHeightHalf - Settings.glBoundaryWallWidth, Settings.glWallHeight);
            GL.Vertex3(-glWidthHalf - Settings.glBoundaryWallWidth, -glHeightHalf - Settings.glBoundaryWallWidth, Settings.glWallHeight);
            GL.Vertex3(-glWidthHalf - Settings.glBoundaryWallWidth, glHeightHalf + Settings.glBoundaryWallWidth, Settings.glWallHeight);
            GL.Vertex3(-glWidthHalf, glHeightHalf + Settings.glBoundaryWallWidth, Settings.glWallHeight);

            // Right Cap
            GL.Vertex3(glWidthHalf, -glHeightHalf - Settings.glBoundaryWallWidth, Settings.glWallHeight);
            GL.Vertex3(glWidthHalf + Settings.glBoundaryWallWidth, -glHeightHalf - Settings.glBoundaryWallWidth, Settings.glWallHeight);
            GL.Vertex3(glWidthHalf + Settings.glBoundaryWallWidth, glHeightHalf + Settings.glBoundaryWallWidth, Settings.glWallHeight);
            GL.Vertex3(glWidthHalf, glHeightHalf + Settings.glBoundaryWallWidth, Settings.glWallHeight);

            // Bottom Cap
            GL.Vertex3(-glWidthHalf, -glHeightHalf, Settings.glWallHeight);
            GL.Vertex3(-glWidthHalf, -glHeightHalf - Settings.glBoundaryWallWidth, Settings.glWallHeight);
            GL.Vertex3(glWidthHalf, -glHeightHalf - Settings.glBoundaryWallWidth, Settings.glWallHeight);
            GL.Vertex3(glWidthHalf, -glHeightHalf, Settings.glWallHeight);



            // Draw the non-boundary walls
            /*
            for (int i = 0; i < game.level.walls.length; i++)
            {
                Wall wall = game.level.walls[i];

                // Walls			

                float glLeft = ArenaUtils.arenaToGLX(wall.x);
                float glRight = ArenaUtils.arenaToGLX(wall.rightLimit);
                float glBottom = ArenaUtils.arenaToGLY(wall.y);
                float glTop = ArenaUtils.arenaToGLY(wall.topLimit);

                GL.Color3f(.3f, .3f, .3f);
                // Back
                GL.Vertex3(glLeft, glTop, Settings.glWallHeight);
                GL.Vertex3(glRight, glTop, Settings.glWallHeight);
                GL.Vertex3(glRight, glTop, 0);
                GL.Vertex3(glLeft, glTop, 0);
                // Front
                GL.Vertex3(glLeft, glBottom, Settings.glWallHeight);
                GL.Vertex3(glRight, glBottom, Settings.glWallHeight);
                GL.Vertex3(glRight, glBottom, 0);
                GL.Vertex3(glLeft, glBottom, 0);
                // Left
                GL.Vertex3(glLeft, glTop, Settings.glWallHeight);
                GL.Vertex3(glLeft, glBottom, Settings.glWallHeight);
                GL.Vertex3(glLeft, glBottom, 0);
                GL.Vertex3(glLeft, glTop, 0);
                // Right
                GL.Vertex3(glRight, glTop, Settings.glWallHeight);
                GL.Vertex3(glRight, glBottom, Settings.glWallHeight);
                GL.Vertex3(glRight, glBottom, 0);
                GL.Vertex3(glRight, glTop, 0);
                GL.Color3f(.4f, .4f, .4f);
                // Top
                GL.Vertex3(glLeft, glTop, Settings.glWallHeight);
                GL.Vertex3(glRight, glTop, Settings.glWallHeight);
                GL.Vertex3(glRight, glBottom, Settings.glWallHeight);
                GL.Vertex3(glLeft, glBottom, Settings.glWallHeight);
            }
            */

            GL.End();
        }
    }
}
