using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace POS
{
    internal class BackGroundColor
    {
        public static void Display_BGColor(object? sender, PaintEventArgs pe)
        {
            PosSystem? main = (PosSystem?)sender;
            if(main != null)
            {
                Rectangle rect = new Rectangle(0,0,main.ClientRectangle.Width, main.ClientRectangle.Height);
                using(SolidBrush solid = new SolidBrush(Color.DarkSlateGray))
                {
                    pe.Graphics.FillRectangle(solid, rect);
                }
            }
        }
    }
}
