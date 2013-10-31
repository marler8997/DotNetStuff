using System;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace More.OpenTK
{
    public abstract class Font
    {
        public readonly Int32 charWidth, charHeight, charSpacing;
        public Font(Int32 charWidth, Int32 charHeight, Int32 charSpacing)
        {
            this.charWidth = charWidth;
            this.charHeight = charHeight;
            this.charSpacing = charSpacing;
        }
        public Int32 GetWidth(Int32 charLength)
        {
            if (charLength <= 0) return 0;
            return (charWidth + charSpacing) * charLength - charSpacing;
        }
        public abstract void Draw(String text, Int32 x, Int32 y);
    }



    public delegate void CharacterRenderFunction(Int32 x, Int32 y, Int32 width, Int32 height);
    public class CharacterRenderMapFont : Font
    {
        readonly CharacterRenderFunction[] renderers;
        public CharacterRenderMapFont(CharacterRenderFunction[] renderers, Int32 charWidth, Int32 charHeight, Int32 charSpacing)
            : base(charWidth, charHeight, charSpacing)
        {
            if (renderers == null || renderers.Length != 256)
                throw new InvalidOperationException(String.Format("Expected the length of the Character Renderes to be 256 but it {0}",
                    (renderers == null) ? 0 : renderers.Length));

		    this.renderers = renderers;
        }
        public override void Draw(String text, Int32 x, Int32 y)
        {
            // Render each character
            for (int i = 0; i < text.Length; i++)
            {
                CharacterRenderFunction renderer = renderers[(Byte)text[i]];

                if (renderer != null) renderer(x, y, charWidth, charHeight);

                x += charWidth + charSpacing;
            }
        }
    }
}
