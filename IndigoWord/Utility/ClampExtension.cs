using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndigoWord.Utility
{
    static class ClampExtension
    {
        public static T Clamp<T>(this T self, T min, T max) where T : IComparable<T>
        {
            var result = self;
            if (result.CompareTo(min) < 0)
            {
                result = min;
            }
            else if (result.CompareTo(max) > 0)
            {
                result = max;
            }

            return result;
        }
    }
}
