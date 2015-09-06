using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndigoWord.Utility
{
    static class CompareExtension
    {
        public static T Clamp<T>(this T self, T min, T max) where T : IComparable<T>
        {
            if (self.CompareTo(min) < 0)
            {
                return min;
            }

            if (self.CompareTo(max) > 0)
            {
                return max;
            }

            return self;
        }

    }
}
