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
         * Find nearest column according by given rect
         */
        public static int FindNearestColumn(this TextLine textLine, Rect srcRect)
        {
            var srcPt = srcRect.Center();

            var info = TextLineInfoManager.Get(textLine);

            return Enumerable.Range(info.StartCharPos, textLine.Length)
                             .Select(pos => new
                                            {
                                                Pos = pos,
                                                Bound = textLine.GetTextBounds(pos, 1).First()
                                            })
                             .OrderBy(item => MathHelper.Distance(srcPt, item.Bound.Rectangle.Center()))
                             .First()
                             .Pos;
        }
    }
}
