using System;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleEx
{
    /// <summary>
    /// Extended container with title, borders and shadows
    /// </summary>
    public class Form : Container
    {
        /// <summary>
        /// Form's border style
        /// </summary>
        public enum BorderStyle : uint
        {
            /// <summary>
            /// No border
            /// </summary>
            None,
            /// <summary>
            /// Single line border
            /// </summary>
            Single,
            /// <summary>
            /// Double line border
            /// </summary>
            Double
        }

        /// <summary>
        /// Form's shadow style
        /// </summary>
        public enum ShadowStyle : uint
        {
            /// <summary>
            /// No shadow
            /// </summary>
            None,
            /// <summary>
            /// Full shadow
            /// </summary>
            Full,
            /// <summary>
            /// High density shadow
            /// </summary>
            High,
            /// <summary>
            /// Medium density shadow
            /// </summary>
            Med,
            /// <summary>
            /// Low density shadow
            /// </summary>
            Low
        }

        BorderStyle border;
        ShadowStyle shadow;

        ConsoleColor tfg;
        ConsoleColor sbg, sfg;

        internal string text;

        /// <summary>
        /// Gets/Sets the form's border style
        /// </summary>
        public BorderStyle Border
        {
            get { return border; }
            set { border = value; }
        }

        /// <summary>
        /// Gets/Sets the form's shadow style
        /// </summary>
        public ShadowStyle Shadow
        {
            get { return shadow; }
            set { shadow = value; }
        }

        /// <summary>
        /// Gets/Sets the form's text foreground color
        /// </summary>
        public ConsoleColor TFg
        {
            get { return tfg; }
            set { tfg = value; }
        }

        /// <summary>
        /// Gets/Sets the form's shadow background color
        /// </summary>
        public ConsoleColor SBg
        {
            get { return sbg; }
            set { sbg = value; }
        }

        /// <summary>
        /// Gets/Sets the form's shadow foreground color
        /// </summary>
        public ConsoleColor SFg
        {
            get { return sfg; }
            set { sfg = value; }
        }

        /// <summary>
        /// Gets/Sets the form's text
        /// </summary>
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        static char[,] fr =
        {
            {'─','│','┌','┐','└','┘'},
            {'═','║','╔','╗','╚','╝'}
        };

        static char[] sw = { '█', '▓', '▒', '░' };

        /// <summary>
        /// Creates a new instance of form
        /// </summary>
        public Form()
        {
            SetBounds(0, 0, 22, 12);
            fg = ConsoleColor.Cyan;
            bg = ConsoleColor.Blue;
            sfg = ConsoleColor.Gray;
            sbg = ConsoleColor.Black;
            tfg = ConsoleColor.White;
            border = Form.BorderStyle.Single;
            shadow = Form.ShadowStyle.Med;
            ConsoleBase.forms.Add(this);
        }

        /// <summary>
        /// Redraws the form and all of its controls
        /// </summary>
        public override void Draw()
        {
            base.Draw();
            uint br = (uint)border - 1;
            uint sh = (uint)shadow - 1;
            //Draw form
            if (border != BorderStyle.None)
            {
                for (int y = bounds.y + 1; y < bounds.y + 1 + bounds.height - 1; y++)
                {
                    ConsoleBase.cbuf[bounds.x, y].SetBuffer(fg, bg, fr[br, 1]);
                    ConsoleBase.cbuf[bounds.x + bounds.width - 1, y].SetBuffer(fg, bg, fr[br, 1]);
                }
                //Borders
                Console.SetCursorPosition(bounds.x, bounds.y);
                for (int x = bounds.x; x < bounds.x + bounds.width; x++)
                    ConsoleBase.cbuf[x, bounds.y].SetBuffer(fg, bg, fr[br, 0]);
                for (int x = bounds.x; x < bounds.x + bounds.width; x++)
                    ConsoleBase.cbuf[x, bounds.y + bounds.height - 1].SetBuffer(fg, bg, fr[br, 0]);
                //Corners
                ConsoleBase.cbuf[bounds.x, bounds.y].SetBuffer(fg, bg, fr[br, 2]);
                ConsoleBase.cbuf[bounds.x + bounds.width - 1, bounds.y].SetBuffer(fg, bg, fr[br, 3]);
                ConsoleBase.cbuf[bounds.x, bounds.y + bounds.height - 1].SetBuffer(fg, bg, fr[br, 4]);
                ConsoleBase.cbuf[bounds.x + bounds.width - 1, bounds.y + bounds.height - 1].SetBuffer(fg, bg, fr[br, 5]);
            }
            //Title
            if (text != null && text.Length > 0)
                for (int i = 0, x = (bounds.x + bounds.x + bounds.width) / 2 - text.Length / 2; i < text.Length; i++, x++)
                    ConsoleBase.cbuf[x, bounds.y].SetBuffer(tfg, bg, text[i]);
            //Shadow
            if (shadow != ShadowStyle.None)
            {
                for (int y = bounds.y + 1; y < bounds.y + 1 + bounds.height; y++)
                    ConsoleBase.cbuf[bounds.x + bounds.width, y].SetBuffer(sfg, sbg, sw[sh]);
                for (int x = bounds.x + 1; x < bounds.x + 1 + bounds.width; x++)
                    ConsoleBase.cbuf[x, bounds.y + bounds.height].SetBuffer(sfg, sbg, sw[sh]);
            }
            //Draw controls
            foreach (Control c in controls)
                if (c.visible)
                    c.Draw();
            ConsoleBase.Draw(bounds.x, bounds.y, bounds.width + 1, bounds.height + 1);
        }
    }

    /// <summary>
    /// Simple message box with selectable options
    /// </summary>
    public class MessageBox : Form
    {
        private MessageBox() { }

        internal static bool active = false;
        static MessageBox msg = null;
        static Label lbl = null;
        static Button[] btns = null;
        static uint retval;

        /// <summary>
        /// Shows the message box
        /// </summary>
        /// <param name="text">Title</param>
        /// <param name="message">Text</param>
        /// <param name="buttons">Selectable buttons as string array</param>
        /// <returns>Index of the selected button</returns>
        public static uint Show(string text, string message, string[] buttons)
        {
            active = true;
            if (msg == null)
            {
                msg = new MessageBox();
                lbl = new Label();
                msg.Add(lbl);
                btns = new Button[3];
                for (int i = 0; i < btns.Length; i++)
                {
                    btns[i] = new Button();
                    btns[i].KeyHandler += MessageBox_KeyHandler;
                    msg.Add(btns[i]);
                }
            }
            msg.visible = true;
            string[] tmp = message.Split('\n');
            int cur, max = 0;
            for (int i = 0; i < tmp.Length; i++)
            {
                cur = tmp[i].Length;
                if (cur > max) max = cur;
            }
            msg.text = text;
            lbl.text = message;
            lbl.SetBounds(0, 1, max, tmp.Length);
            cur = 0;
            for (int i = 0; i < buttons.Length; i++)
                cur += buttons[i].Length + 1;
            cur -= 1;
            if (cur > max) max = cur;
            int startx = max / 2 - cur / 2;
            for (int i = 0; i < btns.Length; i++)
            {
                if (i < buttons.Length)
                {
                    btns[i].visible = true;
                    btns[i].text = buttons[i];
                    if (i == 0) btns[i].x = startx;
                    else btns[i].x = btns[i - 1].x + btns[i - 1].bounds.width + 1;
                    btns[i].y = lbl.bounds.y + lbl.bounds.height + 1;
                    btns[i].bounds.width = buttons[i].Length;
                    btns[i].bounds.height = 1;
                    btns[i].SetRelBounds();
                }
                else
                {
                    btns[i].visible = false;
                    btns[i].text = "";
                }
            }
            int width = max + 2, height = btns[0].bounds.y + 2;
            msg.SetBounds(ConsoleBase.width / 2 - width / 2,
                ConsoleBase.height / 2 - height / 2, width, height);
            msg.RepositionControls();
            btns[0].Focus();
            msg.Draw();
            while (active) Thread.Sleep(1);
            msg.visible = false;
            return retval;
        }

        static bool MessageBox_KeyHandler(object sender, ConsoleKeyInfo e)
        {
            if (e.Key == ConsoleKey.Enter)
            {
                retval = ((Button)sender).tab;
                active = false;
            }
            return true;
        }
    }
}