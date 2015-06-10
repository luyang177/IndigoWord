using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndigoWord.Utility
{
    static class CompareHelper
    {
        private static readonly double Tolerance = 0.0000001;

        public static bool CloseTo(this double self, double other, double tolerance)
        {
            return Math.Abs(self - other) < tolerance;
        }

        public static bool CloseTo(this double self, double other)
        {
            return self.CloseTo(other, Tolerance);
        }
    }
}
