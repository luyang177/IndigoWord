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
         */
        public static int FindClosestColumn(this TextLine textLine, Rect srcRect)
        {
            var srcPt = srcRect.Center();
            return textLine.FindClosestColumn(srcPt);
        }

        /*
         * Find closest column according by given point
         */
        public static int FindClosestColumn(this TextLine textLine, Point point)
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

            //if given point is in the right side of the character, then return col + 1 
            var width = textLine.GetTextBounds(col, 1).First().Rectangle.Width;
            var left = textLine.GetDistanceFromCharacterHit(new CharacterHit(col, 0));
            var centerX = left + width * 0.5;

            //tip: textLine.Length is about single TextLine, however, col is start from 0 which from the first TextLine.
            return point.X > centerX && textLine.Length - 1 > col - info.StartCharPos + 1 ? col + 1 : col;
        }
    }
}
