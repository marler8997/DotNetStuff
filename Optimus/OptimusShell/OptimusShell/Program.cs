using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimusShell
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleKeyInfo keypress;
            do
            {
                keypress = Console.ReadKey(true); // read keystrokes 
                


                /*
                Console.WriteLine(" Your key is: " + keypress.KeyChar);

                // Check for modifier keys. 
                if ((ConsoleModifiers.Alt & keypress.Modifiers) != 0)
                    Console.WriteLine("Alt key pressed.");
                if ((ConsoleModifiers.Control & keypress.Modifiers) != 0)
                    Console.WriteLine("Control key pressed.");
                if ((ConsoleModifiers.Shift & keypress.Modifiers) != 0)
                    Console.WriteLine("Shift key pressed.");
                */

                switch(keypress.Key)
                {

                    // Summary:
                    //     The BACKSPACE key.
                    case ConsoleKey.Backspace:// = 8,

                        break;

                    //
                    // Summary:
                    //     The TAB key.
                    case ConsoleKey.Tab:// = 9,

                        break;

                    //
                    // Summary:
                    //     The CLEAR key.
                    case ConsoleKey.Clear:// = 12,

                        break;

                    //
                    // Summary:
                    //     The ENTER key.
                    case ConsoleKey.Enter:// = 13,

                        break;

                    //
                    // Summary:
                    //     The PAUSE key.
                    case ConsoleKey.Pause:// = 19,

                        break;

                    //
                    // Summary:
                    //     The ESC (ESCAPE) key.
                    case ConsoleKey.Escape:// = 27,

                        break;

                    //
                    // Summary:
                    //     The SPACEBAR key.
                    case ConsoleKey.Spacebar:// = 32,

                        break;

                    //
                    // Summary:
                    //     The PAGE UP key.
                    case ConsoleKey.PageUp:// = 33,

                        break;

                    //
                    // Summary:
                    //     The PAGE DOWN key.
                    case ConsoleKey.PageDown:// = 34,

                        break;

                    //
                    // Summary:
                    //     The END key.
                    case ConsoleKey.End:// = 35,

                        break;

                    //
                    // Summary:
                    //     The HOME key.
                    case ConsoleKey.Home:// = 36,

                        break;

                    //
                    // Summary:
                    //     The LEFT ARROW key.
                    case ConsoleKey.LeftArrow:// = 37,

                        break;

                    //
                    // Summary:
                    //     The UP ARROW key.
                    case ConsoleKey.UpArrow:// = 38,

                        break;

                    //
                    // Summary:
                    //     The RIGHT ARROW key.
                    case ConsoleKey.RightArrow:// = 39,

                        break;

                    //
                    // Summary:
                    //     The DOWN ARROW key.
                    case ConsoleKey.DownArrow:// = 40,

                        break;

                    //
                    // Summary:
                    //     The SELECT key.
                    case ConsoleKey.Select:// = 41,

                        break;

                    //
                    // Summary:
                    //     The PRINT key.
                    case ConsoleKey.Print:// = 42,

                        break;

                    //
                    // Summary:
                    //     The EXECUTE key.
                    case ConsoleKey.Execute:// = 43,

                        break;

                    //
                    // Summary:
                    //     The PRINT SCREEN key.
                    case ConsoleKey.PrintScreen:// = 44,

                        break;

                    //
                    // Summary:
                    //     The INS (INSERT) key.
                    case ConsoleKey.Insert:// = 45,

                        break;

                    //
                    // Summary:
                    //     The DEL (DELETE) key.
                    case ConsoleKey.Delete:// = 46,

                        break;

                    //
                    // Summary:
                    //     The HELP key.
                    case ConsoleKey.Help:// = 47,

                        break;

                    //
                    // Summary:
                    //     The 0 key.
                    case ConsoleKey.D0:// = 48,

                        break;

                    //
                    // Summary:
                    //     The 1 key.
                    case ConsoleKey.D1:// = 49,

                        break;

                    //
                    // Summary:
                    //     The 2 key.
                    case ConsoleKey.D2:// = 50,

                        break;

                    //
                    // Summary:
                    //     The 3 key.
                    case ConsoleKey.D3:// = 51,

                        break;

                    //
                    // Summary:
                    //     The 4 key.
                    case ConsoleKey.D4:// = 52,

                        break;

                    //
                    // Summary:
                    //     The 5 key.
                    case ConsoleKey.D5:// = 53,

                        break;

                    //
                    // Summary:
                    //     The 6 key.
                    case ConsoleKey.D6:// = 54,

                        break;

                    //
                    // Summary:
                    //     The 7 key.
                    case ConsoleKey.D7:// = 55,

                        break;

                    //
                    // Summary:
                    //     The 8 key.
                    case ConsoleKey.D8:// = 56,

                        break;

                    //
                    // Summary:
                    //     The 9 key.
                    case ConsoleKey.D9:// = 57,

                        break;

                    //
                    // Summary:
                    //     The A key.
                    case ConsoleKey.A:// = 65,
                        break;

                    //
                    // Summary:
                    //     The B key.
                    case ConsoleKey.B:// = 66,

                        break;

                    //
                    // Summary:
                    //     The C key.
                    case ConsoleKey.C:// = 67,

                        break;

                    //
                    // Summary:
                    //     The D key.
                    case ConsoleKey.D:// = 68,

                        break;

                    //
                    // Summary:
                    //     The E key.
                    case ConsoleKey.E:// = 69,

                        break;

                    //
                    // Summary:
                    //     The F key.
                    case ConsoleKey.F:// = 70,

                        break;

                    //
                    // Summary:
                    //     The G key.
                    case ConsoleKey.G:// = 71,

                        break;

                    //
                    // Summary:
                    //     The H key.
                    case ConsoleKey.H:// = 72,

                        break;

                    //
                    // Summary:
                    //     The I key.
                    case ConsoleKey.I:// = 73,

                        break;

                    //
                    // Summary:
                    //     The J key.
                    case ConsoleKey.J:// = 74,

                        break;

                    //
                    // Summary:
                    //     The K key.
                    case ConsoleKey.K:// = 75,

                        break;

                    //
                    // Summary:
                    //     The L key.
                    case ConsoleKey.L:// = 76,

                        break;

                    //
                    // Summary:
                    //     The M key.
                    case ConsoleKey.M:// = 77,

                        break;

                    //
                    // Summary:
                    //     The N key.
                    case ConsoleKey.N:// = 78,

                        break;

                    //
                    // Summary:
                    //     The O key.
                    case ConsoleKey.O:// = 79,

                        break;

                    //
                    // Summary:
                    //     The P key.
                    case ConsoleKey.P:// = 80,

                        break;

                    //
                    // Summary:
                    //     The Q key.
                    case ConsoleKey.Q:// = 81,

                        break;

                    //
                    // Summary:
                    //     The R key.
                    case ConsoleKey.R:// = 82,

                        break;

                    //
                    // Summary:
                    //     The S key.
                    case ConsoleKey.S:// = 83,

                        break;

                    //
                    // Summary:
                    //     The T key.
                    case ConsoleKey.T:// = 84,

                        break;

                    //
                    // Summary:
                    //     The U key.
                    case ConsoleKey.U:// = 85,

                        break;

                    //
                    // Summary:
                    //     The V key.
                    case ConsoleKey.V:// = 86,

                        break;

                    //
                    // Summary:
                    //     The W key.
                    case ConsoleKey.W:// = 87,

                        break;

                    //
                    // Summary:
                    //     The X key.
                    case ConsoleKey.X:// = 88,

                        break;

                    //
                    // Summary:
                    //     The Y key.
                    case ConsoleKey.Y:// = 89,

                        break;

                    //
                    // Summary:
                    //     The Z key.
                    case ConsoleKey.Z:// = 90,

                        break;

                    //
                    // Summary:
                    //     The left Windows logo key (Microsoft Natural Keyboard).
                    case ConsoleKey.LeftWindows:// = 91,

                        break;

                    //
                    // Summary:
                    //     The right Windows logo key (Microsoft Natural Keyboard).
                    case ConsoleKey.RightWindows:// = 92,

                        break;

                    //
                    // Summary:
                    //     The Application key (Microsoft Natural Keyboard).
                    case ConsoleKey.Applications:// = 93,

                        break;

                    //
                    // Summary:
                    //     The Computer Sleep key.
                    case ConsoleKey.Sleep:// = 95,

                        break;

                    //
                    // Summary:
                    //     The 0 key on the numeric keypad.
                    case ConsoleKey.NumPad0:// = 96,

                        break;

                    //
                    // Summary:
                    //     The 1 key on the numeric keypad.
                    case ConsoleKey.NumPad1:// = 97,

                        break;

                    //
                    // Summary:
                    //     The 2 key on the numeric keypad.
                    case ConsoleKey.NumPad2:// = 98,

                        break;

                    //
                    // Summary:
                    //     The 3 key on the numeric keypad.
                    case ConsoleKey.NumPad3:// = 99,

                        break;

                    //
                    // Summary:
                    //     The 4 key on the numeric keypad.
                    case ConsoleKey.NumPad4:// = 100,

                        break;

                    //
                    // Summary:
                    //     The 5 key on the numeric keypad.
                    case ConsoleKey.NumPad5:// = 101,

                        break;

                    //
                    // Summary:
                    //     The 6 key on the numeric keypad.
                    case ConsoleKey.NumPad6:// = 102,

                        break;

                    //
                    // Summary:
                    //     The 7 key on the numeric keypad.
                    case ConsoleKey.NumPad7:// = 103,

                        break;

                    //
                    // Summary:
                    //     The 8 key on the numeric keypad.
                    case ConsoleKey.NumPad8:// = 104,

                        break;

                    //
                    // Summary:
                    //     The 9 key on the numeric keypad.
                    case ConsoleKey.NumPad9:// = 105,

                        break;

                    //
                    // Summary:
                    //     The Multiply key.
                    case ConsoleKey.Multiply:// = 106,

                        break;

                    //
                    // Summary:
                    //     The Add key.
                    case ConsoleKey.Add:// = 107,

                        break;

                    //
                    // Summary:
                    //     The Separator key.
                    case ConsoleKey.Separator:// = 108,

                        break;

                    //
                    // Summary:
                    //     The Subtract key.
                    case ConsoleKey.Subtract:// = 109,

                        break;

                    //
                    // Summary:
                    //     The Decimal key.
                    case ConsoleKey.Decimal:// = 110,

                        break;

                    //
                    // Summary:
                    //     The Divide key.
                    case ConsoleKey.Divide:// = 111,

                        break;

                    //
                    // Summary:
                    //     The F1 key.
                    case ConsoleKey.F1:// = 112,

                        break;

                    //
                    // Summary:
                    //     The F2 key.
                    case ConsoleKey.F2:// = 113,

                        break;

                    //
                    // Summary:
                    //     The F3 key.
                    case ConsoleKey.F3:// = 114,

                        break;

                    //
                    // Summary:
                    //     The F4 key.
                    case ConsoleKey.F4:// = 115,

                        break;

                    //
                    // Summary:
                    //     The F5 key.
                    case ConsoleKey.F5:// = 116,

                        break;

                    //
                    // Summary:
                    //     The F6 key.
                    case ConsoleKey.F6:// = 117,

                        break;

                    //
                    // Summary:
                    //     The F7 key.
                    case ConsoleKey.F7:// = 118,

                        break;

                    //
                    // Summary:
                    //     The F8 key.
                    case ConsoleKey.F8:// = 119,

                        break;

                    //
                    // Summary:
                    //     The F9 key.
                    case ConsoleKey.F9:// = 120,

                        break;

                    //
                    // Summary:
                    //     The F10 key.
                    case ConsoleKey.F10:// = 121,

                        break;

                    //
                    // Summary:
                    //     The F11 key.
                    case ConsoleKey.F11:// = 122,

                        break;

                    //
                    // Summary:
                    //     The F12 key.
                    case ConsoleKey.F12:// = 123,

                        break;

                    //
                    // Summary:
                    //     The F13 key.
                    case ConsoleKey.F13:// = 124,

                        break;

                    //
                    // Summary:
                    //     The F14 key.
                    case ConsoleKey.F14:// = 125,

                        break;

                    //
                    // Summary:
                    //     The F15 key.
                    case ConsoleKey.F15:// = 126,

                        break;

                    //
                    // Summary:
                    //     The F16 key.
                    case ConsoleKey.F16:// = 127,

                        break;

                    //
                    // Summary:
                    //     The F17 key.
                    case ConsoleKey.F17:// = 128,

                        break;

                    //
                    // Summary:
                    //     The F18 key.
                    case ConsoleKey.F18:// = 129,

                        break;

                    //
                    // Summary:
                    //     The F19 key.
                    case ConsoleKey.F19:// = 130,

                        break;

                    //
                    // Summary:
                    //     The F20 key.
                    case ConsoleKey.F20:// = 131,

                        break;

                    //
                    // Summary:
                    //     The F21 key.
                    case ConsoleKey.F21:// = 132,

                        break;

                    //
                    // Summary:
                    //     The F22 key.
                    case ConsoleKey.F22:// = 133,

                        break;

                    //
                    // Summary:
                    //     The F23 key.
                    case ConsoleKey.F23:// = 134,

                        break;

                    //
                    // Summary:
                    //     The F24 key.
                    case ConsoleKey.F24:// = 135,

                        break;

                    //
                    // Summary:
                    //     The Browser Back key (Windows 2000 or later).
                    case ConsoleKey.BrowserBack:// = 166,

                        break;

                    //
                    // Summary:
                    //     The Browser Forward key (Windows 2000 or later).
                    case ConsoleKey.BrowserForward:// = 167,

                        break;

                    //
                    // Summary:
                    //     The Browser Refresh key (Windows 2000 or later).
                    case ConsoleKey.BrowserRefresh:// = 168,

                        break;

                    //
                    // Summary:
                    //     The Browser Stop key (Windows 2000 or later).
                    case ConsoleKey.BrowserStop:// = 169,

                        break;

                    //
                    // Summary:
                    //     The Browser Search key (Windows 2000 or later).
                    case ConsoleKey.BrowserSearch:// = 170,

                        break;

                    //
                    // Summary:
                    //     The Browser Favorites key (Windows 2000 or later).
                    case ConsoleKey.BrowserFavorites:// = 171,

                        break;

                    //
                    // Summary:
                    //     The Browser Home key (Windows 2000 or later).
                    case ConsoleKey.BrowserHome:// = 172,

                        break;

                    //
                    // Summary:
                    //     The Volume Mute key (Microsoft Natural Keyboard, Windows 2000 or later).
                    case ConsoleKey.VolumeMute:// = 173,

                        break;

                    //
                    // Summary:
                    //     The Volume Down key (Microsoft Natural Keyboard, Windows 2000 or later).
                    case ConsoleKey.VolumeDown:// = 174,

                        break;

                    //
                    // Summary:
                    //     The Volume Up key (Microsoft Natural Keyboard, Windows 2000 or later).
                    case ConsoleKey.VolumeUp:// = 175,

                        break;

                    //
                    // Summary:
                    //     The Media Next Track key (Windows 2000 or later).
                    case ConsoleKey.MediaNext:// = 176,

                        break;

                    //
                    // Summary:
                    //     The Media Previous Track key (Windows 2000 or later).
                    case ConsoleKey.MediaPrevious:// = 177,

                        break;

                    //
                    // Summary:
                    //     The Media Stop key (Windows 2000 or later).
                    case ConsoleKey.MediaStop:// = 178,

                        break;

                    //
                    // Summary:
                    //     The Media Play/Pause key (Windows 2000 or later).
                    case ConsoleKey.MediaPlay:// = 179,

                        break;

                    //
                    // Summary:
                    //     The Start Mail key (Microsoft Natural Keyboard, Windows 2000 or later).
                    case ConsoleKey.LaunchMail:// = 180,

                        break;

                    //
                    // Summary:
                    //     The Select Media key (Microsoft Natural Keyboard, Windows 2000 or later).
                    case ConsoleKey.LaunchMediaSelect:// = 181,

                        break;

                    //
                    // Summary:
                    //     The Start Application 1 key (Microsoft Natural Keyboard, Windows 2000 or
                    //     later).
                    case ConsoleKey.LaunchApp1:// = 182,

                        break;

                    //
                    // Summary:
                    //     The Start Application 2 key (Microsoft Natural Keyboard, Windows 2000 or
                    //     later).
                    case ConsoleKey.LaunchApp2:// = 183,

                        break;

                    //
                    // Summary:
                    //     The OEM 1 key (OEM specific).
                    case ConsoleKey.Oem1:// = 186,

                        break;

                    //
                    // Summary:
                    //     The OEM Plus key on any country/region keyboard (Windows 2000 or later).
                    case ConsoleKey.OemPlus:// = 187,

                        break;

                    //
                    // Summary:
                    //     The OEM Comma key on any country/region keyboard (Windows 2000 or later).
                    case ConsoleKey.OemComma:// = 188,

                        break;

                    //
                    // Summary:
                    //     The OEM Minus key on any country/region keyboard (Windows 2000 or later).
                    case ConsoleKey.OemMinus:// = 189,

                        break;

                    //
                    // Summary:
                    //     The OEM Period key on any country/region keyboard (Windows 2000 or later).
                    case ConsoleKey.OemPeriod:// = 190,

                        break;

                    //
                    // Summary:
                    //     The OEM 2 key (OEM specific).
                    case ConsoleKey.Oem2:// = 191,

                        break;

                    //
                    // Summary:
                    //     The OEM 3 key (OEM specific).
                    case ConsoleKey.Oem3:// = 192,

                        break;

                    //
                    // Summary:
                    //     The OEM 4 key (OEM specific).
                    case ConsoleKey.Oem4:// = 219,

                        break;

                    //
                    // Summary:
                    //     The OEM 5 (OEM specific).
                    case ConsoleKey.Oem5:// = 220,

                        break;

                    //
                    // Summary:
                    //     The OEM 6 key (OEM specific).
                    case ConsoleKey.Oem6:// = 221,

                        break;

                    //
                    // Summary:
                    //     The OEM 7 key (OEM specific).
                    case ConsoleKey.Oem7:// = 222,

                        break;

                    //
                    // Summary:
                    //     The OEM 8 key (OEM specific).
                    case ConsoleKey.Oem8:// = 223,

                        break;

                    //
                    // Summary:
                    //     The OEM 102 key (OEM specific).
                    case ConsoleKey.Oem102:// = 226,

                        break;

                    //
                    // Summary:
                    //     The IME PROCESS key.
                    case ConsoleKey.Process:// = 229,

                        break;

                    //
                    // Summary:
                    //     The PACKET key (used to pass Unicode characters with keystrokes).
                    case ConsoleKey.Packet:// = 231,

                        break;

                    //
                    // Summary:
                    //     The ATTN key.
                    case ConsoleKey.Attention:// = 246,

                        break;

                    //
                    // Summary:
                    //     The CRSEL (CURSOR SELECT) key.
                    case ConsoleKey.CrSel:// = 247,

                        break;

                    //
                    // Summary:
                    //     The EXSEL (EXTEND SELECTION) key.
                    case ConsoleKey.ExSel:// = 248,

                        break;

                    //
                    // Summary:
                    //     The ERASE EOF key.
                    case ConsoleKey.EraseEndOfFile:// = 249,

                        break;

                    //
                    // Summary:
                    //     The PLAY key.
                    case ConsoleKey.Play:// = 250,

                        break;

                    //
                    // Summary:
                    //     The ZOOM key.
                    case ConsoleKey.Zoom:// = 251,

                        break;

                    //
                    // Summary:
                    //     A constant reserved for future use.
                    case ConsoleKey.NoName:// = 252,

                        break;

                    //
                    // Summary:
                    //     The PA1 key.
                    case ConsoleKey.Pa1:// = 253,

                        break;


                    //
                    // Summary:
                    //     The CLEAR key (OEM specific).
                    case ConsoleKey.OemClear:// = 254,

                        break;



                    default:

                        break;


            }
            } while (keypress.KeyChar != 'Q');
        }
    }
}
