using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using IndigoWord.Core;
using IndigoWord.Utility;

namespace IndigoWord.Edit
{
    static class CaretTraveller
    {
        public static TextPosition DirectionKey(TextDocument document, Caret caret, Key key)
        {
            if (key == Key.Left)
            {
                return document.GetPreviousTextPosition(caret.Position);
            }
            else if (key == Key.Right)
            {
                return document.GetNextTextPosition(caret.Position);
            }
            else if (key == Key.Down)
            {
                return document.GetDownLineTextPosition(caret.Position, caret.CaretRect);
            }
            else if (key == Key.Up)
            {
                return document.GetUpLineTextPosition(caret.Position, caret.CaretRect);
            }

            throw new ArgumentException("DirectionKey Only");
        }

        public static TextPosition Home(TextDocument document, TextPosition position)
        {
            var logicLine = document.FindLogicLine(position.Line);
            var textLine = logicLine.FindTextLine(position.Column, position.IsAtEndOfLine);
            var info = TextLineInfoManager.Get(textLine);
            return new TextPosition(position.Line, info.StartCharPos);
        }

        public static TextPosition End(TextDocument document, TextPosition position, bool isWrap)
        {
            if (position.IsAtEndOfLine)
            {
                return position;
            }

            var logicLine = document.FindLogicLine(position.Line);
            var textLine = logicLine.FindTextLine(position.Column, false);

            var info = TextLineInfoManager.Get(textLine);

            TextPosition pos;
            if (isWrap)
            {
                var col = info.EndCharPos + 1;
                col = col.Clamp(0, logicLine.GetLength() - 1);
                pos = new TextPosition(position.Line, col, col != logicLine.GetLength() - 1);
            }
            else
            {
                pos = new TextPosition(position.Line, info.EndCharPos, false);
            }

            return pos;
        }
    }
}
