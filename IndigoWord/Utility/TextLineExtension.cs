using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.TextFormatting;
using IndigoWord.Core;

namespace IndigoWord.Utility
{
    static class TextLineExtension
    {
        /*
         * Find closest column according by given rect
         * [canShiftRight] means if the given rect is on the right side of the finded column, then choose the next column as result
         */
        public static int FindClosestColumn(this TextLine textLine, Rect srcRect, bool canShiftRight)
        {
            var srcPt = srcRect.Center();
            return textLine.FindClosestColumn(srcPt, canShiftRight);
        }

        /*
         * Find closest column according by given point
         * [canShiftRight] means if the given point is on the right side of the finded column, then choose the next column as result
         */
        public static int FindClosestColumn(this TextLine textLine, Point point, bool canShiftRight)
        {
            var info = TextLineInfoManager.Get(textLine);

            var col = Enumerable.Range(info.StartCharPos, textLine.Length)
                                .Select(pos => new
                                {
                                    Pos = pos,
                                    Bound = textLine.GetTextBounds(pos, 1).First()
                                })
                                .OrderBy(item => MathHelper.Distance(point, item.Bound.Rectangle.Center()))
                                .First()
                                .Pos;

            if (!canShiftRight)
            {
                return col;
            }

            //if given point is in the right side of the character, then return col + 1 
            var width = textLine.GetTextBounds(col, 1).First().Rectangle.Width;
            var left = textLine.GetDistanceFromCharacterHit(new CharacterHit(col, 0));
            var centerX = left + width * 0.5;

            var next = col + 1;
            var logicLine = info.LogicLine;
            var length = logicLine.Text.Length;
            if (logicLine.Text.EndsWith("\r\n"))
            {
                length--;
            }
            return point.X > centerX && next < length ? next : col;
        }
    }
}
