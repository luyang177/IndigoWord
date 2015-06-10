using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using IndigoWord.Core;

namespace IndigoWord.LowFontApi
{
    static class TextRender
    {
        public static IList<TextLine> Render(DrawingContext dc, 
                                             string text, 
                                             bool isWrap)
        {
            var textLines = new List<TextLine>();
            int textStorePosition = 0;
            double pos = 0;

            //TODO FontRendering can reuse?
            var render = new FontRendering();
            var textStore = new CustomTextSource
            {
                Text = text,
                FontRendering = render
            };

            var formatter = TextFormatter.Create();

            while (textStorePosition < textStore.Text.Length)
            {
                var textLine = formatter.FormatLine(
                    textStore,
                    textStorePosition,
                    30 * 6, //paragraphWidth only works for Wrap
                    new GenericTextParagraphProperties(render, isWrap ? TextWrapping.Wrap : TextWrapping.NoWrap),
                    null);

                TextLineInfoManager.Add(textLine, new TextLineInfo
                {
                    Top = pos,
                    StartCharPos = textStorePosition,
                    EndCharPos = textLine.Length
                });

                textLine.Draw(dc, new Point(0, pos), InvertAxes.None);
                textStorePosition += textLine.Length;
                pos += textLine.Height;

                textLines.Add(textLine);
            }

            return textLines;
        }
    }
}
