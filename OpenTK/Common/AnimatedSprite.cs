using System;
/*
namespace More.OpenTK
{
    // TODO: make Animated Sprice From Row
    public class AnimatedSpriteColumn
    {
        readonly Texture2D texture;
        Int32 rows;
        Int32 currentFrame;
        Int32 width, height;

        public AnimatedSpriteColumn(Texture2D texture, Int32 rows)
        {
            this.texture = texture;
            this.rows = rows;

            this.width = texture.Width;
            this.height = texture.Height / rows;

            this.currentFrame = 0;
        }
        public void Reset()
        {
            this.currentFrame = 0;
        }
        public void NextFrame()
        {
            if (this.currentFrame >= rows - 1)
            {
                this.currentFrame = 0;
            }
            else
            {
                this.currentFrame++;
            }
        }
        public void Draw(SpriteBatch spriteBatch, Int32 x, Int32 y)
        {
            Rectangle sourceRectangle = new Rectangle(width, height * currentFrame, width, height);
            Rectangle destinationRectangle = new Rectangle(x, y, width, height);

            spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, Color.White);
        }

    }

    public class AnimatedSpriteFromGrid
    {
        readonly Texture2D texture;
        Int32 rows, columns;
        Int32 currentFrame, frameCount;

        Int32 width, height;

        public AnimatedSpriteFromGrid(Texture2D texture, Int32 rows, Int32 columns)
            : this(texture, rows, columns, rows * columns)
        {
        }
        public AnimatedSpriteFromGrid(Texture2D texture, Int32 rows, Int32 columns, Int32 frameCount)
        {
            this.texture = texture;
            this.rows = rows;
            this.columns = columns;

            this.width = texture.Width / columns;
            this.height = texture.Height / rows;

            this.currentFrame = 0;
            this.frameCount = frameCount;
        }
        public void Reset()
        {
            this.currentFrame = 0;
        }
        public void NextFrame()
        {
            if (this.currentFrame >= frameCount - 1)
            {
                this.currentFrame = 0;
            }
            else
            {
                this.currentFrame++;
            }
        }
        public void Draw(SpriteBatch spriteBatch, Int32 x, Int32 y)
        {
            int currentRow = currentFrame / columns;
            int currentColumn = currentFrame % columns;

            Rectangle sourceRectangle = new Rectangle(width * currentColumn, height * currentRow, width, height);
            Rectangle destinationRectangle = new Rectangle(x, y, width, height);

            spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, Color.White);
        }
        public void Draw(SpriteBatch spriteBatch, Int32 x, Int32 y, float rotation, Vector2 origin)
        {
            int currentRow = currentFrame / columns;
            int currentColumn = currentFrame % columns;

            Rectangle sourceRectangle = new Rectangle(width * currentColumn, height * currentRow, width, height);
            Rectangle destinationRectangle = new Rectangle(x, y, width, height);

            spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, Color.White, rotation, new Vector2(width / 2, height / 2), SpriteEffects.None, 1);
        }
    }
}
*/