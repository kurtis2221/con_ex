using System;
using ConsoleEx;

namespace con_ex_demo
{
    class Program
    {
        static bool mouse = false;
        static Form frm;
        static Label lb;
        static TextBox tb;
        static Button bt;
        static Button bt2;
        static ProgressBar pb;
        static CheckBox cb;
        static ListBox lbx;
        static NumericUpDown nm;

        static void Main(string[] args)
        {
            Console.Title = "Extended Console Class Test";
            ConsoleBase.Init();
            ConsoleBase.SetHotkeyHandler(HotKeyPressed);
            frm = new Form();
            frm.Text = "[Form]";
            frm.SetBounds(0, 0, 25, 20);
            //
            lb = new Label();
            lb.SetBounds(0, 0, 10, 1);
            lb.Text = "3Chars";
            frm.Add(lb);
            //
            tb = new TextBox();
            tb.MaxLen = 3;
            tb.SetBounds(10, 0, 10, 1);
            frm.Add(tb);
            //
            lb = new Label();
            lb.SetBounds(0, 2, 10, 1);
            lb.Text = "Text";
            frm.Add(lb);
            //
            tb = new TextBox();
            tb.SetBounds(10, 2, 10, 1);
            tb.Text = "123";
            frm.Add(tb);
            //
            bt = new Button();
            bt.Text = "Plus";
            bt.SetBounds(0, 4, 10, 1);
            bt.KeyHandler += ButtonKeyPressed;
            frm.Add(bt);
            //
            bt2 = new Button();
            bt2.Text = "Minus";
            bt2.SetBounds(10, 4, 10, 1);
            bt2.KeyHandler += ButtonKeyPressed;
            frm.Add(bt2);
            //
            pb = new ProgressBar();
            pb.SetBounds(0, 6, 10, 1);
            pb.Val = 50;
            frm.Add(pb);
            //
            cb = new CheckBox();
            cb.SetBounds(0, 8, 13, 1);
            cb.Text = "IsChecked";
            frm.Add(cb);
            //
            lbx = new ListBox();
            lbx.SetBounds(0, 10, 15, 4);
            lbx.Items.Add("ABC");
            lbx.Items.Add("DEF");
            lbx.Items.Add("GHI");
            lbx.Items.Add("JKL");
            lbx.Items.Add("MNO");
            lbx.Items.Add("PQR");
            lbx.Items.Add("STU");
            lbx.Items.Add("VWX");
            lbx.Items.Add("YZA");
            lbx.Idx = 4;
            frm.Add(lbx);
            //
            nm = new NumericUpDown();
            nm.SetBounds(0, 16, 7, 1);
            nm.Format = "00.00";
            nm.Min = 1;
            nm.Max = 15;
            frm.Add(nm);
            //
            frm.Draw();
            ConsoleMouse.SetMouseKeyHandler(MouseKeyPressed);
            uint retval = MessageBox.Show("Test", "Test Message12345678901234567\nNew Line", new string[] { "Yes", "No", "Maybe" });
            if (retval == 0) tb.Focus();
            else if (retval == 1) bt.Focus();
            else nm.Focus();
            ConsoleBase.ClearScreen();
            frm.Draw();
        }

        static void HotKeyPressed(ConsoleKeyInfo e)
        {
            if (e.Key == ConsoleKey.Escape)
            {
                Environment.Exit(0);
            }
            else if (e.Key == ConsoleKey.F1)
            {
                ConsoleMouse.SetCursorState(mouse = !mouse);
            }
        }

        static void MouseKeyPressed(NativeClass.MouseButtons e)
        {
            if (e == NativeClass.MouseButtons.Left && ConsoleMouse.X == 0 && ConsoleMouse.Y == 0)
            {
                if (pb.Val >= 100)
                    pb.Val = 0;
                else
                    pb.Val += 10;
                pb.Draw();
            }
        }

        static bool ButtonKeyPressed(object sender, ConsoleKeyInfo e)
        {
            if (e.Key == ConsoleKey.Enter)
            {
                if (sender == bt)
                {
                    if (pb.Val >= 100) pb.Val = 0;
                    else pb.Val += 10;
                }
                else
                {
                    if (pb.Val <= 0) pb.Val = 100;
                    else pb.Val -= 10;
                }
                pb.Draw();
            }
            return true;
        }
    }
}