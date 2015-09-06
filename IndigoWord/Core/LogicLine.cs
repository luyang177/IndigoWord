using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media.TextFormatting;

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

            return position.Column >= 0 && position.Column < length;
        }

        /*
         * return the Text actual size(exclude \r\n) and a more last position
         * for example:
         *              abc\r\n => 4
         *              4\r\n => 2
         *              \r\n => 1
         */
        public int GetLength()
        {
            CheckTextEnd(Text);

            var length = Text.Length;

            //since LogicLine end with \r\n
            length--;
            
            return length;
        }


        public double GetDistanceFromColumn(int column, bool isAtEndOfLine)
        {           
            var textLine = FindTextLine(column, isAtEndOfLine);
            double xPos = textLine.GetDistanceFromCharacterHit(new CharacterHit(column, 0));

            if (isAtEndOfLine)
            {
                /*
                 * When mouse hit the last position of a TextLine, we will display the trailing whitespace if it has
                 * however, when input key down, up, left and right, we will [not] display the trailing whitespace
                 */
                var whiteSpace = textLine.WidthIncludingTrailingWhitespace - textLine.Width;
                xPos += whiteSpace;
            }

            return xPos;
        }

        public double GetTop(int column, bool isAtEndOfLine)
        {
            var textLine = FindTextLine(column, isAtEndOfLine);
            var info = TextLineInfoManager.Get(textLine);
            return Top + info.Top;
        }

        public double GetBottom(int column, bool isAtEndOfLine)
        {
            var textLine = FindTextLine(column, isAtEndOfLine);
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

        public TextLine FindTextLine(int column, bool isAtEndOfLine)
        {
            if (column < 0)
                throw new ArgumentNullException("column < 0");

            if (column >= GetLength())
                throw new ArgumentException("column >= GetLength()");

            int line = column;
            TextLine lastTextLine = null;
            foreach (var textLine in TextLines)
            {
                if (line < textLine.Length)
                {
                    if (isAtEndOfLine && lastTextLine != null)
                    {
                        return lastTextLine;
                    }

                    return textLine;
                }
                else
                {
                    line -= textLine.Length;
                    lastTextLine = textLine;
                }
            }

            throw new ArgumentException("TextLine not find");
        }

        public TextLine FindTextLine(Point point)
        {
            var lastLine = TextLines.Last();
            var lastInfo = TextLineInfoManager.Get(lastLine);
            if (point.Y > Top + lastInfo.Top + lastLine.Height)
            {
                return lastLine;
            }

            return TextLines.SingleOrDefault(tl =>
            {
                var info = TextLineInfoManager.Get(tl);
                var top = Top + info.Top;
                return (point.Y >= top) && (point.Y <= top + tl.Height);
            });            
        }

        /*
         * Split this logicLine by /r, /n and /r/n
         * return IList<LogicLine> which the first element is always this logicLine
         */
        public IList<LogicLine> Split()
        {
            //because once OpenDocument, we add Environment.NewLine to each LogicLine
            Debug.Assert(Text.EndsWith(Environment.NewLine));

            var text = Text;
            text = text.Replace("\r\n", "\r");
            var splits = text.Split(new[] { '\r', '\n' }).ToList();

            //the last element is "" since each LogicLine end with Environment.NewLine
            splits.RemoveAt(splits.Count - 1);

            var list = new List<LogicLine>
            {
                this
            };
            Text = splits.First();

            int lineNumber = Line + 1;
            for (int n = 1; n < splits.Count; n++)
            {
                var line = new LogicLine(lineNumber, splits[n]);
                list.Add(line);
                lineNumber++;
            }

            //the first element is always the given logicLine         
            Debug.Assert(list.Count >= 1);

            foreach (var line in list)
            {
                line.Text += Environment.NewLine;
            }

            return list;
        }

        public void ClearNewLineChars()
        {
            CheckTextEnd(Text);
            Text = Text.Replace(Environment.NewLine, "");
        }


        #endregion

        #region Private Methods

        private void CheckTextEnd(string text)
        {
            if (!text.EndsWith("\r\n"))
            {
                throw new Exception("LogicLine must end with \r\n");
            }
        }

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