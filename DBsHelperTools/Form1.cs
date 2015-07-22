using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBsHelperTools
{
    public partial class Form1 : Form
    {
        Point oldPoint = new Point(0, 0);
        bool haveHandle = false;
        float left = 0f, top = 0f;

        int frameWidth = 100;
        int frameHeight = 100;

        private Image[] transparencies = new Image[]
        {
            Resource1.Transparency5,
            Resource1.Transparency10,
            Resource1.Transparency15,
            Resource1.Transparency20,
            Resource1.Transparency25,
            Resource1.Transparency25,
            Resource1.Transparency30,
            Resource1.Transparency35,
            Resource1.Transparency40,
            Resource1.Transparency45,
            Resource1.Transparency50,
            Resource1.Transparency50,
            Resource1.Transparency55,
            Resource1.Transparency60,
            Resource1.Transparency65,
            Resource1.Transparency70,
            Resource1.Transparency75,
            Resource1.Transparency80,
            Resource1.Transparency85,
            Resource1.Transparency90,
            Resource1.Transparency95,
            Resource1.Transparency100
        };

        public Form1()
        {
            InitializeComponent();

            this.TopMost = true;
            frameWidth = FullImage(0).Width;
            frameHeight = FullImage(0).Height;
            left = -frameWidth;
            top = Screen.PrimaryScreen.WorkingArea.Height / 2f;

            this.Left = Screen.PrimaryScreen.WorkingArea.Width / 2 - this.Width;
            this.Top = Screen.PrimaryScreen.WorkingArea.Height / 2 - this.Height;
        }

        #region Override

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            base.OnClosing(e);
            haveHandle = false;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            InitializeStyles();
            base.OnHandleCreated(e);
            haveHandle = true;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cParms = base.CreateParams;
                cParms.ExStyle |= 0x00080000; // WS_EX_LAYERED
                return cParms;
            }
        }

        #endregion

        private void InitializeStyles()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            UpdateStyles();
        }

        private Image FullImage(int i)
        {
            return transparencies[i];
        }

        public Bitmap FrameImage(int i)
        {
            Bitmap bitmap = new Bitmap(frameWidth, frameHeight);
            Graphics g = Graphics.FromImage(bitmap);
            g.DrawImage(FullImage(i),
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                GraphicsUnit.Pixel);
            return bitmap;
        }

        public void SetBits(Bitmap bitmap)
        {
            if (!haveHandle) return;

            if (!Bitmap.IsCanonicalPixelFormat(bitmap.PixelFormat) || !Bitmap.IsAlphaPixelFormat(bitmap.PixelFormat))
                throw new ApplicationException("The picture must be 32bit picture with alpha channel.");

            IntPtr oldBits = IntPtr.Zero;
            IntPtr screenDC = Win32.GetDC(IntPtr.Zero);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr memDc = Win32.CreateCompatibleDC(screenDC);

            try
            {
                Win32.Point topLoc = new Win32.Point(Left, Top);
                Win32.Size bitMapSize = new Win32.Size(bitmap.Width, bitmap.Height);
                Win32.BLENDFUNCTION blendFunc = new Win32.BLENDFUNCTION();
                Win32.Point srcLoc = new Win32.Point(0, 0);

                hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                oldBits = Win32.SelectObject(memDc, hBitmap);

                blendFunc.BlendOp = Win32.AC_SRC_OVER;
                blendFunc.SourceConstantAlpha = 255;
                blendFunc.AlphaFormat = Win32.AC_SRC_ALPHA;
                blendFunc.BlendFlags = 0;

                Win32.UpdateLayeredWindow(Handle, screenDC, ref topLoc, ref bitMapSize, memDc, ref srcLoc, 0, ref blendFunc, Win32.ULW_ALPHA);
            }
            finally
            {
                if (hBitmap != IntPtr.Zero)
                {
                    Win32.SelectObject(memDc, oldBits);
                    Win32.DeleteObject(hBitmap);
                }
                Win32.ReleaseDC(IntPtr.Zero, screenDC);
                Win32.DeleteDC(memDc);
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await ShowSplashScreen();

            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Icon = Resource1.toolbox;
        }

        private async Task<bool> ShowSplashScreen()
        {
            for (int i = 0; i < transparencies.Length; i++ )
            {
                SetBits(FrameImage(i));
                await Task.Delay(50);
            }

            Hide();

            return true;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
