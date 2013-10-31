using System;
using System.Collections.Generic;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace More.OpenTK
{




    public enum AlignX { Left, Center, Right }
    public enum AlignY { Bottom, Middle, Top }
    /*
    public class Window
    {
        public WindowStruct windowStruct;
        public Window()
        {
        }
        public Window(WindowStruct windowStruct)
        {

        }
    }
    */
    public struct WindowStruct
    {
        public Int32 x, y, width, height;
        public WindowStruct(Int32 x, Int32 y, Int32 width, Int32 height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
        public WindowStruct Intersection(WindowStruct other)
        {
            throw new NotImplementedException();
            if (x == other.x)
            {
                if (y == other.y)
                {
                    return new WindowStruct(x, y,
                        (width <= other.width) ? width : other.width,
                        (height <= other.height) ? height : other.height);
                }
                else if (y < other.y)
                {

                }
                else
                {

                }
            }



            return new WindowStruct();
        }
    }
    public struct SquareWidth
    {
        public static readonly SquareWidth Zero = new SquareWidth(0);

        public Int32 top,right,bottom,left;
        public SquareWidth(Int32 width)
        {
            this.top = width;
            this.right = width;
            this.bottom = width;
            this.left = width;
        }
        public SquareWidth(Int32 topAndBottom, Int32 leftAndRight)
        {
            this.top = topAndBottom;
            this.bottom = topAndBottom;
            this.left = leftAndRight;
            this.right = leftAndRight;
        }
        public SquareWidth(Int32 top, Int32 right, Int32 bottom, Int32 left)
        {
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.left = left;
        }
    }
    public struct ChildRelativeLocationX
    {
        public AlignX parentAlign;
        public Int32 offset;
        public AlignX childAlign;
        public ChildRelativeLocationX(AlignX parentAndChildAlignX)
        {
            this.parentAlign = parentAndChildAlignX;
            this.offset = 0;
            this.childAlign = parentAndChildAlignX;
        }
        public ChildRelativeLocationX(AlignX parentAndChildAlignX, Int32 offset)
        {
            this.parentAlign = parentAndChildAlignX;
            this.offset = offset;
            this.childAlign = parentAndChildAlignX;
        }
        public ChildRelativeLocationX(AlignX parentAlignX, AlignX childAlignX)
        {
            this.parentAlign = parentAlignX;
            this.offset = 0;
            this.childAlign = childAlignX;
        }
        public ChildRelativeLocationX(AlignX parentAlignX, Int32 offset, AlignX childAlignX)
        {
            this.parentAlign = parentAlignX;
            this.offset = offset;
            this.childAlign = childAlignX;
        }
        public Int32 GetChildComponentX(WindowStruct parentContentWindow, Int32 childComponentWindowWidth)
        {
            Int32 parentXOffset = parentContentWindow.x +
                ( (parentAlign == AlignX.Right) ? -offset : offset);

            switch(parentAlign)
            {
            case AlignX.Left: switch (childAlign) {
                case AlignX.Left  : return parentXOffset;
                case AlignX.Center: return parentXOffset                                  - childComponentWindowWidth  / 2;
                case AlignX.Right : return parentXOffset                                  - childComponentWindowWidth     ;
            } throw new InvalidOperationException();
            case AlignX.Center: switch (childAlign) {
                case AlignX.Left  : return parentXOffset +  parentContentWindow.width / 2                                 ;
                case AlignX.Center: return parentXOffset + (parentContentWindow.width     - childComponentWindowWidth) / 2;
                case AlignX.Right : return parentXOffset +  parentContentWindow.width / 2 - childComponentWindowWidth     ;
            } throw new InvalidOperationException();
            case AlignX.Right: switch (childAlign) {
                case AlignX.Left  : return parentXOffset +  parentContentWindow.width                                     ;
                case AlignX.Center: return parentXOffset +  parentContentWindow.width     - childComponentWindowWidth  / 2;
                case AlignX.Right : return parentXOffset +  parentContentWindow.width     - childComponentWindowWidth     ;
            } throw new InvalidOperationException();
            }
            throw new InvalidOperationException();
        }
        public override String ToString()
        {
            if (parentAlign == childAlign)
            {
                if (offset == 0) return parentAlign.ToString();
                return parentAlign + " " + offset.ToString();
            }
            if (offset == 0) return parentAlign.ToString() + " " + childAlign.ToString();
                return parentAlign + " " + offset.ToString() + " " + childAlign.ToString();
        }
    }
    public struct ChildRelativeLocationY
    {
        public AlignY parentAlign;
        public Int32 offset;
        public AlignY childAlign;
        public ChildRelativeLocationY(AlignY parentAndChildAlignY)
        {
            this.parentAlign = parentAndChildAlignY;
            this.offset = 0;
            this.childAlign = parentAndChildAlignY;
        }
        public ChildRelativeLocationY(AlignY parentAndChildAlignY, Int32 offset)
        {
            this.parentAlign = parentAndChildAlignY;
            this.offset = offset;
            this.childAlign = parentAndChildAlignY;
        }
        public ChildRelativeLocationY(AlignY parentAlignY, AlignY childAlignY)
        {
            this.parentAlign = parentAlignY;
            this.offset = 0;
            this.childAlign = childAlignY;
        }
        public ChildRelativeLocationY(AlignY parentAlignY, Int32 offset, AlignY childAlignY)
        {
            this.parentAlign = parentAlignY;
            this.offset = offset;
            this.childAlign = childAlignY;
        }
        public Int32 GetChildComponentY(WindowStruct parentContentWindow, Int32 childComponentWindowHeight)
        {
            Int32 parentYOffset = parentContentWindow.y +
                ( (parentAlign == AlignY.Top) ? -offset : offset);

            switch(parentAlign)
            {
            case AlignY.Bottom: switch (childAlign) {
                case AlignY.Bottom: return parentYOffset;
                case AlignY.Middle: return parentYOffset                                  - childComponentWindowHeight  / 2;
                case AlignY.Top   : return parentYOffset                                  - childComponentWindowHeight     ;
            } throw new InvalidOperationException();
            case AlignY.Middle: switch (childAlign) {
                case AlignY.Bottom: return parentYOffset +  parentContentWindow.height / 2                                 ;
                case AlignY.Middle: return parentYOffset + (parentContentWindow.height     - childComponentWindowHeight) / 2;
                case AlignY.Top   : return parentYOffset +  parentContentWindow.height / 2 - childComponentWindowHeight     ;
            } throw new InvalidOperationException();
            case AlignY.Top: switch (childAlign) {
                case AlignY.Bottom: return parentYOffset +  parentContentWindow.height                                     ;
                case AlignY.Middle: return parentYOffset +  parentContentWindow.height     - childComponentWindowHeight  / 2;
                case AlignY.Top   : return parentYOffset +  parentContentWindow.height     - childComponentWindowHeight     ;
            } throw new InvalidOperationException();
            }
            throw new InvalidOperationException();
        }
        public override String ToString()
        {
            if (parentAlign == childAlign)
            {
                if (offset == 0) return parentAlign.ToString();
                return parentAlign + " " + offset.ToString();
            }
            if (offset == 0) return parentAlign.ToString() + " " + childAlign.ToString();
            return parentAlign + " " + offset.ToString() + " " + childAlign.ToString();
        }
    }




    //
    // Components whose content sizes are not affected by the sizes of any other components (child/parent/sibling)
    //    1. Label
    
    //
    // Different Tyes of components
    //   1. FixedBox: The content size of the box is not depenent on any other components
    //   2. WrapperBox: The content size of the box is dependent on the size of the children components
    //   3. FillBox: The content size of the box is dependent on the available space from the parent
    //

    public enum ContentSizeType
    {
        Independent, // The content size of the component is not dependent on any other components
                     // so during the render process, it's size is already determined
        FillX,
        FillY,
        FillBoth,
        ChildDependent,
    }

    public abstract class Component
    {
        //
        //                                   Component Window
        //      -----------------------------------------------------------------------------
        //     |                                 Border                                      |
        //     |   -----------------------------------------------------------------------   |
        //     |  |                              Padding                                  |  |
        //     |  |   -----------------------------------------------------------------   |  |
        //     |  |  |                                                                 |  |  |
        //     |  |  |                                                                 |  |  |
        //     |  |  |                                                                 |  |  |
        //     |  |  |                                                                 |  |  |
        //     |  |  |                        ContentWindow                            |  |  |
        //     |  |  |                                                                 |  |  |
        //     |  |  |                                                                 |  |  |
        //     |  |  |                                                                 |  |  |
        //     |  |  |                                                                 |  |  |
        //     |  |  |                                                                 |  |  |
        //     |  |  |                                                                 |  |  |
        //     |  |   -----------------------------------------------------------------   |  |
        //     |  |                                                                       |  |
        //     |   -----------------------------------------------------------------------   |
        //     |                                                                             |
        //      -----------------------------------------------------------------------------
        //
        public class ComponentAndContentWindow
        {
            readonly Component componentReference;

            //
            // For the Location and Size pair, only one of each pair may be true at a time
            // If one is set, it will determine wheither changing the border/padding will recalculate the component or content window
            //
            System.Boolean componentLocationSet;
            System.Boolean contentLocationSet  ;
            
            System.Boolean componentSizeSet    ;
            System.Boolean contentSizeSet      ;

            //
            // These public fields are only meant to be read
            // To write to them the Set methods should be used
            //
            internal WindowStruct component;
            internal WindowStruct content;

            internal ComponentAndContentWindow(Component componentReference)
            {
                this.componentReference = componentReference;

                this.componentLocationSet = false;
                this.contentLocationSet   = false;
                this.componentSizeSet     = false;
                this.contentSizeSet       = false;

                this.component = new WindowStruct();
                this.content = new WindowStruct();
            }

            internal void BorderOrPaddingChanged()
            {
                SquareWidth border = componentReference.border;
                SquareWidth padding = componentReference.padding;

                if (componentLocationSet)
                {
                    content.x = component.x + border.left + padding.left;
                    content.y = component.y + border.bottom + padding.bottom;
                }
                else if(contentLocationSet)
                {
                    component.x = content.x - padding.left - border.left;
                    component.y = content.y - padding.bottom - border.bottom;
                }

                if(componentSizeSet)
                {
                    content.width = component.width - border.left - border.right - padding.left - padding.right;
                    content.height = component.height - border.bottom - border.top - padding.bottom - padding.top;
                }
                else if(contentLocationSet)
                {
                    component.width = content.width + border.left + border.right + padding.left + padding.right;
                    component.height = content.height + border.bottom + border.top + padding.bottom + padding.top;
                }
            }

            internal void SetComponentLocation(Int32 x, Int32 y)
            {
                if(contentLocationSet) throw new InvalidOperationException("You cannot set the components 'ComponentLocation' after setting its 'ContentLocation', you can only set one or the other");
                componentLocationSet = true;

                component.x = x;
                component.y = y;

                SquareWidth border = componentReference.border;
                SquareWidth padding = componentReference.padding;
                content.x = component.x + border.left + padding.left;
                content.y = component.y + border.bottom + padding.bottom;
            }
            internal void SetContentLocation(Int32 x, Int32 y)
            {
                if (componentLocationSet) throw new InvalidOperationException("You cannot set the components 'ContentLocation' after setting its 'ComponentLocation', you can only set one or the other");
                contentLocationSet = true;

                content.x = x;
                content.y = y;

                SquareWidth border = componentReference.border;
                SquareWidth padding = componentReference.padding;
                component.x = content.x - padding.left - border.left;
                component.y = content.y - padding.bottom - border.bottom;
            }
            internal void SetComponentSize(Int32 width, Int32 height)
            {
                if(contentSizeSet) throw new InvalidOperationException("You cannot set the components 'ComponentSize' after setting its 'ContentSize', you can only set one or the other");
                componentSizeSet = true;

                component.width = width;
                component.height = height;

                SquareWidth border = componentReference.border;
                SquareWidth padding = componentReference.padding;
                content.width = component.width - border.left - border.right - padding.left - padding.right;
                content.height = component.height - border.bottom - border.top - padding.bottom - padding.top;
            }
            internal void SetContentSize(Int32 width, Int32 height)
            {
                if(componentSizeSet) throw new InvalidOperationException("You cannot set the components 'ContentSize' after setting its 'ComponentSize', you can only set one or the other");
                contentSizeSet = true;

                content.width = width;
                content.height = height;

                SquareWidth border = componentReference.border;
                SquareWidth padding = componentReference.padding;
                component.width = content.width + border.left + border.right + padding.left + padding.right;
                component.height = content.height + border.bottom + border.top + padding.bottom + padding.top;
            }
        }

        internal readonly ContentSizeType contentSizeType;

        public Color4 backgroundSetting        = Color4.Transparent;
        public Color4 borderColor              = Color4.Black;

        protected SquareWidth border  = SquareWidth.Zero;
        protected SquareWidth padding = SquareWidth.Zero;
        internal readonly ComponentAndContentWindow window;

        internal WindowStruct renderWindow;

        public Component(ContentSizeType contentSizeType)
        {
            this.contentSizeType = contentSizeType;
            this.window = new ComponentAndContentWindow(this);
        }
        //
        //
        //
        public void SetComponentLocation(Int32 x, Int32 y)
        {
            window.SetComponentLocation(x, y);
        }
        public void SetContentLocation(Int32 x, Int32 y)
        {
            window.SetContentLocation(x, y);
        }
        public virtual void SetComponentSize(Int32 width, Int32 height)
        {
            if (contentSizeType != ContentSizeType.Independent)
                throw new InvalidOperationException(String.Format("You can only call SetComponentSize on a component with an 'Independent' content size, but this component content size is '{0}'", contentSizeType));
            window.SetComponentSize(width, height);
        }
        public virtual void SetContentSize(Int32 width, Int32 height)
        {
            if (contentSizeType != ContentSizeType.Independent)
                throw new InvalidOperationException(String.Format("You can only call SetContentSize on a component with an 'Independent' content size, but this component content size is '{0}'", contentSizeType));
            window.SetContentSize(width, height);
        }

        //
        // Border Width Functions
        //
        public void SetBorderWidth(Int32 width)
        {
            border.top = width;
            border.right = width;
            border.bottom = width;
            border.left = width;
            window.BorderOrPaddingChanged();
        }
        public void SetBorderWidth(Int32 topAndBottom, Int32 leftAndRight)
        {
            border.top = topAndBottom;
            border.bottom = topAndBottom;
            border.left = leftAndRight;
            border.right = leftAndRight;
            window.BorderOrPaddingChanged();
        }
        public void SetBorderWidth(Int32 top, Int32 right, Int32 bottom, Int32 left)
        {
            border.top = top;
            border.right = right;
            border.bottom = bottom;
            border.left = left;
            window.BorderOrPaddingChanged();
        }

        //
        // Padding Functions
        //
        public void SetPadding(Int32 width)
        {
            padding.top = width;
            padding.right = width;
            padding.bottom = width;
            padding.left = width;
            window.BorderOrPaddingChanged();
        }
        public void SetPadding(Int32 topAndBottom, Int32 leftAndRight)
        {
            padding.top = topAndBottom;
            padding.bottom = topAndBottom;
            padding.left = leftAndRight;
            padding.right = leftAndRight;
            window.BorderOrPaddingChanged();
        }
        public void SetPadding(Int32 top, Int32 right, Int32 bottom, Int32 left)
        {
            padding.top = top;
            padding.right = right;
            padding.bottom = bottom;
            padding.left = left;
            window.BorderOrPaddingChanged();
        }


        //
        //
        //
        public abstract void CalculateRenderVariables(Component parent);

        protected abstract void DrawContent();
        public void DrawComponent()
        {
            WindowStruct componentWindow = window.component;
            
            Console.WriteLine("Component {0} Component {1},{2} {3}x{4} Content {5},{6} {7}x{8}",
                GetType().Name,
                window.component.x, window.component.y, window.component.width, window.component.height,
                window.content.x, window.content.y, window.content.width, window.content.height);
            
            //
            // Draw the border
            //
            if (border.top > 0 || border.right > 0 || border.bottom > 0 || border.left > 0)
            {
                //
                // Check if there is any content for the border
                //
                if(componentWindow.width > 0 || componentWindow.height > 0)
                {
                    GL.Begin(BeginMode.Quads);
                        GL.Color4(borderColor);
                        if (border.top > 0)
                        {
                            GL.Vertex2(componentWindow.x                        , componentWindow.y + componentWindow.height - border.top); // BottomLeft
                            GL.Vertex2(componentWindow.x                        , componentWindow.y + componentWindow.height                       ); // TopLeft
                            GL.Vertex2(componentWindow.x + componentWindow.width  , componentWindow.y + componentWindow.height                       ); // TopRight
                            GL.Vertex2(componentWindow.x + componentWindow.width, componentWindow.y + componentWindow.height - border.top); // BottomRight
                        }
                        if (border.right > 0)
                        {
                            GL.Vertex2(componentWindow.x + componentWindow.width - border.right, componentWindow.y); // BottomLeft
                            GL.Vertex2(componentWindow.x + componentWindow.width - border.right, componentWindow.y + componentWindow.height); // TopLeft
                            GL.Vertex2(componentWindow.x + componentWindow.width                         , componentWindow.y + componentWindow.height); // TopRight
                            GL.Vertex2(componentWindow.x + componentWindow.width                         , componentWindow.y                       ); // BottomRight
                        }
                        if (border.bottom > 0)
                        {
                            GL.Vertex2(componentWindow.x                        , componentWindow.y                          ); // BottomLeft
                            GL.Vertex2(componentWindow.x                        , componentWindow.y + border.bottom); // TopLeft
                            GL.Vertex2(componentWindow.x + componentWindow.width, componentWindow.y + border.bottom); // TopRight
                            GL.Vertex2(componentWindow.x + componentWindow.width  , componentWindow.y                          ); // BottomRight
                        }
                        if (border.left > 0)
                        {
                            GL.Vertex2(componentWindow.x                        , componentWindow.y                       ); // BottomLeft
                            GL.Vertex2(componentWindow.x                        , componentWindow.y + componentWindow.height); // TopLeft
                            GL.Vertex2(componentWindow.x + border.left, componentWindow.y + componentWindow.height); // TopRight
                            GL.Vertex2(componentWindow.x + border.left, componentWindow.y); // BottomRight
                        }
                    GL.End();
                }
            }

            //
            // Draw the background
            //
            if (backgroundSetting != Color4.Transparent)
            {
                if (componentWindow.width > 0 && componentWindow.height > 0)
                {
                    GL.Color4(backgroundSetting);
                    GL.Begin(BeginMode.Quads);
                        GL.Vertex2(componentWindow.x + border.left, componentWindow.y + border.bottom); // BottomLeft
                        GL.Vertex2(componentWindow.x + border.left, componentWindow.y + componentWindow.height - border.top); // TopLeft
                        GL.Vertex2(componentWindow.x + componentWindow.width - border.right, componentWindow.y + componentWindow.height - border.top); // TopRight
                        GL.Vertex2(componentWindow.x + componentWindow.width - border.right, componentWindow.y + border.bottom); // BottomRight
                    GL.End();
                }
            }

            //
            // Call the classes DrawPaddedContent to draw the inner content of the component
            //
            DrawContent();
        }
    }
    public class Label : Component
    {
        Font font;
        String text;
        public Color4 fontColor = Color4.Black;

        public Label(Font font)
            : base(ContentSizeType.Independent)
        {
            this.font = font;
            UpdateContentSizeFromFontOrText();
        }
        public Label(Font font, String text)
            : base(ContentSizeType.Independent)
        {
            this.font = font;
            this.text = text;
            UpdateContentSizeFromFontOrText();
        }

        public void SetText(String text)
        {
            this.text = text;
            UpdateContentSizeFromFontOrText();
        }

        public void SetFontColor(Color4 fontColor)
        {
            this.fontColor = fontColor;
        }
        public override void SetComponentSize(int width, int height)
        {
            throw new InvalidOperationException("You cannot call SetComponentSize on a label because the component size is dependent on the content size which is determined by the font/text");
        }
        public override void SetContentSize(int width, int height)
        {
            throw new InvalidOperationException("You cannot call SetContentSize on a label because the content size is determined by the font/text");
        }

        void UpdateContentSizeFromFontOrText()
        {
            if(font == null || text == null)
            {
                window.SetContentSize(0, 0);
            }
            else
            {
                window.SetContentSize(font.GetWidth(text.Length), font.charHeight);
            }
        }
        public override void CalculateRenderVariables(Component parent)
        {
        }
        protected override void DrawContent()
        {
            if (font != null && text != null)
            {
                GL.Color4(fontColor);
                font.Draw(text, window.content.x, window.content.y);
            }
        }
    }
    public class IndependentContentSizeBox : Component
    {
        Component child;
        ChildRelativeLocationX childRelativeLocationX;
        ChildRelativeLocationY childRelativeLocationY;

        public IndependentContentSizeBox()
            : base(ContentSizeType.Independent)
        {
        }
        public IndependentContentSizeBox(Component child)
            : base(ContentSizeType.Independent)
        {
            this.child = child;
        }

        //
        // Child Relative Location X
        //
        public void SetChildRelativeLoactionX(AlignX parentAndChildAlignX)
        {
            childRelativeLocationX.parentAlign = parentAndChildAlignX;
            childRelativeLocationX.offset = 0;
            childRelativeLocationX.childAlign = parentAndChildAlignX;
        }
        public void SetChildRelativeLoactionX(AlignX parentAndChildAlignX, Int32 offset)
        {
            childRelativeLocationX.parentAlign = parentAndChildAlignX;
            childRelativeLocationX.offset = offset;
            childRelativeLocationX.childAlign = parentAndChildAlignX;
        }
        public void SetChildRelativeLoactionX(AlignX parentAlignX, AlignX childAlignX)
        {
            childRelativeLocationX.parentAlign = parentAlignX;
            childRelativeLocationX.offset = 0;
            childRelativeLocationX.childAlign = childAlignX;
        }
        public void SetChildRelativeLoactionX(AlignX parentAlignX, Int32 offset, AlignX childAlignX)
        {
            childRelativeLocationX.parentAlign = parentAlignX;
            childRelativeLocationX.offset = offset;
            childRelativeLocationX.childAlign = childAlignX;
        }

        //
        // Child Relative Location Y
        //
        public void SetChildRelativeLoactionY(AlignY parentAndChildAlignY)
        {
            childRelativeLocationY.parentAlign = parentAndChildAlignY;
            childRelativeLocationY.offset = 0;
            childRelativeLocationY.childAlign = parentAndChildAlignY;
        }
        public void SetChildRelativeLoactionY(AlignY parentAndChildAlignY, Int32 offset)
        {
            childRelativeLocationY.parentAlign = parentAndChildAlignY;
            childRelativeLocationY.offset = offset;
            childRelativeLocationY.childAlign = parentAndChildAlignY;
        }
        public void SetChildRelativeLoactionY(AlignY parentAlignY, AlignY childAlignY)
        {
            childRelativeLocationY.parentAlign = parentAlignY;
            childRelativeLocationY.offset = 0;
            childRelativeLocationY.childAlign = childAlignY;
        }
        public void SetChildRelativeLoactionY(AlignY parentAlignY, Int32 offset, AlignY childAlignY)
        {
            childRelativeLocationY.parentAlign = parentAlignY;
            childRelativeLocationY.offset = offset;
            childRelativeLocationY.childAlign = childAlignY;
        }
        public override void CalculateRenderVariables(Component parent)
        {
            if(child != null)
            {
                //
                // Setup child render window
                //
                // TODO need to find the intersection of the renderWindow and the contneWindow
                child.renderWindow = window.content;
                /*
                child.renderWindow.x = this.contentWindow.x + this.insetBorderWidth.left + this.padding.left;
                child.renderWindow.width = this.contentWindow.width - this.insetBorderWidth.right - this.padding.right;
                child.renderWindow.y = this.contentWindow.y + this.insetBorderWidth.bottom + this.padding.bottom;
                child.renderWindow.height = this.contentWindow.height - this.insetBorderWidth.top - this.padding.top;
                */

                //
                // Setup Child Size if it is a fill
                //
                if(child.contentSizeType == ContentSizeType.FillX || child.contentSizeType == ContentSizeType.FillBoth)
                {
                    child.window.SetComponentSize(window.content.width, window.content.height);
                }

                //
                // Setup Child content location
                //
                Int32 childComponentX = childRelativeLocationX.GetChildComponentX(window.content, child.window.component.width);
                Int32 childComponentY = childRelativeLocationY.GetChildComponentY(window.content, child.window.component.height);
                child.window.SetComponentLocation(childComponentX, childComponentY);
            }
        }
        protected override void DrawContent()
        {
            if (child != null)
            {
                child.DrawComponent();
            }
        }
    }
    public class FillSquare : Component
    {
        public FillSquare()
            : base(ContentSizeType.FillBoth)
        {
        }

        public override void CalculateRenderVariables(Component parent)
        {
            throw new NotImplementedException();
        }

        protected override void DrawContent()
        {
            throw new NotImplementedException();
        }
    }




    public class YBox : Component
    {
        System.Boolean topToBottom;
        readonly List<Component> children = new List<Component>();

        public YBox(System.Boolean topToBottom, ContentSizeType contentSizeType)
            : base(contentSizeType)
        {
            this.topToBottom = topToBottom;
        }

        public void Add(Component component)
        {
            children.Add(component);
        }
        public override void CalculateRenderVariables(Component parent)
        {





            /*
            Int32 currentChildRenderCornerY = paddingY;
            for (int i = 0; i < components.Count; i++)
            {
                Component component = components[i];
                component.renderYIsFromBottom = topToBottom ? false : true;

                component.CalculateRenderVariables(this);
                currentChildRenderCornerY += component.renderHeight;
            }
            */
        }
        protected override void DrawContent()
        {
            throw new NotImplementedException();
        }
    }
}
