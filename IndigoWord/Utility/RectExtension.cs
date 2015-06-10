using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IndigoWord.Utility
{
    static class RectExtension
    {
        public static Point Center(this Rect rc)
        {
            return new Point(rc.Left + rc.Width * 0.5,
                             rc.Top + rc.Height * 0.5);
        }
    }
}
