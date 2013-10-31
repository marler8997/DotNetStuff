using System;

using More.OpenTK;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace TestGui
{
    class TestGuiGameWindow : CustomGameWindow
    {
        static void Main(string[] args)
        {
            new TestGuiGameWindow().CustomGameLoop(null);
        }

        IndependentContentSizeBox windowBox;
        IndependentContentSizeBox topLeftBox, topRightBox, bottomRightBox, bottomLeftBox, centerBox;

        public TestGuiGameWindow()
            : base(800, 600, GraphicsMode.Default, "TestGui")
        {
            CharacterRenderMapFont smallFont = new CharacterRenderMapFont(
            CharacterRenderSetFactory.VariableLengthSegmentRenderers, 7, 14, 3);

            Label topLeftLabel = new Label(smallFont, "Top Left");
            topLeftLabel.backgroundSetting = Color4.Red;
            topLeftLabel.SetPadding(10);
            topLeftLabel.SetBorderWidth(5);
            topLeftLabel.borderColor = Color4.BlueViolet;


            Label topRightLabel = new Label(smallFont, "Top Right");
            topRightLabel.backgroundSetting = Color4.Green;
            topRightLabel.SetPadding(10);

            Label bottomRightLabel = new Label(smallFont, "Bottom Right");
            bottomRightLabel.backgroundSetting = Color4.Blue;
            bottomRightLabel.fontColor = Color4.White;
            bottomRightLabel.SetPadding(10);

            Label bottomLeftLabel = new Label(smallFont, "Bottom Left");
            bottomLeftLabel.backgroundSetting = Color4.Yellow;
            bottomLeftLabel.SetPadding(10);

            Label centerLabel = new Label(smallFont, "Center");
            centerLabel.backgroundSetting = Color4.BurlyWood;
            centerLabel.SetPadding(10);


            topLeftBox = new IndependentContentSizeBox(topLeftLabel);
            topLeftBox.SetComponentLocation(10, 400);
            topLeftBox.SetBorderWidth(4);
            topLeftBox.SetContentSize(300, 150);
            topLeftBox.SetChildRelativeLoactionX(AlignX.Right, 10);
            topLeftBox.SetChildRelativeLoactionY(AlignY.Bottom, 10);

            topRightBox = new IndependentContentSizeBox(topRightLabel);
            topRightBox.SetComponentLocation(400, 400);
            topRightBox.SetBorderWidth(5);
            topRightBox.SetContentSize(300, 150);
            topRightBox.SetChildRelativeLoactionX(AlignX.Left, 10);
            topRightBox.SetChildRelativeLoactionY(AlignY.Bottom, 10);

            bottomRightBox = new IndependentContentSizeBox(bottomRightLabel);
            bottomRightBox.SetComponentLocation(400, 10);
            bottomRightBox.SetBorderWidth(8);
            bottomRightBox.SetContentSize(300, 150);
            bottomRightBox.SetChildRelativeLoactionX(AlignX.Left, 10);
            bottomRightBox.SetChildRelativeLoactionY(AlignY.Top, 10);

            bottomLeftBox = new IndependentContentSizeBox(bottomLeftLabel);
            bottomLeftBox.SetComponentLocation(10, 10);
            bottomLeftBox.SetBorderWidth(20);
            bottomLeftBox.SetContentSize(300, 150);
            bottomLeftBox.SetChildRelativeLoactionX(AlignX.Right, 10);
            bottomLeftBox.SetChildRelativeLoactionY(AlignY.Top, 10);

            centerBox = new IndependentContentSizeBox(centerLabel);
            centerBox.SetComponentLocation(200, 200);
            centerBox.SetBorderWidth(3);
            centerBox.SetContentSize(300, 150);
            centerBox.SetChildRelativeLoactionX(AlignX.Center);
            centerBox.SetChildRelativeLoactionY(AlignY.Middle);


            //
            // Setup OpenGL
            //
            GL.ClearColor(Color4.Wheat);
            //
            // Enable Transparency
            //
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);


        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
        }

        public override void Initialize(long nowMicros)
        {
        }

        public override bool Update(long nowMicros, int diffMicros)
        {
            if (Keyboard[Key.Escape]) return true;

            //topLeftBox.SetLocation(10, 500, Corner.TopLeft);
            topLeftBox.CalculateRenderVariables(null);

            //topRightBox.SetLocation(500, 500, Corner.TopRight);
            topRightBox.CalculateRenderVariables(null);

            //bottomRightBox.SetLocation(10, 500, Corner.BottomRight);
            bottomRightBox.CalculateRenderVariables(null);

            //bottomLeftBox.SetLocation(10, 10, Corner.BottomLeft);
            bottomLeftBox.CalculateRenderVariables(null);

            centerBox.CalculateRenderVariables(null);

            return false;
        }
        public override void Render()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Ortho(0, ClientRectangle.Width, 0, ClientRectangle.Height, -100, 100);

            topLeftBox.DrawComponent();
            topRightBox.DrawComponent();
            bottomRightBox.DrawComponent();
            bottomLeftBox.DrawComponent();
            centerBox.DrawComponent();
        }
    }
}
