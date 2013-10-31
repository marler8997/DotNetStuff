using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

using More;

using OpenTK;
using OpenTK.Graphics;

namespace More.OpenTK
{
    public abstract class CustomApplicationWindow : GameWindow
    {
        AutoResetEvent updateEvent;

        public CustomApplicationWindow(AutoResetEvent updateEvent, int width, int height, GraphicsMode mode, string title)
            : base(width, height, mode, title)
        {
            this.updateEvent = updateEvent;
        }
        public CustomApplicationWindow(AutoResetEvent updateEvent, int width, int height, GraphicsMode mode, string title, GameWindowFlags options)
            : base(width, height, mode, title, options)
        {
            this.updateEvent = updateEvent;
        }
        public CustomApplicationWindow(AutoResetEvent updateEvent, int width, int height, GraphicsMode mode, string title, GameWindowFlags options, DisplayDevice device)
            : base(width, height, mode, title, options, device)
        {
            this.updateEvent = updateEvent;
        }
        public CustomApplicationWindow(AutoResetEvent updateEvent, int width, int height, GraphicsMode mode, string title, GameWindowFlags options, DisplayDevice device, int major, int minor, GraphicsContextFlags flags)
            : base(width, height, mode, title, options, device, major, minor, flags)
        {
            this.updateEvent = updateEvent;
        }
        public CustomApplicationWindow(AutoResetEvent updateEvent, int width, int height, GraphicsMode mode, string title, GameWindowFlags options, DisplayDevice device, int major, int minor, GraphicsContextFlags flags, IGraphicsContext sharedContext)
            : base(width, height, mode, title, options, device, major, minor, flags, sharedContext)
        {
            this.updateEvent = updateEvent;
        }


        //
        // The Window Listen Thread
        // 
        // 
        // The Application Loop
        //
        //    1. Initialize
        //    Loop
        //        1. Render
        //           - Call Render();
        //           - Call SwapBuffers();
        //        3. WaitForEvents
        //        4. Wait for updates
        //           - Call Update
        //
        //
        public abstract void Initialize();
        public abstract void Update();
        public abstract void Draw();



        Boolean windowMessageLoopRunning;
        public void MessageLoop()
        {
            try
            {
                while (true)
                {
                    ProcessEvents();
                    if (IsExiting) break;
                }
            }
            finally
            {
                windowMessageLoopRunning = false;
            }
        }



        public void Run()
        {
            // Summary:
            //     Occurs after the window has closed.
            //public event EventHandler<EventArgs> Closed;
            //
            // Summary:
            //     Occurs when the window is about to close.
            //public event EventHandler<CancelEventArgs> Closing;
            //
            // Summary:
            //     Occurs when the window is disposed.
            //public event EventHandler<EventArgs> Disposed;
            //
            // Summary:
            //     Occurs when the OpenTK.NativeWindow.Focused property of the window changes.
            //public event EventHandler<EventArgs> FocusedChanged;
            //
            // Summary:
            //     Occurs when the OpenTK.NativeWindow.Icon property of the window changes.
            //public event EventHandler<EventArgs> IconChanged;
            //
            // Summary:
            //     Occurs whenever a character is typed.
            //public event EventHandler<KeyPressEventArgs> KeyPress;
            //
            // Summary:
            //     Occurs whenever the mouse cursor enters the window OpenTK.NativeWindow.Bounds.
            //public event EventHandler<EventArgs> MouseEnter;
            //
            // Summary:
            //     Occurs whenever the mouse cursor leaves the window OpenTK.NativeWindow.Bounds.
            //public event EventHandler<EventArgs> MouseLeave;
            //
            // Summary:
            //     Occurs whenever the window is moved.
            Move += new EventHandler<EventArgs>((sender, e) => { Console.WriteLine("WindowMoved"); updateEvent.Set(); });
            //
            // Summary:
            //     Occurs whenever the window is resized.
            //public event EventHandler<EventArgs> Resize;
            //
            // Summary:
            //     Occurs when the OpenTK.NativeWindow.Title property of the window changes.
            //public event EventHandler<EventArgs> TitleChanged;
            //
            // Summary:
            //     Occurs when the OpenTK.NativeWindow.Visible property of the window changes.
            //public event EventHandler<EventArgs> VisibleChanged;
            //
            // Summary:
            //     Occurs when the OpenTK.NativeWindow.WindowBorder property of the window changes.
            //public event EventHandler<EventArgs> WindowBorderChanged;
            //
            // Summary:
            //     Occurs when the OpenTK.NativeWindow.WindowState property of the window changes.
            WindowStateChanged += new EventHandler<EventArgs>((sender, e) => { Console.WriteLine("WindowStateChanged"); updateEvent.Set(); });


            //
            // Make the game visible and call the resize method
            //
            Visible = true;
            OnResize(null);


            windowMessageLoopRunning = true;
            new Thread(MessageLoop).Start();


            Initialize();

            while (true)
            {
                //
                // Render
                //
                Draw();
                if (!windowMessageLoopRunning) break;

                //
                // Swap Buffers
                //
                SwapBuffers();
                if (!windowMessageLoopRunning) break;

                //
                //
                //
                //ProcessEvents();


                //
                // Wait for an update
                //
                updateEvent.WaitOne();
                if (!windowMessageLoopRunning) break;

                //
                // Update
                //
                Update();
                if (!windowMessageLoopRunning) break;
            }
        }
    }
}
