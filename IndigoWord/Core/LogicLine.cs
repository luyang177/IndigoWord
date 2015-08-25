using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.TextFormatting;
using IndigoWord.Render;

namespace IndigoWord.Core
{
    class LogicLine : IDisposable
    {
        #region Constructor

        public LogicLine(int line, string text)
        {
            Line = line;
            Text = text;
        }

        #endregion

        #region Properties

        public int Line { get; set; }

        public string Text { get; set; }

        private readonly List<TextLine> _textLines = new List<TextLine>();
        public List<TextLine> TextLines
        {
            get { return _textLines; }
        }

        //the top of its LogicLine, relative to the start of the document.
        public double Top { get; set; }

        public double Bottom
        {
            get { return Top + TextLines.Sum(tl => tl.Height); }
        }

        #endregion

        #region Public Methods

        public bool Exist(TextPosition position)
        {
            if (position.Line != Line)
            {
                return false;
            }

            var length = GetLength();

            return position.Column >= 0 &&
                   position.Column < length;
        }

        public int GetLength()
        {
            //since the last text line has more 1 pos, we -1
            var length = TextLines.Sum(tl => tl.Length) - 1;
            return length;
        }

        public double GetXPosition(int column)
        {
            var textLine = GetTextLine(column);
            double xPos = textLine.GetDistanceFromCharacterHit(new CharacterHit(column, 0));
            return xPos;
        }

        public double GetTop(int column)
        {
            var textLine = GetTextLine(column);
            var info = TextLineInfoManager.Get(textLine);
            return Top + info.Top;
        }

        public double GetBottom(int column)
        {
            var textLine = GetTextLine(column);
            var info = TextLineInfoManager.Get(textLine);
            return Top + info.Top + textLine.Height;
        }

        public void AddTextLines(IEnumerable<TextLine> lines)
        {
            TextLines.AddRange(lines);
        }

        public void RemoveTextLines()
        {
            TextLines.Clear();
        }

        public TextLine GetTextLine(int column)
        {
            if (column < 0)
                throw new ArgumentNullException("column < 0");

            if (column > GetLength())
                throw new ArgumentException("column > GetLength()");

            int line = column;
            foreach (var textLine in TextLines)
            {
                if (line < textLine.Length)
                {
                    return textLine;
                }
                else
                {
                    line -= textLine.Length;
                }
            }

            throw new ArgumentException("TextLine not find");
        }

        public TextLine GetNextTextLine(TextLine textLine)
        {
            var index = TextLines.IndexOf(textLine);

            var nextIndex = index + 1;
            return nextIndex < TextLines.Count ? TextLines[nextIndex] : null;
        }

        public TextLine GetPreviousTextLine(TextLine textLine)
        {
            var index = TextLines.IndexOf(textLine);

            var nextIndex = index - 1;
            return nextIndex >= 0 && nextIndex < TextLines.Count
                   ? TextLines[nextIndex] : null;
        }

        public TextLine FindTextLine(Point point)
        {
            return TextLines.SingleOrDefault(tl =>
            {
                var info = TextLineInfoManager.Get(tl);
                var top = Top + info.Top;
                return (point.Y >= top) && (point.Y <= top + tl.Height);
            });            
        }

        #endregion

        #region Private Methods



        #endregion

        #region Private Properties and Fields

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            TextLines.ForEach(tl => tl.Dispose());
        }

        #endregion
    }
}