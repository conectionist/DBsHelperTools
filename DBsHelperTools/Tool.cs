using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBsHelperTools
{
    public class Tool
    {
        public string name;
        public Image image;

        public Tool(string _name, Image _image)
        {
            name = _name;
            image = _image;
        }
    }
}
