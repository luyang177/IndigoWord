using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndigoWord.Render
{
    public interface IOffset
    {
        double HorizontalOffset { get; }

        //Document vary from top to bottom means this offset vary from 0 to a positive number.
        double VerticalOffset { get; }
    }
}
