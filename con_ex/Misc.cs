using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ConsoleEx
{
    /// <summary>
    /// Base class of the extended console library
    /// </summary>
    public class ConsoleBase
    {
        /// <summary>
        /// CPU sleep time for handlers
        /// </summary>
        public const int TIME_WAIT = 10;

        static event HotKeyHandler hndl;
        /// <summary>
        /// Key handler that is on application level
        /// </summary>
        /// <param name="e"></param>
        public delegate void HotKeyHandler(ConsoleKeyInfo e);

        static bool enabled;
        static bool hotkey;
        static Thread thd;
        internal static ConsoleBuffer[,] cbuf;
        internal static int width, height;

        internal static List<Form> forms;
        static ConsoleKeyPress ckp;
        static bool drawing = false;
        static ConsoleKeyInfo keyinfo;
        static ConsoleColor bg, fg;

        /// <summary>
        /// Gets if the console is currently drawing
        /// </summary>
        public static bool Drawing
        {
            get { return drawing; }
        }

        /// <summary>
        /// Gets/Sets the console's background color
        /// </summary>
        public static ConsoleColor Bg
        {
            get { return bg; }
            set { bg = value; }
        }

        /// <summary>
        /// Gets/Sets the console's foreground color
        /// </summary>
        public static ConsoleColor Fg
        {
            get { return fg; }
            set { fg = value; }
        }

        private ConsoleBase() { }

        /// <summary>
        /// Initializes the extended console with the default buffer area
        /// </summary>
        /// <returns></returns>
        public static bool Init() { return Init(80, 25); }

        /// <summary>
        /// Initializes the extended console
        /// </summary>
        /// <param name="width">Buffer area width</param>
        /// <param name="height">Buffer are height</param>
        /// <returns></returns>
        public static bool Init(int width, int height)
        {
            if (enabled) return false;

            Console.Clear();
            Console.CursorVisible = false;
            bg = ConsoleColor.Black;
            fg = ConsoleColor.Gray;
            ConsoleBase.width = Console.BufferWidth = Console.WindowWidth = width;
            ConsoleBase.height = Console.BufferHeight = Console.WindowHeight = height;
            cbuf = new ConsoleBuffer[width, height];
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    cbuf[i, j] = new ConsoleBuffer();
            forms = new List<Form>();
            ConsoleKeyPress.Init();
            ckp = new ConsoleKeyPress();
            enabled = true;
            return true;
        }

        /// <summary>
        /// Deinitializes the extended console
        /// </summary>
        /// <returns></returns>
        public static bool DeInit()
        {
            if (!enabled) return false;

            ConsoleKeyPress.DeInit();
            enabled = false;
            NativeClass.NullThread(ref thd);
            return true;
        }

        /// <summary>
        /// Refreshes the console window
        /// </summary>
        public static void ReDraw()
        {
            NativeClass.RedrawWindow(NativeClass.winhndl, IntPtr.Zero, IntPtr.Zero,
                0x0400 | //RDW_FRAME
                0x0100 | //RDW_UPDATENOW
                0x0001   //RDW_INVALIDATE
                );
        }

        /// <summary>
        /// Redraws the whole console
        /// </summary>
        public static void Draw()
        {
            Draw(0, 0, width, height);
        }

        /// <summary>
        /// Redraws the console
        /// </summary>
        /// <param name="bounds">Redraw area</param>
        public static void Draw(Rect bounds)
        {
            Draw(bounds.x, bounds.y, bounds.width, bounds.height);
        }

        /// <summary>
        /// Redraws the console
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public static void Draw(int x, int y, int width, int height)
        {
            while (drawing) Thread.Sleep(1);
            drawing = true;
            for (int j = y; j < y + height && j < ConsoleBase.height; j++)
            {
                for (int i = x; i < x + width && i < ConsoleBase.width; i++)
                {
                    if (!cbuf[i, j].dr) continue;

                    Console.ForegroundColor = cbuf[i, j].fg;
                    Console.BackgroundColor = cbuf[i, j].bg;
                    Console.SetCursorPosition(i, j);
                    Console.Write(cbuf[i, j].ch);
                    cbuf[i, j].dr = false;
                }
            }
            drawing = false;
        }

        /// <summary>
        /// Clears the console, resets the buffer
        /// </summary>
        public static void ClearScreen()
        {
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    cbuf[i, j].bg = bg;
                    cbuf[i, j].fg = fg;
                    cbuf[i, j].ch = ' ';
                    cbuf[i, j].dr = true;
                }
            Draw();
        }

        /// <summary>
        /// Changes all buffers to not draw enything even if needed
        /// </summary>
        public static void ClearBuffer()
        {
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    cbuf[i, j].dr = false;
        }

        /// <summary>
        /// Sets the hotkey handler, only one can be set
        /// </summary>
        /// <param name="hndl"></param>
        public static void SetHotkeyHandler(HotKeyHandler hndl)
        {
            ConsoleBase.hndl = hndl;
            hotkey = hndl != null;
            if (hotkey)
            {
                if (thd != null) return;
                thd = new Thread(Thd);
                thd.Start();
            }
            else
                NativeClass.NullThread(ref thd);
        }

        static void Thd()
        {
            while (enabled)
            {
                Thread.Sleep(TIME_WAIT);
                if (!ckp.GetKey(out keyinfo)) continue;
                if (hotkey) hndl(keyinfo);
            }
            ConsoleMouse.enabled = false;
            Console.ResetColor();
            Console.Clear();
        }
    }

    /// <summary>
    /// Mouse handler class for the extended console, experimental
    /// </summary>
    public class ConsoleMouse
    {
        static event MouseKeyHandler hndl;
        /// <summary>
        /// Mouse key handler
        /// </summary>
        /// <param name="e"></param>
        public delegate void MouseKeyHandler(NativeClass.MouseButtons e);

        static int x, y;
        static NativeClass.WIN32_POINT pos;
        static NativeClass.WIN32_POINT delta, delta2, cur_center;
        static NativeClass.WIN32_RECT clip;
        static Thread thd, thd2;
        static ConsoleBuffer buf;
        internal static bool enabled, mousekey;

        /// <summary>
        /// Mouse cursor's X position
        /// </summary>
        public static int X
        {
            get { return x; }
            set { x = value; }
        }

        /// <summary>
        /// Mouse cursor's Y position
        /// </summary>
        public static int Y
        {
            get { return y; }
            set { y = value; }
        }

        /// <summary>
        /// Gets if the mouse is enabled
        /// </summary>
        public static bool Enabled
        {
            get { return enabled; }
        }

        /// <summary>
        /// Gets if the mouse key handler is enabled
        /// </summary>
        public static bool MouseKey
        {
            get { return mousekey; }
        }

        private ConsoleMouse() { }

        /// <summary>
        /// Enables/Disables the mouse cursor
        /// </summary>
        /// <param name="input"></param>
        public static void SetCursorState(bool input)
        {
            if (enabled == input) return;

            enabled = input;
            if (buf == null) buf = new ConsoleBuffer();
            if (enabled)
            {
                thd = new Thread(Thd);
                thd.Start();
                //Trap mouse
                NativeClass.GetWindowRect(NativeClass.winhndl, ref clip);
                clip.Left += 32;
                clip.Top += 32;
                clip.Right -= 32;
                clip.Bottom -= 32;
                NativeClass.ClipCursor(ref clip);
                cur_center.X = clip.Left + (clip.Right - clip.Left) / 2;
                cur_center.Y = clip.Top + (clip.Bottom - clip.Top) / 2;
                NativeClass.SetCursorPos(cur_center.X, cur_center.Y);
                NativeClass.GetCursorPos(ref pos);
                delta = delta2 = pos;
                DrawCur();
                ConsoleBase.ReDraw();
            }
            else
            {
                NativeClass.NullThread(ref thd);
                DrawBuff();
                ConsoleBase.ReDraw();
                clip.Left = -1;
                clip.Top = -1;
                clip.Right = -1;
                clip.Bottom = -1;
                NativeClass.ClipCursor(ref clip);
            }
        }

        /// <summary>
        /// Sets the mouse key handler, only one can be set
        /// </summary>
        /// <param name="hndl"></param>
        public static void SetMouseKeyHandler(MouseKeyHandler hndl)
        {
            ConsoleMouse.hndl = hndl;
            mousekey = hndl != null;
            if (mousekey)
            {
                if (thd2 != null) return;
                thd2 = new Thread(Thd2);
                thd2.Start();
            }
            else
            {
                NativeClass.NullThread(ref thd2);
            }
        }

        /// <summary>
        /// Gets if the mouse cursor is enabled
        /// </summary>
        /// <returns></returns>
        public static bool GetCursorState()
        {
            return enabled;
        }

        static void DrawBuff()
        {
            ConsoleBase.cbuf[x, y].SetBuffer(buf.fg, buf.bg, buf.ch);
            ConsoleBase.Draw(x, y, 1, 1);
        }

        static void DrawCur()
        {
            buf.SetBuffer(ConsoleBase.cbuf[x, y].fg, ConsoleBase.cbuf[x, y].bg, ConsoleBase.cbuf[x, y].ch);
            ConsoleBase.cbuf[x, y].bg = ConsoleColor.DarkYellow;
            ConsoleBase.cbuf[x, y].dr = true;
            ConsoleBase.Draw(x, y, 1, 1);
        }

        static void Thd()
        {
            while (enabled)
            {
                Thread.Sleep(ConsoleBase.TIME_WAIT);
                //Movement
                NativeClass.GetCursorPos(ref pos);
                delta = pos;
                NativeClass.SetCursorPos(cur_center.X, cur_center.Y); ;
                if (delta.X != delta2.X || delta.Y != delta2.Y)
                {
                    DrawBuff();
                    delta2.X = delta.X - delta2.X;
                    if (delta2.X > 0) x += 1; else if (delta2.X < 0) x -= 1;
                    delta2.Y = delta.Y - delta2.Y;
                    if (delta2.Y > 0) y += 1; else if (delta2.Y < 0) y -= 1;
                    if (x < 0) x = 0;
                    else if (x > ConsoleBase.width - 1) x = ConsoleBase.width - 1;
                    if (y < 0) y = 0;
                    else if (y > ConsoleBase.height - 2) y = ConsoleBase.height - 2;
                    DrawCur();
                    NativeClass.GetCursorPos(ref pos);
                    delta = pos;
                    ConsoleBase.ReDraw();
                }
                delta2 = delta;
            }
        }

        static void Thd2()
        {
            while (mousekey)
            {
                Thread.Sleep(ConsoleBase.TIME_WAIT);
                if (!enabled) continue;
                if (NativeClass.GetForegroundWindow() != NativeClass.winhndl) continue;

                if ((NativeClass.GetKeyState(NativeClass.MouseButtons.Left) & 0x8000) > 0)
                    hndl(NativeClass.MouseButtons.Left);
                else if ((NativeClass.GetKeyState(NativeClass.MouseButtons.Right) & 0x8000) > 0)
                    hndl(NativeClass.MouseButtons.Right);
                else if ((NativeClass.GetKeyState(NativeClass.MouseButtons.Middle) & 0x8000) > 0)
                    hndl(NativeClass.MouseButtons.Middle);
            }
        }
    }

    internal class ConsoleKeyPress
    {
        public static ConsoleKeyInfo info;
        static List<ConsoleKeyPress> inst;
        static Thread thd;
        static bool enabled;
        bool gotkey;

        public ConsoleKeyPress()
        {
            gotkey = true;
            inst.Add(this);
        }

        public bool GetKey(out ConsoleKeyInfo info)
        {
            info = default(ConsoleKeyInfo);
            if (gotkey) return false;
            info = ConsoleKeyPress.info;
            gotkey = true;
            return true;
        }

        internal static void Init()
        {
            enabled = true;
            inst = new List<ConsoleKeyPress>();
            thd = new Thread(Thd);
            thd.Start();
        }

        internal static void DeInit()
        {
            enabled = false;
            NativeClass.NullThread(ref thd);
        }

        static void Thd()
        {
            while (enabled)
            {
                info = Console.ReadKey(true);
                foreach (ConsoleKeyPress k in inst)
                    k.gotkey = false;
            }
        }
    }

    /// <summary>
    /// Rectangle
    /// </summary>
    public class Rect
    {
        /// <summary>
        /// Rectangle's X position
        /// </summary>
        public int x;
        /// <summary>
        /// Rectangle's Y position
        /// </summary>
        public int y;
        /// <summary>
        /// Rectangle's width
        /// </summary>
        public int width;
        /// <summary>
        /// Rectangle's height
        /// </summary>
        public int height;

        /// <summary>
        /// Creates a new instance of rectangle
        /// </summary>
        public Rect() { }

        /// <summary>
        /// Creates a new instance of rectangle
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public Rect(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }

    internal class ConsoleBuffer
    {
        public ConsoleColor bg, fg;
        public char ch;
        public bool dr = false;

        public void SetBuffer(ConsoleColor fg, ConsoleColor bg, char ch)
        {
            this.fg = fg;
            this.bg = bg;
            this.ch = ch;
            this.dr = true;
        }
    }

    //Win32 functions
    /// <summary>
    /// Class for DLLImports, handlers
    /// </summary>
    public class NativeClass
    {
        [DllImport("user32.dll")]
        internal static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

        [DllImport("user32.dll")]
        internal static extern bool GetWindowRect(IntPtr hwnd, ref WIN32_RECT lpRect);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        internal static extern short GetKeyState(MouseButtons nVirtKey);

        [DllImport("user32.dll")]
        internal static extern bool GetCursorPos(ref WIN32_POINT lpPoint);

        [DllImport("user32.dll")]
        internal static extern bool ClipCursor(ref WIN32_RECT lpRect);

        [DllImport("user32.dll")]
        internal static extern bool SetCursorPos(int x, int y);

        //Window handle
        internal static IntPtr winhndl = Process.GetCurrentProcess().MainWindowHandle;

        //User stuff
        internal struct WIN32_RECT { public int Left, Top, Right, Bottom; }
        internal struct WIN32_POINT { public int X, Y; }

        /// <summary>
        /// Mouse buttons for the mouse handler
        /// </summary>
        public enum MouseButtons : int
        {
            /// <summary>
            /// Left mouse button
            /// </summary>
            Left = 0x01,
            /// <summary>
            /// Right mouse button
            /// </summary>
            Right = 0x02,
            /// <summary>
            /// Middle mouse button
            /// </summary>
            Middle = 0x04
        }

        private NativeClass() { }

        /// <summary>
        /// Helper class for handlers, nulls the thread after exited
        /// </summary>
        /// <param name="thd"></param>
        public static void NullThread(ref Thread thd)
        {
            while (thd.IsAlive) Thread.Sleep(1);
            thd = null;
        }
    }
}