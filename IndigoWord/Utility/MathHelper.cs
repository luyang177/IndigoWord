using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace IndigoWord.Utility
{
    static class MathHelper
    {
        public static double Distance(IEnumerable<Point> points)
        {
            var list = points.ToList();
            if (list.Count() < 2)
                return 0.0;

            double sum = 0.0;
            for (int i = 0; i < list.Count(); i++)
            {
                if (i + 1 < list.Count)
                    sum += Distance(list[i], list[i + 1]);
            }
            return sum;
        }

        public static double Distance(IEnumerable<Point> points, double? factor)
        {
            var length = Distance(points);
            length = factor.HasValue ? length * factor.Value : length;
            return length;
        }

        public static double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow((p1.X - p2.X), 2) + Math.Pow((p1.Y - p2.Y), 2));
        }

        public static double Distance(Point p1, Point p2, double? factor)
        {
            var length = Distance(p1, p2);
            length = factor.HasValue ? length * factor.Value : length;
            return length;
        }        
    }
}
