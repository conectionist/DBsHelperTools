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
    public partial class ToolBar : Form
    {
        private List<Tool> tools;
        private ToolTip toolTip;
        public ToolBar()
        {
            InitializeComponent();

            tools = new List<Tool>();

            toolTip = new ToolTip();

            this.Height = Screen.PrimaryScreen.WorkingArea.Height;

            Location = new Point(Screen.PrimaryScreen.WorkingArea.Right - this.Width, 0);
        }

        public void AddTool(Tool tool)
        {
            tools.Add(tool);
        }

        private void ToolBar_Load(object sender, EventArgs e)
        {
            int i = 0;

            foreach(Tool tool in tools)
            {
                Button b = new Button();
                b.Name = tool.name;
                b.Image = tool.image;
                b.Size = new Size(40, 40);
                b.Location = new Point(5, 5 + 45 * i);
                b.Click += ToolBar_BtnClick;

                toolTip.SetToolTip(b, b.Name);

                Controls.Add(b);

                i++;
            }
        }

        private void ToolBar_BtnClick(object sender, EventArgs e)
        {
            Button b = sender as Button;
            if (b == null)
                return;

            string path =
                Directory.GetCurrentDirectory() + "\\" +
                "Tools" + "\\" +
                b.Name + "\\" +
                b.Name + ".exe";

            if (File.Exists(path))
            {
                System.Diagnostics.Process.Start(path);
            }
        }
    }
}
