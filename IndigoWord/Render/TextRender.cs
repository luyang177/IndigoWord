using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using IndigoWord.Core;
using IndigoWord.LowFontApi;

namespace IndigoWord.Render
{
    static class TextRender
    {
        public static IList<TextLine> Render(DrawingContext dc, 
                                             LogicLine logicLine,   
                                             string text, 
                                             FontRendering fontRender,
                                             bool isWrap)
        {
            var textLines = new List<TextLine>();
            int textStorePosition = 0;
            double pos = 0;

            var textStore = new CustomTextSource
            {
                Text = text,
                FontRendering = fontRender
            };

            var formatter = TextFormatter.Create();

            while (textStorePosition < textStore.Text.Length)
            {
                var textLine = formatter.FormatLine(
                    textStore,
                    textStorePosition,
                    30 * 6, //paragraphWidth only works for Wrap
                    new GenericTextParagraphProperties(fontRender, isWrap ? TextWrapping.Wrap : TextWrapping.NoWrap),
                    null);

                var info = new TextLineInfo
                {
                    LogicLine = logicLine,
                    Top = pos,
                    StartCharPos = textStorePosition,
                    EndCharPos = textStorePosition + textLine.Length - 1,
                    IsLast = false
                };
                TextLineInfoManager.Add(textLine, info);

                textLine.Draw(dc, new Point(0, pos), InvertAxes.None);
                textStorePosition += textLine.Length;

                pos += textLine.Height;

                textLines.Add(textLine);

                var isLastTextLine = textStorePosition >= textStore.Text.Length;
                if (isLastTextLine)
                {
                    if (text.EndsWith("\r\n"))
                    {
                        info.EndCharPos--;
                    }
                    info.IsLast = true;
                }
            }

            return textLines;
        }
    }
}
