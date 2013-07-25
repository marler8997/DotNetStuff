using System;
using System.Runtime.InteropServices;
using System.Security;

using OpenTK;

namespace Marler.OpenTK.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr hwnd;
        public int message;
        public IntPtr wParam;
        public IntPtr lParam;
        public int time;
        public int pt_x;
        public int pt_y;
    } 

    public class WindowsHelper
    {
        readonly IntPtr windowHandle;
        public WindowsHelper(String windowTitleString)
        {
            //
            // Find the window handle
            //
            IntPtr windowTitle = Marshal.StringToHGlobalAuto(windowTitleString);
            this.windowHandle = WindowsNativeFunctions.FindWindow(IntPtr.Zero, windowTitle);

            if (windowHandle == IntPtr.Zero) throw new ArgumentException(String.Format("Could not find window with title '{0}'", windowTitleString));
        }

        public void BlockingProcessMessage()
        {            
            MSG msg = new MSG();
            int ret = WindowsNativeFunctions.GetMessage(ref msg, windowHandle, 0, 0);
            if (ret == -1)
            {
                throw new PlatformException(String.Format("An error happened while processing the message queue. Windows error: {0}", Marshal.GetLastWin32Error()));
            }
            WindowsNativeFunctions.TranslateMessage(ref msg);
            WindowsNativeFunctions.DispatchMessage(ref msg);
        }
    }   


    public static class WindowsNativeFunctions
    {
        /*
        // Methods
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern bool AdjustWindowRect([In, Out] ref Win32Rectangle lpRect, WindowStyle dwStyle, bool bMenu);
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern bool AdjustWindowRectEx(ref Win32Rectangle lpRect, WindowStyle dwStyle, bool bMenu, ExtendedWindowStyle dwExStyle);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BringWindowToTop(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, WindowMessage Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int ChangeDisplaySettings(DeviceMode device_mode, ChangeDisplaySettingsEnum flags);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int ChangeDisplaySettingsEx([MarshalAs(UnmanagedType.LPTStr)] string lpszDeviceName, DeviceMode lpDevMode, IntPtr hwnd, ChangeDisplaySettingsEnum dwflags, IntPtr lParam);
        [DllImport("gdi32.dll")]
        internal static extern int ChoosePixelFormat(IntPtr dc, ref PixelFormatDescriptor pfd);
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern bool ClientToScreen(IntPtr hWnd, ref Point point);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CreateWindowEx(ExtendedWindowStyle ExStyle, IntPtr ClassAtom, IntPtr WindowName, WindowStyle Style, int X, int Y, int Width, int Height, IntPtr HandleToParentWindow, IntPtr Menu, IntPtr Instance, IntPtr Param);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CreateWindowEx(ExtendedWindowStyle ExStyle, [MarshalAs(UnmanagedType.LPTStr)] string className, [MarshalAs(UnmanagedType.LPTStr)] string windowName, WindowStyle Style, int X, int Y, int Width, int Height, IntPtr HandleToParentWindow, IntPtr Menu, IntPtr Instance, IntPtr Param);
        [CLSCompliant(false), SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr DefRawInputProc(RawInput[] RawInput, int Input, uint SizeHeader);
        [CLSCompliant(false), SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr DefRawInputProc(ref RawInput RawInput, int Input, uint SizeHeader);
        [CLSCompliant(false), SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr DefRawInputProc(IntPtr RawInput, int Input, uint SizeHeader);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, WindowMessage msg, IntPtr wParam, IntPtr lParam);
        [DllImport("gdi32.dll")]
        internal static extern int DescribePixelFormat(IntPtr deviceContext, int pixel, int pfdSize, ref PixelFormatDescriptor pixelFormat);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool DestroyWindow(IntPtr windowHandle);
        */
        [CLSCompliant(false), DllImport("User32.dll")]
        internal static extern IntPtr DispatchMessage(ref MSG msg);
        /*
        [DllImport("dwmapi.dll")]
        public static extern unsafe IntPtr DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, void* pvAttribute, int cbAttribute);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumDisplayDevices([MarshalAs(UnmanagedType.LPTStr)] string lpDevice, int iDevNum, [In, Out] WindowsDisplayDevice lpDisplayDevice, int dwFlags);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool EnumDisplaySettings([MarshalAs(UnmanagedType.LPTStr)] string device_name, DisplayModeSettingsEnum graphics_mode, [In, Out] DeviceMode device_mode);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool EnumDisplaySettings([MarshalAs(UnmanagedType.LPTStr)] string device_name, int graphics_mode, [In, Out] DeviceMode device_mode);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumDisplaySettingsEx([MarshalAs(UnmanagedType.LPTStr)] string lpszDeviceName, DisplayModeSettingsEnum iModeNum, [In, Out] DeviceMode lpDevMode, int dwFlags);
        [return: MarshalAs(UnmanagedType.Bool)]
        */
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr FindWindow(IntPtr ClassName, IntPtr WindowName);
                /*
        [DllImport("kernel32.dll")]
        internal static extern bool FreeLibrary(IntPtr handle);
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern short GetAsyncKeyState(VirtualKeys vKey);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetCapture();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool GetClassInfoEx(IntPtr hinst, [MarshalAs(UnmanagedType.LPTStr)] string lpszClass, ref ExtendedWindowClass lpwcx);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool GetClassInfoEx(IntPtr hinst, UIntPtr lpszClass, ref ExtendedWindowClass lpwcx);
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern bool GetClientRect(IntPtr windowHandle, out Win32Rectangle clientRectangle);
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern bool GetCursorPos(ref Point point);
        [DllImport("user32.dll")]
        internal static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern IntPtr GetFocus();
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern short GetKeyState(VirtualKeys vKey);
        */
        [CLSCompliant(false), SuppressUnmanagedCodeSecurity, DllImport("User32.dll")]
        internal static extern int GetMessage(ref MSG msg, IntPtr windowHandle, int messageFilterMin, int messageFilterMax);
        /*
        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPTStr)] string module_name);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);
        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetProcAddress(IntPtr handle, string funcname);
        [SuppressUnmanagedCodeSecurity, DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern int GetQueueStatus([MarshalAs(UnmanagedType.U4)] QueueStatusFlags flags);
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetRawInputBuffer([Out] RawInput[] Data, [In, Out] ref int Size, [In] int SizeHeader);
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetRawInputBuffer([Out] IntPtr Data, [In, Out] ref int Size, [In] int SizeHeader);
        [SuppressUnmanagedCodeSecurity, CLSCompliant(false), DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetRawInputBuffer([Out] RawInput[] Data, [In, Out] ref uint Size, [In] uint SizeHeader);
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetRawInputData(IntPtr RawInput, GetRawInputDataEnum Command, out RawInput Data, [In, Out] ref int Size, int SizeHeader);
        [SuppressUnmanagedCodeSecurity, CLSCompliant(false), DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetRawInputData(IntPtr RawInput, GetRawInputDataEnum Command, out RawInput Data, [In, Out] ref uint Size, uint SizeHeader);
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetRawInputData(IntPtr RawInput, GetRawInputDataEnum Command, [Out] IntPtr Data, [In, Out] ref int Size, int SizeHeader);
        [CLSCompliant(false), SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetRawInputData(IntPtr RawInput, GetRawInputDataEnum Command, [Out] IntPtr Data, [In, Out] ref uint Size, uint SizeHeader);
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetRawInputDeviceInfo(IntPtr Device, [MarshalAs(UnmanagedType.U4)] RawInputDeviceInfoEnum Command, [In, Out] RawInputDeviceInfo Data, [In, Out] ref int Size);
        [CLSCompliant(false), SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetRawInputDeviceInfo(IntPtr Device, [MarshalAs(UnmanagedType.U4)] RawInputDeviceInfoEnum Command, [In, Out] RawInputDeviceInfo Data, [In, Out] ref uint Size);
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetRawInputDeviceInfo(IntPtr Device, [MarshalAs(UnmanagedType.U4)] RawInputDeviceInfoEnum Command, [In, Out] IntPtr Data, [In, Out] ref int Size);
        [CLSCompliant(false), SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetRawInputDeviceInfo(IntPtr Device, [MarshalAs(UnmanagedType.U4)] RawInputDeviceInfoEnum Command, [In, Out] IntPtr Data, [In, Out] ref uint Size);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetRawInputDeviceList([In, Out] IntPtr RawInputDeviceList, [In, Out] ref int NumDevices, int Size);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetRawInputDeviceList([In, Out] RawInputDeviceList[] RawInputDeviceList, [In, Out] ref int NumDevices, int Size);
        [CLSCompliant(false), DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetRawInputDeviceList([In, Out] IntPtr RawInputDeviceList, [In, Out] ref uint NumDevices, uint Size);
        [CLSCompliant(false), DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetRawInputDeviceList([In, Out] RawInputDeviceList[] RawInputDeviceList, [In, Out] ref uint NumDevices, uint Size);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetRegisteredRawInputDevices([Out] RawInput[] RawInputDevices, [In, Out] ref int NumDevices, int cbSize);
        [CLSCompliant(false), DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetRegisteredRawInputDevices([Out] RawInput[] RawInputDevices, [In, Out] ref uint NumDevices, uint cbSize);
        [DllImport("gdi32.dll", SetLastError = true)]
        internal static extern IntPtr GetStockObject(int index);
        [DllImport("user32.dll")]
        internal static extern IntPtr GetWindowDC(IntPtr hwnd);
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll")]
        internal static extern bool GetWindowInfo(IntPtr hwnd, ref WindowInfo wi);
        internal static UIntPtr GetWindowLong(IntPtr handle, GetWindowLongOffsets index)
        {
            if (IntPtr.Size == 4)
            {
                return (UIntPtr)GetWindowLongInternal(handle, index);
            }
            return GetWindowLongPtrInternal(handle, index);
        }

        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
        private static extern uint GetWindowLongInternal(IntPtr hWnd, GetWindowLongOffsets nIndex);
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern UIntPtr GetWindowLongPtrInternal(IntPtr hWnd, GetWindowLongOffsets nIndex);
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern bool GetWindowRect(IntPtr windowHandle, out Win32Rectangle windowRectangle);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetWindowText(IntPtr hWnd, [In, Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr intPtr);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool KillTimer(IntPtr hWnd, UIntPtr uIDEvent);
        public static IntPtr LoadCursor(CursorName lpCursorName)
        {
            return LoadCursor(IntPtr.Zero, new IntPtr((int)lpCursorName));
        }

        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursor(IntPtr hInstance, IntPtr lpCursorName);
        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursor(IntPtr hInstance, string lpCursorName);
        [DllImport("user32.dll")]
        public static extern IntPtr LoadIcon(IntPtr hInstance, string lpIconName);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string dllName);
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern uint MapVirtualKey(VirtualKeys vkey, MapVirtualKeyType uMapType);
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern uint MapVirtualKey(uint uCode, MapVirtualKeyType uMapType);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr MonitorFromPoint(POINT pt, MonitorFrom dwFlags);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorFrom dwFlags);
        internal static unsafe IntPtr NextRawInputStructure(IntPtr data)
        {
            return RawInputAlign((IntPtr)(((void*)data) + API.RawInputHeaderSize));
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [CLSCompliant(false), SuppressUnmanagedCodeSecurity, DllImport("User32.dll")]
        internal static extern bool PeekMessage(ref MSG msg, IntPtr hWnd, int messageFilterMin, int messageFilterMax, int flags);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity, CLSCompliant(false), DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool PostMessage(IntPtr hWnd, WindowMessage Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern void PostQuitMessage(int exitCode);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity, DllImport("kernel32.dll")]
        internal static extern bool QueryPerformanceCounter(ref long PerformanceCount);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity, DllImport("kernel32.dll")]
        internal static extern bool QueryPerformanceFrequency(ref long PerformanceFrequency);
        private static unsafe IntPtr RawInputAlign(IntPtr data)
        {
            return (IntPtr)(((void*)data) + ((IntPtr.Size - 1) & ~(IntPtr.Size - 1)));
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern ushort RegisterClass(ref WindowClass window_class);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern ushort RegisterClassEx(ref ExtendedWindowClass window_class);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool RegisterRawInputDevices(RawInputDevice[] RawInputDevices, int NumDevices, int Size);
        [return: MarshalAs(UnmanagedType.Bool)]
        [CLSCompliant(false), DllImport("user32.dll", SetLastError = true)]
        internal static extern bool RegisterRawInputDevices(RawInputDevice[] RawInputDevices, uint NumDevices, uint Size);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool ReleaseCapture();
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        internal static extern bool ReleaseDC(IntPtr hwnd, IntPtr DC);
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern bool ScreenToClient(IntPtr hWnd, ref Point point);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, WindowMessage Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr SetCapture(IntPtr hwnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr SetFocus(IntPtr hwnd);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("kernel32.dll")]
        internal static extern void SetLastError(int dwErrCode);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("gdi32.dll", SetLastError = true)]
        internal static extern bool SetPixelFormat(IntPtr dc, int format, ref PixelFormatDescriptor pfd);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern UIntPtr SetTimer(IntPtr hWnd, UIntPtr nIDEvent, uint uElapse, TimerProc lpTimerFunc);
        internal static IntPtr SetWindowLong(IntPtr handle, WindowProcedure newValue)
        {
            return SetWindowLong(handle, GetWindowLongOffsets.WNDPROC, Marshal.GetFunctionPointerForDelegate(newValue));
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, GetWindowLongOffsets nIndex, [MarshalAs(UnmanagedType.FunctionPtr)] WindowProcedure dwNewLong);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, GetWindowLongOffsets nIndex, int dwNewLong);
        internal static IntPtr SetWindowLong(IntPtr handle, GetWindowLongOffsets item, IntPtr newValue)
        {
            IntPtr zero = IntPtr.Zero;
            SetLastError(0);
            if (IntPtr.Size == 4)
            {
                zero = new IntPtr(SetWindowLong(handle, item, newValue.ToInt32()));
            }
            else
            {
                zero = SetWindowLongPtr(handle, item, newValue);
            }
            if (zero == IntPtr.Zero)
            {
                int num = Marshal.GetLastWin32Error();
                if (num != 0)
                {
                    throw new PlatformException(string.Format("Failed to modify window border. Error: {0}", num));
                }
            }
            return zero;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, GetWindowLongOffsets nIndex, [MarshalAs(UnmanagedType.FunctionPtr)] WindowProcedure dwNewLong);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, GetWindowLongOffsets nIndex, IntPtr dwNewLong);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        internal static extern bool SetWindowPos(IntPtr handle, IntPtr insertAfter, int x, int y, int cx, int cy, SetWindowPosFlags flags);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool SetWindowText(IntPtr hWnd, [MarshalAs(UnmanagedType.LPTStr)] string lpString);
        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, int dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, ShGetFileIconFlags uFlags);
        [SuppressUnmanagedCodeSecurity, DllImport("user32.dll", SetLastError = true)]
        internal static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommand nCmdShow);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity, DllImport("gdi32.dll", SetLastError = true)]
        internal static extern bool SwapBuffers(IntPtr dc);
        [SuppressUnmanagedCodeSecurity, DllImport("winmm.dll")]
        internal static extern IntPtr TimeBeginPeriod(int period);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool TrackMouseEvent(ref TrackMouseEventStructure lpEventTrack);
        */
        [CLSCompliant(false), DllImport("User32.dll")]
        internal static extern bool TranslateMessage(ref MSG lpMsg);
        /*
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern short UnregisterClass(IntPtr className, IntPtr instance);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern short UnregisterClass([MarshalAs(UnmanagedType.LPTStr)] string className, IntPtr instance);

        // Nested Types
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void TimerProc(IntPtr hwnd, WindowMessage uMsg, UIntPtr idEvent, int dwTime);
        */
    }
}
