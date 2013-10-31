using System;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace More.OpenTK
{
    public static class CharacterRenderSetFactory
    {
        public static CharacterRenderFunction[] CreateFixedWidthSegmentRenderSet(float segmentWidth)
        {
            CharacterRenderFunction[] renderers = new CharacterRenderFunction[256];
            renderers['A'] = new EightSegmentFixedWidthRenderer(EightSegment.A, segmentWidth).DrawChar;
            renderers['B'] = new EightSegmentFixedWidthRenderer(EightSegment.B, segmentWidth).DrawChar;
            renderers['C'] = new EightSegmentFixedWidthRenderer(EightSegment.C, segmentWidth).DrawChar;
            renderers['D'] = new EightSegmentFixedWidthRenderer(EightSegment.D, segmentWidth).DrawChar;
            renderers['E'] = new EightSegmentFixedWidthRenderer(EightSegment.E, segmentWidth).DrawChar;
            renderers['F'] = new EightSegmentFixedWidthRenderer(EightSegment.F, segmentWidth).DrawChar;
            renderers['G'] = new EightSegmentFixedWidthRenderer(EightSegment.G, segmentWidth).DrawChar;
            renderers['H'] = new EightSegmentFixedWidthRenderer(EightSegment.H, segmentWidth).DrawChar;
            renderers['I'] = new EightSegmentFixedWidthRenderer(EightSegment.I, segmentWidth).DrawChar;
            renderers['J'] = new EightSegmentFixedWidthRenderer(EightSegment.J, segmentWidth).DrawChar;
            renderers['K'] = new EightSegmentFixedWidthRenderer(EightSegment.K, segmentWidth).DrawChar;
            renderers['L'] = new EightSegmentFixedWidthRenderer(EightSegment.L, segmentWidth).DrawChar;

            renderers['N'] = new NRendererFixedSegmentWidth(segmentWidth).DrawChar;
            renderers['O'] = new EightSegmentFixedWidthRenderer(EightSegment.O, segmentWidth).DrawChar;
            renderers['P'] = new EightSegmentFixedWidthRenderer(EightSegment.P, segmentWidth).DrawChar;
            //...
            renderers['R'] = new RRendererFixedSegmentWidth(segmentWidth).DrawChar;
            renderers['S'] = new EightSegmentFixedWidthRenderer(EightSegment.S, segmentWidth).DrawChar;
            renderers['T'] = new EightSegmentFixedWidthRenderer(EightSegment.T, segmentWidth).DrawChar;
            renderers['U'] = new EightSegmentFixedWidthRenderer(EightSegment.U, segmentWidth).DrawChar;
            renderers['V'] = new VRendererFixedSegmentWidth(segmentWidth).DrawChar;

            renderers['Y'] = new YRendererFixedSegmentWidth(segmentWidth).DrawChar;

            // Set lower case to have same renderer has upper case
            for (char c = 'a'; c <= 'z'; c++)
            {
                renderers[c] = renderers[c + ('A' - 'a')];
            }

            renderers['0'] = renderers['O'];
            renderers['1'] = new EightSegmentFixedWidthRenderer(EightSegment.One, segmentWidth).DrawChar;
            renderers['2'] = new EightSegmentFixedWidthRenderer(EightSegment.Two, segmentWidth).DrawChar;
            renderers['3'] = new EightSegmentFixedWidthRenderer(EightSegment.Three, segmentWidth).DrawChar;
            renderers['4'] = new EightSegmentFixedWidthRenderer(EightSegment.Four, segmentWidth).DrawChar;
            renderers['5'] = renderers['S'];
            renderers['6'] = new EightSegmentFixedWidthRenderer(EightSegment.Five, segmentWidth).DrawChar;
            renderers['7'] = new EightSegmentFixedWidthRenderer(EightSegment.Six, segmentWidth).DrawChar;
            renderers['8'] = renderers['B'];
            renderers['9'] = new EightSegmentFixedWidthRenderer(EightSegment.Seven, segmentWidth).DrawChar;

            renderers['.'] = new PeriodRendererFixedSegmentWidth(segmentWidth).DrawChar;
            return renderers;
        }
        
        static CharacterRenderFunction[] variableLengthSegmentRenderers;
        public static CharacterRenderFunction[] VariableLengthSegmentRenderers
        {
            get
            {
                if(variableLengthSegmentRenderers == null)
                {
                    variableLengthSegmentRenderers = new CharacterRenderFunction[256];
                    variableLengthSegmentRenderers['A'] = new EightSegmentRelativeWidthRenderer(EightSegment.A).DrawChar;
                    variableLengthSegmentRenderers['B'] = new EightSegmentRelativeWidthRenderer(EightSegment.B).DrawChar;
                    variableLengthSegmentRenderers['C'] = new EightSegmentRelativeWidthRenderer(EightSegment.C).DrawChar;
                    variableLengthSegmentRenderers['D'] = new EightSegmentRelativeWidthRenderer(EightSegment.D).DrawChar;
                    variableLengthSegmentRenderers['E'] = new EightSegmentRelativeWidthRenderer(EightSegment.E).DrawChar;
                    variableLengthSegmentRenderers['F'] = new EightSegmentRelativeWidthRenderer(EightSegment.F).DrawChar;
                    variableLengthSegmentRenderers['G'] = new EightSegmentRelativeWidthRenderer(EightSegment.G).DrawChar;
                    variableLengthSegmentRenderers['H'] = new EightSegmentRelativeWidthRenderer(EightSegment.H).DrawChar;
                    variableLengthSegmentRenderers['I'] = new EightSegmentRelativeWidthRenderer(EightSegment.I).DrawChar;
                    variableLengthSegmentRenderers['J'] = new EightSegmentRelativeWidthRenderer(EightSegment.J).DrawChar;
                    variableLengthSegmentRenderers['K'] = new EightSegmentRelativeWidthRenderer(EightSegment.K).DrawChar;
                    variableLengthSegmentRenderers['L'] = new EightSegmentRelativeWidthRenderer(EightSegment.L).DrawChar;
                    variableLengthSegmentRenderers['M'] = new MRenderer().DrawChar;

                    variableLengthSegmentRenderers['N'] = NRenderer.Instance.DrawChar;
                    variableLengthSegmentRenderers['O'] = new EightSegmentRelativeWidthRenderer(EightSegment.O).DrawChar;
                    variableLengthSegmentRenderers['P'] = new EightSegmentRelativeWidthRenderer(EightSegment.P).DrawChar;
                    //...
                    variableLengthSegmentRenderers['R'] = RRenderer.Instance.DrawChar;
                    variableLengthSegmentRenderers['S'] = new EightSegmentRelativeWidthRenderer(EightSegment.S).DrawChar;
                    variableLengthSegmentRenderers['T'] = new EightSegmentRelativeWidthRenderer(EightSegment.T).DrawChar;
                    variableLengthSegmentRenderers['U'] = new EightSegmentRelativeWidthRenderer(EightSegment.U).DrawChar;
                    variableLengthSegmentRenderers['V'] = VRenderer.DrawChar;

                    //variableLengthSegmentRenderers['Y'] = new YRenderer(segmentWidth);
                    variableLengthSegmentRenderers['X'] = XRenderer.DrawChar;

                    // Set lower case to have same renderer has upper case
                    for (char c = 'a'; c <= 'z'; c++)
                    {
                        variableLengthSegmentRenderers[c] = variableLengthSegmentRenderers[c + ('A' - 'a')];
                    }

                    variableLengthSegmentRenderers['0'] = variableLengthSegmentRenderers['O'];
                    variableLengthSegmentRenderers['1'] = new EightSegmentRelativeWidthRenderer(EightSegment.One).DrawChar;
                    variableLengthSegmentRenderers['2'] = new EightSegmentRelativeWidthRenderer(EightSegment.Two).DrawChar;
                    variableLengthSegmentRenderers['3'] = new EightSegmentRelativeWidthRenderer(EightSegment.Three).DrawChar;
                    variableLengthSegmentRenderers['4'] = new EightSegmentRelativeWidthRenderer(EightSegment.Four).DrawChar;
                    variableLengthSegmentRenderers['5'] = variableLengthSegmentRenderers['S'];
                    variableLengthSegmentRenderers['6'] = new EightSegmentRelativeWidthRenderer(EightSegment.Six).DrawChar;
                    variableLengthSegmentRenderers['7'] = new EightSegmentRelativeWidthRenderer(EightSegment.Seven).DrawChar;
                    variableLengthSegmentRenderers['8'] = variableLengthSegmentRenderers['B'];
                    variableLengthSegmentRenderers['9'] = new EightSegmentRelativeWidthRenderer(EightSegment.Nine).DrawChar;

                    variableLengthSegmentRenderers['.'] = PeriodRenderer.DrawChar;

                }
                return variableLengthSegmentRenderers;;
            }
        }
    }


    public static class EightSegment
    {
        public const Byte Top         = 0x01;
        public const Byte TopRight    = 0x02;
        public const Byte BottomRight = 0x04;
        public const Byte Bottom      = 0x08;
        public const Byte BottomLeft  = 0x10;
        public const Byte TopLeft     = 0x20;
        public const Byte HorzCenter  = 0x40;
        public const Byte VertCenter  = (Byte)0x80;
        
        public const Byte Zero  = O;
        public const Byte One   = (Byte)(EightSegment.VertCenter);
        public const Byte Two   = (Byte)(EightSegment.Top | EightSegment.TopRight | EightSegment.HorzCenter | EightSegment.BottomLeft | EightSegment.Bottom);
        public const Byte Three = (Byte)(EightSegment.Top | EightSegment.TopRight | EightSegment.HorzCenter | EightSegment.BottomRight | EightSegment.Bottom);
        public const Byte Four  = (Byte)(EightSegment.TopLeft | EightSegment.HorzCenter | EightSegment.TopRight | EightSegment.BottomRight);
        public const Byte Five  = S;
        public const Byte Six   = (Byte)(EightSegment.TopLeft | EightSegment.BottomLeft | EightSegment.Bottom | EightSegment.BottomRight | EightSegment.HorzCenter);
        public const Byte Seven = (Byte)(EightSegment.Top | EightSegment.TopRight | EightSegment.BottomRight);
        public const Byte Eight = B;
        public const Byte Nine  = (Byte)(EightSegment.Top | EightSegment.TopRight | EightSegment.BottomRight | EightSegment.Bottom | EightSegment.TopLeft | EightSegment.HorzCenter);

        public const Byte A = (Byte)(EightSegment.Top | EightSegment.TopRight | EightSegment.BottomRight | EightSegment.BottomLeft | EightSegment.TopLeft | EightSegment.HorzCenter);
        public const Byte B = (Byte)(EightSegment.Top | EightSegment.TopRight | EightSegment.BottomRight | EightSegment.Bottom | EightSegment.BottomLeft | EightSegment.TopLeft | EightSegment.HorzCenter);
        public const Byte C = (Byte)(EightSegment.Top | EightSegment.Bottom | EightSegment.BottomLeft | EightSegment.TopLeft);
        public const Byte D = (Byte)(EightSegment.Top | EightSegment.TopRight | EightSegment.BottomRight | EightSegment.Bottom | EightSegment.BottomLeft | EightSegment.TopLeft);
        public const Byte E = (Byte)(EightSegment.Top | EightSegment.Bottom | EightSegment.BottomLeft | EightSegment.TopLeft | EightSegment.HorzCenter);
        public const Byte F = (Byte)(EightSegment.Top | EightSegment.BottomLeft | EightSegment.TopLeft | EightSegment.HorzCenter);
        public const Byte G = (Byte)(EightSegment.Top | EightSegment.BottomRight | EightSegment.Bottom | EightSegment.BottomLeft | EightSegment.TopLeft);
        public const Byte H = (Byte)(EightSegment.TopRight | EightSegment.BottomRight | EightSegment.BottomLeft | EightSegment.TopLeft | EightSegment.HorzCenter);
        public const Byte I = (Byte)(EightSegment.Top | EightSegment.Bottom | EightSegment.VertCenter);
        public const Byte J = (Byte)(EightSegment.TopRight | EightSegment.BottomRight | EightSegment.Bottom);
        public const Byte K = (Byte)(EightSegment.TopRight | EightSegment.BottomRight | EightSegment.BottomLeft | EightSegment.TopLeft | EightSegment.HorzCenter);
        public const Byte L = (Byte)(EightSegment.Bottom | EightSegment.BottomLeft | EightSegment.TopLeft);

        public const Byte O = (Byte)(EightSegment.Top | EightSegment.TopRight | EightSegment.BottomRight | EightSegment.Bottom | EightSegment.BottomLeft | EightSegment.TopLeft);
        public const Byte P = (Byte)(EightSegment.Top | EightSegment.TopRight | EightSegment.BottomLeft | EightSegment.TopLeft | EightSegment.HorzCenter);

        public const Byte S = (Byte)(EightSegment.Top | EightSegment.TopLeft | EightSegment.HorzCenter | EightSegment.BottomRight | EightSegment.Bottom);
        public const Byte T = (Byte)(EightSegment.Top | EightSegment.VertCenter);
        public const Byte U = (Byte)(EightSegment.TopRight | EightSegment.BottomRight | EightSegment.Bottom | EightSegment.BottomLeft | EightSegment.TopLeft);

        public const Byte MPartial = (Byte)(EightSegment.TopRight | EightSegment.BottomRight | EightSegment.BottomLeft | EightSegment.TopLeft);
        public const Byte NPartial = (Byte)(EightSegment.TopRight | EightSegment.BottomRight | EightSegment.BottomLeft | EightSegment.TopLeft);
        public const Byte RPartial = (Byte)(EightSegment.Top | EightSegment.TopRight | EightSegment.BottomLeft | EightSegment.TopLeft | EightSegment.HorzCenter);
    }
    public class EightSegmentRelativeWidthRenderer
    {    	
	    public readonly byte segmentFlags;
        public EightSegmentRelativeWidthRenderer(byte segmentFlags)
        {
		    this.segmentFlags = segmentFlags;
	    }
        public virtual void DrawChar(Int32 x, Int32 y, Int32 width, Int32 height)
        {
            float segmentWidth = width / 5;
            float halfHeight = height / 2f;
    		
		    GL.Begin(BeginMode.Quads);
		        if((segmentFlags & EightSegment.Top) != 0)
                {
			        GL.Vertex2(x        ,y + height               );
			        GL.Vertex2(x + width,y + height               );
			        GL.Vertex2(x + width,y + height - segmentWidth);	
			        GL.Vertex2(x        ,y + height - segmentWidth);		
		        }
		        if((segmentFlags & EightSegment.TopRight) != 0)
                {
			        GL.Vertex2(x + width - segmentWidth,y + height);
			        GL.Vertex2(x + width               ,y + height);
			        GL.Vertex2(x + width               ,y + halfHeight);
			        GL.Vertex2(x + width - segmentWidth,y + halfHeight);
		        }
		        if((segmentFlags & EightSegment.BottomRight) != 0)
                {
			        GL.Vertex2(x + width - segmentWidth,y + halfHeight);
			        GL.Vertex2(x + width               ,y + halfHeight);
			        GL.Vertex2(x + width               ,y             );
			        GL.Vertex2(x + width - segmentWidth,y             );
		        }
                if ((segmentFlags & EightSegment.Bottom) != 0)
                {
			        GL.Vertex2(x        ,y               );
			        GL.Vertex2(x + width,y               );
			        GL.Vertex2(x + width,y + segmentWidth);	
			        GL.Vertex2(x        ,y + segmentWidth);		
		        }
                if ((segmentFlags & EightSegment.BottomLeft) != 0)
                {
			        GL.Vertex2(x + segmentWidth,y + halfHeight);
			        GL.Vertex2(x               ,y + halfHeight);
			        GL.Vertex2(x               ,y             );
			        GL.Vertex2(x + segmentWidth,y             );
		        }
                if ((segmentFlags & EightSegment.TopLeft) != 0)
                {
			        GL.Vertex2(x + segmentWidth,y + height);
			        GL.Vertex2(x               ,y + height);
			        GL.Vertex2(x               ,y + halfHeight);
			        GL.Vertex2(x + segmentWidth,y + halfHeight);
		        }
        		
		        float segmentHalfWidth;
                if ((segmentFlags & EightSegment.HorzCenter) != 0)
                {
			        segmentHalfWidth = segmentWidth / 2f;
			        GL.Vertex2(x        ,y + halfHeight + segmentHalfWidth);
			        GL.Vertex2(x + width,y + halfHeight + segmentHalfWidth);
			        GL.Vertex2(x + width,y + halfHeight - segmentHalfWidth);	
			        GL.Vertex2(x        ,y + halfHeight - segmentHalfWidth);		
		        }
                if ((segmentFlags & EightSegment.VertCenter) != 0)
                {
			        segmentHalfWidth = segmentWidth / 2f;
			        float widthHalf = width/2f;
			        GL.Vertex2(x + widthHalf - segmentHalfWidth,y + height);
			        GL.Vertex2(x + widthHalf + segmentHalfWidth,y + height);
			        GL.Vertex2(x + widthHalf + segmentHalfWidth,y         );	
			        GL.Vertex2(x + widthHalf - segmentHalfWidth,y         );		
		        }
            GL.End();
	    }
    }


    public class EightSegmentFixedWidthRenderer
    {    	
	    public readonly byte segmentFlags;
	    public readonly float segmentWidth;

        public EightSegmentFixedWidthRenderer(byte segmentFlags, float segmentWidth)
        {
		    this.segmentFlags = segmentFlags;
		    this.segmentWidth = segmentWidth;
	    }
        public virtual void DrawChar(Int32 x, Int32 y, Int32 width, Int32 height)
        {
            float halfHeight = height / 2f;
    		
		    GL.Begin(BeginMode.Quads);
		        if((segmentFlags & EightSegment.Top) != 0)
                {
			        GL.Vertex2(x        ,y + height               );
			        GL.Vertex2(x + width,y + height               );
			        GL.Vertex2(x + width,y + height - segmentWidth);	
			        GL.Vertex2(x        ,y + height - segmentWidth);		
		        }
		        if((segmentFlags & EightSegment.TopRight) != 0)
                {
			        GL.Vertex2(x + width - segmentWidth,y + height);
			        GL.Vertex2(x + width               ,y + height);
			        GL.Vertex2(x + width               ,y + halfHeight);
			        GL.Vertex2(x + width - segmentWidth,y + halfHeight);
		        }
		        if((segmentFlags & EightSegment.BottomRight) != 0)
                {
			        GL.Vertex2(x + width - segmentWidth,y + halfHeight);
			        GL.Vertex2(x + width               ,y + halfHeight);
			        GL.Vertex2(x + width               ,y             );
			        GL.Vertex2(x + width - segmentWidth,y             );
		        }
                if ((segmentFlags & EightSegment.Bottom) != 0)
                {
			        GL.Vertex2(x        ,y               );
			        GL.Vertex2(x + width,y               );
			        GL.Vertex2(x + width,y + segmentWidth);	
			        GL.Vertex2(x        ,y + segmentWidth);		
		        }
                if ((segmentFlags & EightSegment.BottomLeft) != 0)
                {
			        GL.Vertex2(x + segmentWidth,y + halfHeight);
			        GL.Vertex2(x               ,y + halfHeight);
			        GL.Vertex2(x               ,y             );
			        GL.Vertex2(x + segmentWidth,y             );
		        }
                if ((segmentFlags & EightSegment.TopLeft) != 0)
                {
			        GL.Vertex2(x + segmentWidth,y + height);
			        GL.Vertex2(x               ,y + height);
			        GL.Vertex2(x               ,y + halfHeight);
			        GL.Vertex2(x + segmentWidth,y + halfHeight);
		        }
        		
		        float segmentHalfWidth;
                if ((segmentFlags & EightSegment.HorzCenter) != 0)
                {
			        segmentHalfWidth = segmentWidth / 2f;
			        GL.Vertex2(x        ,y + halfHeight + segmentHalfWidth);
			        GL.Vertex2(x + width,y + halfHeight + segmentHalfWidth);
			        GL.Vertex2(x + width,y + halfHeight - segmentHalfWidth);	
			        GL.Vertex2(x        ,y + halfHeight - segmentHalfWidth);		
		        }
                if ((segmentFlags & EightSegment.VertCenter) != 0)
                {
			        segmentHalfWidth = segmentWidth / 2f;
			        float widthHalf = width/2f;
			        GL.Vertex2(x + widthHalf - segmentHalfWidth,y + height);
			        GL.Vertex2(x + widthHalf + segmentHalfWidth,y + height);
			        GL.Vertex2(x + widthHalf + segmentHalfWidth,y         );	
			        GL.Vertex2(x + widthHalf - segmentHalfWidth,y         );		
		        }
            GL.End();
	    }
    }
    public class RRendererFixedSegmentWidth : EightSegmentFixedWidthRenderer
    {		
		public RRendererFixedSegmentWidth(float segmentWidth)
			: base(EightSegment.RPartial, segmentWidth)
		{
        }
        public override void DrawChar(Int32 x, Int32 y, Int32 width, Int32 height)
        {
            base.DrawChar(x, y, width, height);

            float segmentWidth = width / 5f;

			float xMiddle = x + width/2;
			float yMiddle = y + height / 2f;
			
			// Draw leg of R
			GL.Begin(BeginMode.Quads);
			
			GL.Vertex2(xMiddle                 ,yMiddle);
			GL.Vertex2(xMiddle   + segmentWidth,yMiddle);
			GL.Vertex2(x + width               ,y      );
			GL.Vertex2(x + width - segmentWidth,y      );
			
			GL.End();
		}
    }
    public class RRenderer : EightSegmentRelativeWidthRenderer
    {
        static RRenderer instance;
        public static RRenderer Instance
        {
            get { if (instance == null) instance = new RRenderer(); return instance; }
        }
        private RRenderer() : base(EightSegment.RPartial) { }
        public override void DrawChar(Int32 x, Int32 y, Int32 width, Int32 height)
        {
            base.DrawChar(x, y, width, height);

            float segmentWidth = width / 5f;

            float xMiddle = x + width / 2;
            float yMiddle = y + height / 2f;

            // Draw leg of R
            GL.Begin(BeginMode.Quads);

            GL.Vertex2(xMiddle                 , yMiddle);
            GL.Vertex2(xMiddle   + segmentWidth, yMiddle);
            GL.Vertex2(x + width               , y      );
            GL.Vertex2(x + width - segmentWidth, y      );

            GL.End();
        }
    }
    public class NRenderer : EightSegmentRelativeWidthRenderer
    {
        static NRenderer instance;
        public static NRenderer Instance
        {
            get { if (instance == null) instance = new NRenderer(); return instance; }
        }
        private NRenderer() : base(EightSegment.NPartial) { }
        public override void DrawChar(Int32 x, Int32 y, Int32 width, Int32 height)
        {
            base.DrawChar(x, y, width, height);
            float segmentWidth = width / 5;

            // Draw leg of R
            GL.Begin(BeginMode.Quads);

            GL.Vertex2(x                       , y + height);
            GL.Vertex2(x         + segmentWidth, y + height);
            GL.Vertex2(x + width               , y         );
            GL.Vertex2(x + width - segmentWidth, y         );

            GL.End();
        }
    }
    public class NRendererFixedSegmentWidth : EightSegmentFixedWidthRenderer
    {		
		public NRendererFixedSegmentWidth(float segmentWidth)
			: base(EightSegment.NPartial, segmentWidth)
		{
        }
        public override void DrawChar(Int32 x, Int32 y, Int32 width, Int32 height)
        {
            base.DrawChar(x, y, width, height);
			
			// Draw leg of R
			GL.Begin(BeginMode.Quads);
			
			GL.Vertex2(x                       ,y + height);
			GL.Vertex2(x   + segmentWidth      ,y + height);
			GL.Vertex2(x + width               ,y      );
			GL.Vertex2(x + width - segmentWidth,y      );
			
			GL.End();
		}
    }
    public class YRendererFixedSegmentWidth
    {
        public float segmentWidth;
        private float segmentHalfWidth;

        public YRendererFixedSegmentWidth(float segmentWidth)
        {
            this.segmentWidth = segmentWidth;
            this.segmentHalfWidth = segmentWidth / 2;
        }
        public void DrawChar(Int32 x, Int32 y, Int32 width, Int32 height)
        {
            float xMiddle = x + width / 2f;
            float yMiddle = y + height / 2f;

            GL.Begin(BeginMode.Quads);

            GL.Vertex2(xMiddle - segmentHalfWidth, yMiddle);
            GL.Vertex2(xMiddle + segmentHalfWidth, yMiddle);
            GL.Vertex2(xMiddle + segmentHalfWidth, y);
            GL.Vertex2(xMiddle - segmentHalfWidth, y);

            GL.Vertex2(x, y + height);
            GL.Vertex2(x + segmentWidth, y + height);
            GL.Vertex2(xMiddle + segmentHalfWidth, yMiddle);
            GL.Vertex2(xMiddle - segmentHalfWidth, yMiddle);

            GL.Vertex2(x + width - segmentWidth, y + height);
            GL.Vertex2(x + width, y + height);
            GL.Vertex2(xMiddle + segmentHalfWidth, yMiddle);
            GL.Vertex2(xMiddle - segmentHalfWidth, yMiddle);

            GL.End();
        }
    }
    public static class VRenderer
    {
        public static void DrawChar(Int32 x, Int32 y, Int32 width, Int32 height)
        {
            float segmentWidth = width / 5f;
            float segmentHalfWidth = segmentWidth / 2;
            float xMiddle = x + width / 2f;

            // Draw leg of R
            GL.Begin(BeginMode.Quads);

            GL.Vertex2(x, y + height);
            GL.Vertex2(x + segmentWidth, y + height);
            GL.Vertex2(xMiddle + segmentHalfWidth, y);
            GL.Vertex2(xMiddle - segmentHalfWidth, y);

            GL.Vertex2(x + width - segmentWidth, y + height);
            GL.Vertex2(x + width, y + height);
            GL.Vertex2(xMiddle + segmentHalfWidth, y);
            GL.Vertex2(xMiddle - segmentHalfWidth, y);

            GL.End();
        }
    }
    public class VRendererFixedSegmentWidth
    {
		public float segmentWidth;
		public float segmentHalfWidth;
		
		public VRendererFixedSegmentWidth(float segmentWidth) {
			this.segmentWidth = segmentWidth;
			this.segmentHalfWidth = segmentWidth / 2f;
        }
        public void DrawChar(Int32 x, Int32 y, Int32 width, Int32 height)
        {
			float xMiddle = x + width / 2f;
			
			// Draw leg of R
			GL.Begin(BeginMode.Quads);
			
			GL.Vertex2(x                         ,y + height);
			GL.Vertex2(x       + segmentWidth    ,y + height);
			GL.Vertex2(xMiddle + segmentHalfWidth,y      );
			GL.Vertex2(xMiddle - segmentHalfWidth,y      );

			GL.Vertex2(x + width - segmentWidth,y + height);
			GL.Vertex2(x + width               ,y + height);
			GL.Vertex2(xMiddle + segmentHalfWidth,y      );
			GL.Vertex2(xMiddle - segmentHalfWidth,y      );
			
			GL.End();
		}
    }
    public static class PeriodRenderer
    {
        public static void DrawChar(Int32 x, Int32 y, Int32 width, Int32 height)
        {
            float segmentWidth = width / 5f;
            float segmentHalfWidth = segmentWidth / 2;
            float xMiddle = x + width / 2f;

            GL.Begin(BeginMode.Quads);
            GL.Vertex2(xMiddle - segmentHalfWidth, y + segmentWidth);
            GL.Vertex2(xMiddle + segmentHalfWidth, y + segmentWidth);
            GL.Vertex2(xMiddle + segmentHalfWidth, y);
            GL.Vertex2(xMiddle - segmentHalfWidth, y);
            GL.End();
        }
    }
    public class PeriodRendererFixedSegmentWidth
    {		
		public float segmentWidth;
		private float segmentHalfWidth;

        public PeriodRendererFixedSegmentWidth(float segmentWidth)
        {
			this.segmentWidth = segmentWidth;
			this.segmentHalfWidth = segmentWidth / 2;
        }
        public void DrawChar(Int32 x, Int32 y, Int32 width, Int32 height)
        {
			float xMiddle = x + width / 2f;
			
			GL.Begin(BeginMode.Quads);
			GL.Vertex2(xMiddle - segmentHalfWidth,y + segmentWidth);
			GL.Vertex2(xMiddle + segmentHalfWidth,y + segmentWidth);
			GL.Vertex2(xMiddle + segmentHalfWidth,y);
			GL.Vertex2(xMiddle - segmentHalfWidth,y);
			GL.End();
		}		
	}






    public class MRenderer : EightSegmentRelativeWidthRenderer
    {
        public MRenderer()
            : base(EightSegment.MPartial)
        {
        }
        public override void DrawChar(Int32 x, Int32 y, Int32 width, Int32 height)
        {
            base.DrawChar(x, y, width, height);

            float segmentWidth = width / 5f;
            float xMiddle = x + width / 2;
            float yMiddle = y + height / 2f;

            GL.Begin(BeginMode.QuadStrip);

            GL.Vertex2(x, y + height - segmentWidth);
            GL.Vertex2(x, y + height);
            GL.Vertex2(xMiddle, yMiddle - segmentWidth);
            GL.Vertex2(xMiddle, yMiddle);
            GL.Vertex2(x + width, y + height - segmentWidth);
            GL.Vertex2(x + width, y + height);

            GL.End();
        }
    }
    public static class XRenderer
    {
        public static void DrawChar(Int32 x, Int32 y, Int32 width, Int32 height)
        {
            float segmentWidth = width / 5f;

            GL.Begin(BeginMode.Quads);

            GL.Vertex2(x                       , y + height);
            GL.Vertex2(x         + segmentWidth, y + height);
            GL.Vertex2(x + width               , y         );
            GL.Vertex2(x + width - segmentWidth, y         );

            GL.Vertex2(x + width - segmentWidth, y + height);
            GL.Vertex2(x + width               , y + height);
            GL.Vertex2(x         + segmentWidth, y         );
            GL.Vertex2(x                       , y         );

            GL.End();
        }
    }


























    //
    //
    //
    public static class MarlerOnePixelLineCurveRenderer
    {
        private static CharacterRenderFunction[] renderFunctions = null;
        public static CharacterRenderFunction[] RenderFunctions
        {
            get
            {
                if (renderFunctions == null)
                {
                    renderFunctions = new CharacterRenderFunction[256];

                    renderFunctions['1'] = One;
                    renderFunctions['2'] = Two;
                    renderFunctions['3'] = Three;
                    renderFunctions['4'] = Four;

                    renderFunctions['Z'] = Z;
                }
                return renderFunctions;
            }
        }
        /*
        static void Zero(float x, float y, float width, float height)
        {
            GL.Begin(BeginMode.LineStrip);
            for (float i = 0; i <= 2 * Math.PI + .01; i += (2f * Math.PI / 10f))
            {
                //GL.Vertex2(
            }
            //GL.Vertex2(x, y + height);
            //GL.Vertex2(x + width, y + height);
            //GL.Vertex2(x, y);
            //GL.Vertex2(x + width, y);
            GL.End();
        }
        */
        static void One(Int32 x, Int32 y, Int32 width, Int32 height)
        {
            float halfWidth = width / 2f;
            float quarterWidth = halfWidth / 2f;

            GL.Begin(BeginMode.LineStrip);
                GL.Vertex2(x + quarterWidth, y + height - quarterWidth);
                GL.Vertex2(x + halfWidth   , y + height);
                GL.Vertex2(x + halfWidth   , y         );
            GL.End();
        }
        static void Two(Int32 x, Int32 y, Int32 width, Int32 height)
        {
            float halfWidth = width / 2f;
            float quarterWidth = width / 4f;

            float halfHeight = height / 2f;
            float quarterHeight = height / 4f;

            GL.Begin(BeginMode.LineStrip);
            GL.Vertex2(x                       , y + height - quarterWidth);
            GL.Vertex2(x         + quarterWidth, y + height               );
            GL.Vertex2(x + width - quarterWidth, y + height               );
            GL.Vertex2(x + width               , y + height - quarterWidth);
            GL.Vertex2(x + width               , y + halfHeight + quarterWidth);
            GL.Vertex2(x                       , y                         );
            GL.Vertex2(x + width               , y                         );
            GL.End();
        }
        static void Three(Int32 x, Int32 y, Int32 width, Int32 height)
        {
            float halfWidth = width / 2f;
            float quarterWidth = width / 4f;

            float halfHeight = height / 2f;
            float quarterHeight = height / 4f;

            GL.Begin(BeginMode.LineStrip);
            GL.Vertex2(x                       , y + height - quarterWidth);
            GL.Vertex2(x         + quarterWidth, y + height               );
            GL.Vertex2(x + width - quarterWidth, y + height               );
            GL.Vertex2(x + width               , y + height - quarterWidth);
            GL.Vertex2(x + width               , y + halfHeight + quarterWidth);
            GL.Vertex2(x + width - quarterWidth, y + halfHeight                );
            GL.Vertex2(x + halfWidth           , y + halfHeight                ); // middle
            GL.Vertex2(x + width - quarterWidth, y + halfHeight                );
            GL.Vertex2(x + width               , y + halfHeight - quarterWidth);
            GL.Vertex2(x + width               , y +        + quarterWidth);
            GL.Vertex2(x + width - quarterWidth, y                        );
            GL.Vertex2(x         + quarterWidth, y                        );
            GL.Vertex2(x                       , y          + quarterWidth);
            GL.End();
        }
        static void Four(Int32 x, Int32 y, Int32 width, Int32 height)
        {
            float halfHeight = height / 2f;

            GL.Begin(BeginMode.LineStrip);
            GL.Vertex2(x, y + height);
            GL.Vertex2(x, y + halfHeight);
            GL.Vertex2(x + width, y + halfHeight);
            GL.End();
            GL.Begin(BeginMode.Lines);
            GL.Vertex2(x + width, y + height);
            GL.Vertex2(x + width, y );
            GL.End();
        }




        static void Z(Int32 x, Int32 y, Int32 width, Int32 height)
        {
            GL.Begin(BeginMode.LineStrip);
            GL.Vertex2(x        , y + height);
            GL.Vertex2(x + width, y + height);
            GL.Vertex2(x        , y         );
            GL.Vertex2(x + width, y         );
            GL.End();
        }
    }


}
