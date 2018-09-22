using System;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleEx
{
    /// <summary>
    /// Base class of the displayable controls, must be inherited
    /// </summary>
    public abstract class ControlBase
    {
        internal int x, y;

        /// <summary>
        /// Control's background color
        /// </summary>
        protected ConsoleColor bg;
        /// <summary>
        /// Control's foreground color
        /// </summary>
        protected ConsoleColor fg;

        internal Rect bounds;
        internal Container parent;
        internal bool enabled;
        internal bool visible;
        internal bool focused;

        /// <summary>
        /// Gets the control's relative x position
        /// </summary>
        public int X
        {
            get { return x; }
        }

        /// <summary>
        /// Gets the control's relative y position
        /// </summary>
        public int Y
        {
            get { return y; }
        }

        /// <summary>
        /// Gets the control's width
        /// </summary>
        public int Width
        {
            get { return bounds.width; }
        }

        /// <summary>
        /// Gets the control's height
        /// </summary>
        public int Height
        {
            get { return bounds.height; }
        }

        /// <summary>
        /// Gets/Sets the control's background color
        /// </summary>
        public ConsoleColor Bg
        {
            get { return bg; }
            set { bg = value; }
        }

        /// <summary>
        /// Gets/Sets the control's foreground color
        /// </summary>
        public ConsoleColor Fg
        {
            get { return fg; }
            set { fg = value; }
        }

        /// <summary>
        /// Gets the control's bounds, x and y are absolute positions
        /// </summary>
        public Rect Bounds
        {
            get { return bounds; }
        }

        /// <summary>
        /// Gets the control's parent
        /// </summary>
        public Container Parent
        {
            get { return parent; }
        }

        /// <summary>
        /// Gets/Sets the control's enabled status
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// Gets/Sets the control's visibility
        /// </summary>
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        /// <summary>
        /// Gets the control's focus
        /// </summary>
        public bool Focused
        {
            get { return focused; }
        }

        /// <summary>
        /// Control's drawing code, must be overridden
        /// </summary>
        public abstract void Draw();

        /// <summary>
        /// Sets the control's position and size
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public void SetBounds(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            bounds.width = width;
            bounds.height = height;
            SetRelBounds();
        }

        //Background repositioning
        internal void SetRelBounds()
        {
            if (parent != null)
            {
                bounds.x = this.x + parent.bounds.x + 1;
                bounds.y = this.y + parent.bounds.y + 1;
            }
            else
            {
                bounds.x = this.x;
                bounds.y = this.y;
            }
        }
    }

    /// <summary>
    /// Base class of the containers, which can contain controls, inherited from ControlBase
    /// </summary>
    public class Container : ControlBase
    {
        internal List<Control> controls;
        internal List<Container> containers;

        internal uint tablen;

        internal static uint ids;
        uint id;

        /// <summary>
        /// Gets container's current Id
        /// </summary>
        public uint Id
        {
            get { return id; }
        }

        /// <summary>
        /// Creates a new instance of container
        /// </summary>
        public Container()
        {
            fg = ConsoleColor.White;
            bg = ConsoleColor.Blue;
            bounds = new Rect();
            controls = new List<Control>();
            containers = new List<Container>();
            enabled = true;
            visible = true;
            tablen = 0;
            id = ids++;
        }

        /// <summary>
        /// Adds a control to the container
        /// </summary>
        /// <param name="input"></param>
        public void Add(Control input)
        {
            if (input.parent != null)
                input.parent.Rem(input);
            controls.Add(input);
            input.parent = this;
            if (input.Focusable)
            {
                input.tab = tablen;
                tablen++;
            }
            input.SetRelBounds();
        }

        /// <summary>
        /// Removes a control from the container
        /// </summary>
        /// <param name="input"></param>
        public void Rem(Control input)
        {
            if (input.parent != this)
                return;
            controls.Remove(input);
            input.parent = null;
            input.SetRelBounds();
        }

        /// <summary>
        /// Adds a container to the container
        /// </summary>
        /// <param name="input"></param>
        public void Add(Container input)
        {
            if (input.parent != null)
                input.parent.Rem(input);
            containers.Add(input);
            input.parent = this;
            input.SetRelBounds();
        }

        /// <summary>
        /// Removes a container from the container
        /// </summary>
        /// <param name="input"></param>
        public void Rem(Container input)
        {
            if (input.parent != this)
                return;
            containers.Remove(input);
            input.parent = null;
            input.SetRelBounds();
        }

        /// <summary>
        /// Counts the controls/containers on the container
        /// </summary>
        public int ControlCount
        {
            get { return controls.Count; }
        }

        /// <summary>
        /// Only needed if container bounds were changed after controls were added
        /// </summary>
        public void RepositionControls()
        {
            foreach (Control ctrl in controls)
                ctrl.SetRelBounds();
        }

        /// <summary>
        /// Only needed if container bounds were changed after controls were added
        /// </summary>
        public void RepositionContainers()
        {
            foreach (Container ctrl in containers)
                ctrl.SetRelBounds();
        }

        /// <summary>
        /// Draws the container
        /// </summary>
        public override void Draw()
        {
            for (int y = bounds.y; y < bounds.y + bounds.height; y++)
            {
                for (int x = bounds.x; x < bounds.x + bounds.width; x++)
                {
                    ConsoleBase.cbuf[x, y].fg = fg;
                    ConsoleBase.cbuf[x, y].bg = bg;
                    ConsoleBase.cbuf[x, y].ch = ' ';
                    ConsoleBase.cbuf[x, y].dr = true;
                }
            }
            //Draw controls
            foreach (Control c in controls)
                if (c.visible)
                    c.Draw();
            ConsoleBase.Draw(bounds.x, bounds.y, bounds.width, bounds.height);
        }

        /// <summary>
        /// Regenerates the tabs based on the collection
        /// </summary>
        public void RegenTabs()
        {
            uint idx = 0;
            foreach (Control ctrl in controls)
                if (ctrl.Focusable)
                    ctrl.tab = idx++;
        }

        /// <summary>
        /// Container's helper mehtod to change to the next control from the current
        /// </summary>
        /// <param name="input"></param>
        public void NextFocus(Control input)
        {
            input.UnFocus();
            uint tab = 0;
            tab = input.tab;
            do
            {
                if (tab == tablen - 1) tab = 0;
                else tab += 1;
                foreach (Control c in controls)
                {
                    if (c.CanFocus && c.tab == tab)
                    {
                        c.Focus();
                        return;
                    }
                }
            }
            while (tab != input.tab);
        }

        /// <summary>
        /// Container's helper mehtod to change to the previous control from the current
        /// </summary>
        /// <param name="input"></param>
        public void PrevFocus(Control input)
        {
            input.UnFocus();
            uint tab = 0;
            tab = input.tab;
            do
            {
                if (tab == 0) tab = tablen - 1;
                else tab -= 1;
                foreach (Control c in controls)
                {
                    if (c.CanFocus && c.tab == tab)
                    {
                        c.Focus();
                        return;
                    }
                }
            }
            while (tab != input.tab);
        }
    }

    /// <summary>
    /// Base class of the controls, which can be added to containers, inherited from ControlBase
    /// </summary>
    public class Control : ControlBase
    {
        event KeyPress hndl;

        /// <summary>
        /// Delegate for the control's keypress event
        /// </summary>
        /// <param name="sender">Current control</param>
        /// <param name="e">Pressed key information</param>
        /// <returns></returns>
        public delegate bool KeyPress(object sender, ConsoleKeyInfo e);
        static ConsoleKeyPress ckp;

        /// <summary>
        /// Control's selected state background color
        /// </summary>
        protected ConsoleColor sbg;
        /// <summary>
        /// Control's selected state foreground color
        /// </summary>
        protected ConsoleColor sfg;
        /// <summary>
        /// Control's current state background color
        /// </summary>
        protected ConsoleColor cbg;
        /// <summary>
        /// Control's current state foreground color
        /// </summary>
        protected ConsoleColor cfg;

        internal uint tab;
        internal string text;

        /// <summary>
        /// Control can be focused
        /// </summary>
        protected bool focusable;
        static Control ctrl;

        static Thread thd;
        static ConsoleKeyInfo keyinfo;
        static bool active;

        internal static uint ids;
        uint id;

        /// <summary>
        /// Gets/Sets the control's selected state background color
        /// </summary>
        public ConsoleColor SBg
        {
            get { return sbg; }
            set { sbg = value; }
        }

        /// <summary>
        /// Gets/Sets the control's selected state foreground color
        /// </summary>
        public ConsoleColor SFg
        {
            get { return sfg; }
            set { sfg = value; }
        }

        /// <summary>
        /// Gets/Sets the control's current state foreground color
        /// </summary>
        public ConsoleColor CBg
        {
            get { return cbg; }
        }

        /// <summary>
        /// Gets/Sets the control's current state foreground color
        /// </summary>
        public ConsoleColor CFg
        {
            get { return cfg; }
        }

        /// <summary>
        /// Gets/Sets the control's tab
        /// </summary>
        public uint Tab
        {
            get { return tab; }
            set { tab = value; }
        }

        /// <summary>
        /// Gets/Sets the control's text
        /// </summary>
        public virtual string Text
        {
            get { return text; }
            set { text = value; }
        }

        /// <summary>
        /// Gets if the control is focusable
        /// </summary>
        public bool Focusable
        {
            get { return focusable; }
        }

        /// <summary>
        /// Gets the current focused control
        /// </summary>
        public static Control ActiveControl
        {
            get { return ctrl; }
        }

        /// <summary>
        /// Gets the controls' event handler status including focus changing behaviours
        /// </summary>
        public static bool Active
        {
            get { return active; }
        }

        /// <summary>
        /// Gets control's current Id
        /// </summary>
        public uint Id
        {
            get { return id; }
        }

        /// <summary>
        /// Gets if the control can be focused right now
        /// </summary>
        public bool CanFocus
        {
            get { return focusable && enabled && visible; }
        }

        /// <summary>
        /// Keypress handler for the current control
        /// </summary>
        public event KeyPress KeyHandler
        {
            add { this.hndl += value; }
            remove { this.hndl -= value; }
        }

        static Control()
        {
            ckp = new ConsoleKeyPress();
            EnableHandlers(true);
        }

        /// <summary>
        /// Creates a new instance of control
        /// </summary>
        public Control()
        {
            bounds = new Rect();
            fg = ConsoleColor.White;
            bg = ConsoleColor.Blue;
            sfg = ConsoleColor.Blue;
            sbg = ConsoleColor.White;
            cfg = fg;
            cbg = bg;
            enabled = true;
            visible = true;
            focusable = true;
            id = ids++;
            text = "";
        }

        /// <summary>
        /// Enables/Disables all controls event handlers including focus chaning behaviours
        /// </summary>
        /// <param name="input"></param>
        public static void EnableHandlers(bool input)
        {
            if (active == input) return;

            active = input;
            if (active)
            {
                thd = new Thread(Thd);
                thd.Start();
            }
            else
                NativeClass.NullThread(ref thd);
        }

        /// <summary>
        /// Redraws the control, overrideable
        /// </summary>
        public override void Draw()
        {
            string tmp = text.PadRight(bounds.width);
            for (int i = 0, x = bounds.x; i < tmp.Length; i++, x++)
                ConsoleBase.cbuf[x, bounds.y].SetBuffer(cfg, cbg, tmp[i]);
            ConsoleBase.Draw(bounds);
        }

        /// <summary>
        /// Changes the focus to the control, if the control supports it, overrideable
        /// </summary>
        public virtual void Focus()
        {
            if (!CanFocus) return;

            cfg = sfg;
            cbg = sbg;
            focused = true;
            ctrl = this;
            Draw();
        }

        internal virtual void UnFocus()
        {
            if (!CanFocus) return;

            focused = false;
            cfg = fg;
            cbg = bg;
            Draw();
        }

        static void Thd()
        {
            while (active)
            {
                Thread.Sleep(ConsoleBase.TIME_WAIT);
                if (ctrl == null || !ctrl.focused) continue;
                if (!ckp.GetKey(out keyinfo)) continue;

                if (ctrl.hndl == null || ctrl.hndl(ctrl, keyinfo))
                {
                    if (keyinfo.Key == ConsoleKey.Tab || keyinfo.Key == ConsoleKey.DownArrow)
                    {
                        ((Container)ctrl.parent).NextFocus(ctrl);
                        continue;
                    }
                    if (keyinfo.Key == ConsoleKey.UpArrow)
                    {
                        ((Container)ctrl.parent).PrevFocus(ctrl);
                        continue;
                    }
                    ctrl.GetInput(keyinfo);
                }
            }
        }

        /// <summary>
        /// Control's input handler, overrideable
        /// </summary>
        /// <param name="e"></param>
        protected virtual void GetInput(ConsoleKeyInfo e) { }

        /// <summary>
        /// Changes the parent of the control
        /// </summary>
        /// <param name="input"></param>
        public void SetParent(Container input)
        {
            if (parent != null)
                parent.Rem(this);
            parent.Add(this);
        }

        /// <summary>
        /// Gets the control's parent, otherwise returns null
        /// </summary>
        /// <returns></returns>
        public Container GetParent()
        {
            return parent;
        }
    }

    /// <summary>
    /// Label, supports multiple lines
    /// </summary>
    public class Label : Control
    {
        /// <summary>
        /// Creates a new instance of label
        /// </summary>
        public Label()
        {
            focusable = false;
        }

        /// <summary>
        /// Redraws the label, overrideable
        /// </summary>
        public override void Draw()
        {
            string tmp;
            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                tmp = lines[i].PadRight(bounds.width);
                for (int i2 = 0, x = bounds.x; i2 < tmp.Length; i2++, x++)
                    ConsoleBase.cbuf[x, bounds.y + i].SetBuffer(cfg, cbg, tmp[i2]);
            }
        }
    }

    /// <summary>
    /// TextBox, only supports single line, height is ignored
    /// </summary>
    public class TextBox : Control
    {
        int idx;
        internal int maxlen;
        char[] data;

        /// <summary>
        /// Gets/Sets the maximum length of the text box
        /// </summary>
        public int MaxLen
        {
            get { return maxlen; }
            set
            {
                if (value < 0)
                    throw new Exception();
                maxlen = value;
            }
        }

        /// <summary>
        /// Gets/Sets the text of the text box
        /// </summary>
        public override string Text
        {
            get { return text; }
            set
            {
                if (value.Length > maxlen)
                    throw new Exception();
                for (int i = 0; i < value.Length; i++)
                    data[i] = value[i];
                idx = value.Length;
            }
        }

        /// <summary>
        /// Creates a new instance of text box
        /// </summary>
        public TextBox()
        {
            bg = ConsoleColor.Black;
            fg = ConsoleColor.White;
            sbg = ConsoleColor.White;
            sfg = ConsoleColor.Black;
            cfg = fg;
            cbg = bg;
            idx = 0;
            maxlen = 32;
            data = new char[maxlen];
        }

        /// <summary>
        /// Redraws the text box, overrideable
        /// </summary>
        public override void Draw()
        {
            //Field
            int i, x;
            for (x = bounds.x; x < bounds.x + bounds.width; x++)
                ConsoleBase.cbuf[x, bounds.y].SetBuffer(cfg, cbg, ' ');
            //Text
            i = idx - bounds.width + 1;
            if (i < 0) i = 0;
            for (x = bounds.x; i < idx; i++, x++)
                ConsoleBase.cbuf[x, bounds.y].SetBuffer(cfg, cbg, data[i]);
            //Cursor if focused
            if (focused)
                ConsoleBase.cbuf[x, bounds.y].SetBuffer(cfg, cbg, '_');
            ConsoleBase.Draw(bounds);
        }

        /// <summary>
        /// TextBox's input handler, overrideable
        /// </summary>
        /// <param name="e"></param>
        protected override void GetInput(ConsoleKeyInfo e)
        {
            char tmp = e.KeyChar;
            if (idx < maxlen && (char.IsLetterOrDigit(tmp) || tmp == ' '))
                data[idx++] = tmp;
            else if (idx > 0 && tmp == '\b')
                data[--idx] = ' ';
            else return;
            text = new string(data, 0, idx);
            Draw();
        }
    }

    /// <summary>
    /// Button, clickable
    /// </summary>
    public class Button : Control { }

    /// <summary>
    /// CheckBox, can be checked and unchecked, height is ignored
    /// </summary>
    public class CheckBox : Control
    {
        bool check;

        /// <summary>
        /// Gets/Sets the check box's checked state
        /// </summary>
        public bool Check
        {
            get { return check; }
            set { check = value; }
        }

        /// <summary>
        /// Redraws the check box, overrideable
        /// </summary>
        public override void Draw()
        {
            string tmp = ("[" + (check ? "X" : " ") + "] " + text).PadRight(bounds.width);
            for (int i = 0, x = bounds.x; i < tmp.Length; i++, x++)
                ConsoleBase.cbuf[x, bounds.y].SetBuffer(cfg, cbg, tmp[i]);
            for (int x = bounds.x; x < bounds.width; x++)
                ConsoleBase.cbuf[x, y].SetBuffer(cfg, cbg, ' ');
            ConsoleBase.Draw(bounds);
        }

        /// <summary>
        /// Checkbox's input handler, overrideable
        /// </summary>
        /// <param name="e"></param>
        protected override void GetInput(ConsoleKeyInfo e)
        {
            if (e.Key == ConsoleKey.Enter)
            {
                check = !check;
                Draw();
            }
        }
    }

    /// <summary>
    /// ListBox, can highlight and select elements
    /// </summary>
    public class ListBox : Control
    {
        internal List<string> items;
        int idx;
        int scr;
        static char up_a = '▲';
        static char dw_a = '▼';

        /// <summary>
        /// Gets/Sets the current selected index from the list box
        /// </summary>
        public int Idx
        {
            get { return idx; }
            set
            {
                if (value < 0 || value > items.Count - 1)
                    throw new Exception();
                idx = value;
                if (idx < scr) scr = idx;
                else if (idx >= scr + bounds.height)
                {
                    scr = idx;
                    if (scr > items.Count - bounds.height)
                        scr = items.Count - bounds.height;
                }
            }
        }

        /// <summary>
        /// Gets the items from the list box
        /// </summary>
        public List<string> Items
        {
            get { return items; }
        }

        /// <summary>
        /// Creates a new instance of list box
        /// </summary>
        public ListBox()
        {
            items = new List<string>();
            idx = 0;
            scr = 0;
        }

        /// <summary>
        /// Redraws the list box, overrideable
        /// </summary>
        public override void Draw()
        {
            string tmp;
            for (int s = scr, y = bounds.y; y < bounds.y + bounds.height; y++, s++)
            {
                tmp = items[s].PadRight(bounds.width - 1);
                if (s == idx)
                    for (int i = 0, x = bounds.x; i < tmp.Length; i++, x++)
                        ConsoleBase.cbuf[x, y].SetBuffer(sfg, sbg, tmp[i]);
                else
                    for (int i = 0, x = bounds.x; i < tmp.Length; i++, x++)
                        ConsoleBase.cbuf[x, y].SetBuffer(fg, bg, tmp[i]);
                ConsoleBase.cbuf[bounds.x + bounds.width - 1, y].SetBuffer(fg, bg, '█');
            }
            ConsoleBase.cbuf[bounds.x + bounds.width - 1, bounds.y].SetBuffer(cfg, cbg, up_a);
            ConsoleBase.cbuf[bounds.x + bounds.width - 1, bounds.y + bounds.height - 1].SetBuffer(cfg, cbg, dw_a);
            ConsoleBase.Draw(bounds);
        }

        /// <summary>
        /// ListBox's input handler, overrideable
        /// </summary>
        /// <param name="e"></param>
        protected override void GetInput(ConsoleKeyInfo e)
        {
            if (e.Key == ConsoleKey.LeftArrow && idx > 0)
                idx--;
            else if (e.Key == ConsoleKey.RightArrow && idx < items.Count - 1)
                idx++;
            if (idx >= scr + bounds.height && scr < items.Count - bounds.height)
                scr++;
            else if (idx < scr && scr > 0)
                scr--;
            Draw();
        }
    }

    /// <summary>
    /// NumericUpDown, can move between min and max values with specified offset
    /// </summary>
    public class NumericUpDown : Control
    {
        decimal min, max;
        decimal val;
        decimal offs;
        string format;
        static char lf_a = '◄';
        static char rt_a = '►';

        /// <summary>
        /// Gets/Sets the numeric up-down's minimum value
        /// </summary>
        public decimal Min
        {
            get { return min; }
            set
            {
                min = value;
                if (min > max) max = min;
                if (val < min) Val = min;
            }
        }

        /// <summary>
        /// Gets/Sets the numeric up-down's maximum value
        /// </summary>
        public decimal Max
        {
            get { return max; }
            set
            {
                max = value;
                if (max < min) min = max;
                if (val > max) Val = max;
            }
        }

        /// <summary>
        /// Gets/Sets the numeric up-down's value
        /// </summary>
        public decimal Val
        {
            get { return val; }
            set
            {
                if (value < min || value > max)
                    throw new Exception();
                val = value;
                text = val.ToString(format);
            }
        }

        /// <summary>
        /// Gets/Sets the numeric up-down's offset
        /// </summary>
        public decimal Offs
        {
            get { return offs; }
            set
            {
                if (value < 0)
                    throw new Exception();
                offs = value;
            }
        }

        /// <summary>
        /// Gets/Sets the numeric up-down's number format
        /// </summary>
        public string Format
        {
            get { return format; }
            set
            {
                format = value;
                text = val.ToString(format);
            }
        }

        /// <summary>
        /// Creates a new instance of numeric up-down
        /// </summary>
        public NumericUpDown()
        {
            bg = ConsoleColor.Black;
            cbg = bg;
            format = "0.00";
            val = 0;
            offs = 1;
            text = val.ToString(format);
        }

        /// <summary>
        /// Redraws the numeric up-down, overrideable
        /// </summary>
        public override void Draw()
        {
            string tmp = text.PadRight(bounds.width - 2);
            for (int i = 0, x = bounds.x + 1; i < tmp.Length; i++, x++)
                ConsoleBase.cbuf[x, bounds.y].SetBuffer(fg, bg, tmp[i]);
            ConsoleBase.cbuf[bounds.x, bounds.y].SetBuffer(cfg, cbg, lf_a);
            ConsoleBase.cbuf[bounds.x + bounds.width - 1, bounds.y].SetBuffer(cfg, cbg, rt_a);
            ConsoleBase.Draw(bounds);
        }

        /// <summary>
        /// NumericUpDowns input handler, overrideable
        /// </summary>
        /// <param name="e"></param>
        protected override void GetInput(ConsoleKeyInfo e)
        {
            if (e.Key == ConsoleKey.LeftArrow)
                val -= offs;
            else if (e.Key == ConsoleKey.RightArrow)
                val += offs;
            if (val < min) val = min;
            else if (val > max) val = max;
            text = val.ToString(format);
            Draw();
        }
    }

    /// <summary>
    /// ProgressBar, can display current progress, height is ignored
    /// </summary>
    public class ProgressBar : Control
    {
        int min, max;
        int val;

        /// <summary>
        /// Gets/Sets the progress bar's minimum value
        /// </summary>
        public int Min
        {
            get { return min; }
            set
            {
                min = value;
                if (min > max) max = min;
                if (val < min) val = min;
            }
        }

        /// <summary>
        /// Gets/Sets the progress bar's maximum value
        /// </summary>
        public int Max
        {
            get { return max; }
            set
            {
                max = value;
                if (max < min) min = max;
                if (val > max) val = max;
            }
        }

        /// <summary>
        /// Gets/Sets the progress bar's value
        /// </summary>
        public int Val
        {
            get { return val; }
            set
            {
                if (value < min || value > max)
                    throw new Exception();
                val = value;
            }
        }

        /// <summary>
        /// Creates a new instance of progress bar
        /// </summary>
        public ProgressBar()
        {
            bg = ConsoleColor.Black;
            sbg = ConsoleColor.Yellow;
            val = 0;
            min = 0;
            max = 100;
            focusable = false;
        }

        /// <summary>
        /// Redraws the progress bar, overrideable
        /// </summary>
        public override void Draw()
        {
            int tmp = (int)(bounds.width * ((float)val / (max - min)));
            for (int i = 0; i < tmp; i++)
                ConsoleBase.cbuf[bounds.x + i, bounds.y].SetBuffer(sfg, sbg, ' ');
            for (int i = tmp; i < bounds.width; i++)
                ConsoleBase.cbuf[bounds.x + i, bounds.y].SetBuffer(fg, bg, ' ');
            ConsoleBase.Draw(bounds);
        }
    }
}