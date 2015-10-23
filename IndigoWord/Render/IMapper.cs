using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IndigoWord.Render
{
    interface IMapper
    {
        //Map point from display coordinate to origin coordinate
        Point MapScreen2Origin(Point point);

        //Map point from origin coordinate to display coordinate
        Point MapOrigin2Screen(Point point);
    }
}
