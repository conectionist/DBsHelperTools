using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBsHelperTools
{
    public partial class Form1 : Form
    {
        bool haveHandle = false;
        float left = 0f, top = 0f;

        int frameWidth = 100;
        int frameHeight = 100;

        ToolBar toolBarWindow;

        bool bUseToolBar;

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

            toolBarWindow = new ToolBar();

            bUseToolBar = true;
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

            AddTools();

            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Icon = Resource1.toolbox;

            toolBarWindow.Show();
        }

        private void AddTools()
        {
            string[] tools = GetAvailableTools();
            string toolsPath = Directory.GetCurrentDirectory() + "\\Tools"; 

            foreach (string tool in tools)
            {
                ToolStripMenuItem genericToolStripMenuItem = new ToolStripMenuItem();

                if(bUseToolBar)
                {
                    toolBarWindow.AddTool(new Tool(tool, Icon.ExtractAssociatedIcon(string.Format("{0}\\{1}\\{2}.exe", toolsPath, tool, tool)).ToBitmap()));
                }
                else
                {
                    genericToolStripMenuItem.Name = tool + "_ToolStripMenuItem";
                    genericToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
                    genericToolStripMenuItem.Text = tool;
                    genericToolStripMenuItem.Click += new System.EventHandler(this.genericToolStripMenuItem_Click);
                    genericToolStripMenuItem.Image = Icon.ExtractAssociatedIcon(string.Format("{0}\\{1}\\{2}.exe", toolsPath, tool, tool)).ToBitmap();
                    this.contextMenuStrip1.Items.Add(genericToolStripMenuItem);
                }
            }

            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);

            // add a separator
            if (!bUseToolBar)
                this.contextMenuStrip1.Items.Add("-");

            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(153, 48);
        }

        private string[] GetAvailableTools()
        {
            string[] directories = Directory.GetDirectories(Directory.GetCurrentDirectory());
            int i = -1;
            for (i = 0; i < directories.Length; i++ )
            {
                if (directories[i].EndsWith("Tools"))
                    break;
            }

            if (i >= directories.Length)
                return new string[] { };

            string[] toolPaths = Directory.GetDirectories(directories[i]);
            if (toolPaths.Length <= 0)
                return new string[] { };

            List<string> validTools = new List<string>();

            foreach (string toolPath in toolPaths)
            {
                if (IsValidTool(toolPath))
                    validTools.Add(Path.GetFileName(toolPath));
            }

            return validTools.ToArray();
        }

        private bool IsValidTool(string toolPath)
        {
            return true;
        }

        private async Task<bool> ShowSplashScreen()
        {
            for (int i = 0; i < transparencies.Length; i++ )
            {
                SetBits(FrameImage(i));
                await Task.Delay(50);
            }

            await Task.Delay(1500);

            Hide();

            return true;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void genericToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path =
                Directory.GetCurrentDirectory() + "\\" +
                "Tools" + "\\" +
                sender.ToString() + "\\" +
                sender.ToString() + ".exe";

            if(File.Exists(path))
            {
                System.Diagnostics.Process.Start(path);
            }
        }
    }
}
