using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
namespace PixieColor
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowDC(IntPtr window);
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern uint GetPixel(IntPtr dc, int x, int y);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int ReleaseDC(IntPtr window, IntPtr dc);
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        string hex, rgb;
        int x, y;
        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]

        private static extern int SetProcessWorkingSetSize(IntPtr process, int minimumWorkingSetSize, int maximumWorkingSetSize);

        public static void alzheimer()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle,-1, -1);
        } 
        enum KeyModifier
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8,
            CtrlAlt = 3,
            ALtShift = 5
        }
        public Form1()
        {
            InitializeComponent();
            RegisterHotKey(this.Handle, 0, (int)KeyModifier.CtrlAlt, Keys.C.GetHashCode());
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
            notifyIcon1.Visible = true;
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (!IsStartupItem())
            {
                rkApp.SetValue("Pixel Color", Application.ExecutablePath.ToString());
            }
            panel2.Hide();
            timer1.Start();
            timer1.Interval = 100;
            RegisterHotKey(this.Handle, 1, 6, (int)Keys.C);
    
        }
        private bool IsStartupItem()
        {
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (rkApp.GetValue("Pixel Color") == null)
                return false;
            else
                return true;
        }
        public Color GetColorAt(int x, int y)
        {
            int a = (int)(GetPixel(GetWindowDC(GetDesktopWindow()), x, y));
            ReleaseDC(GetDesktopWindow(), GetWindowDC(GetDesktopWindow()));
            return Color.FromArgb(0, (a >> 0) & 0xff, (a >> 8) & 0xff, (a >> 16) & 0xff);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            x = Cursor.Position.X;
            y = Cursor.Position.Y;
            Color c = GetColorAt(x, y);
            hex = c.Name;
            rgb = "rgb(" + c.R + " , " + c.G + " , " + c.B + ")";
            label1.Text = "Hex: # " + hex;
            label2.Text = "RGB: " + rgb;
            label3.Text = "Position: X : " + x + "  Y : " + y;
            panel1.BackColor = Color.FromArgb(c.R, c.G, c.B);
            alzheimer();
        }
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x0312)
            {
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                KeyModifier modifier = (KeyModifier)((int)m.LParam & 0xFFFF);
                int id = m.WParam.ToInt32();
                if ((int)modifier == 3)
                {
                    x = Cursor.Position.X;
                    y = Cursor.Position.Y;
                    Color c = GetColorAt(x, y);
                    hex = c.Name;
                    rgb = "rgb(" + c.R + " , " + c.G + " , " + c.B + ")";
                    Clipboard.SetText("#" + hex);
                }
                else
                {
                    x = Cursor.Position.X;
                    y = Cursor.Position.Y;
                    Color c = GetColorAt(x, y);
                    hex = c.Name;
                    rgb = "rgb(" + c.R + " , " + c.G + " , " + c.B + ")";
                    Clipboard.SetText(rgb);
                }
            }
        }
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            move(e);
        }

        public void move(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel2.Show();
        }
        private void label8_MouseDown(object sender, MouseEventArgs e)
        {
            move(e);
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
           // timer1.Start();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            notifyIcon1.Visible = true;
            this.Hide();
            timer1.Dispose();
            notifyIcon1.ShowBalloonTip(3000, "Pixel Color minimized ", "Still can pick and copy colors using the shortcuts \n RGB: Ctrl+Shift+C \n HEX: Ctrl+Alt+C ", ToolTipIcon.Info);
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
            timer1.Start();
        }

        private void notifyIcon1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) {
                contextMenuStrip1.Show(Cursor.Position);
            }
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            notifyIcon1.Visible = false;
        }
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://www.facebook.com/soufiane.belchhab");
        }

        private void label7_MouseClick(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://www.facebook.com/soufiane.belchhab");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            panel2.Hide();
        }
    }
}

