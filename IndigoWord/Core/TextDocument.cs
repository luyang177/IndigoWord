using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media.TextFormatting;
using IndigoWord.Utility;

namespace IndigoWord.Core
{
    class TextDocument
    {
        #region Constructor

        public TextDocument(IEnumerable<LogicLine> lines)
        {
            Debug.Assert(_lines.Count == 0);

            foreach (var line in lines)
            {
                AddLogicLine(line);
            }

            Debug.Assert(_lines.Count != 0);
        }

        public static TextDocument Empty()
        {
            var doc = new TextDocument();

            Debug.Assert(doc._lines.Count == 0);

            var line = new LogicLine(0, Environment.NewLine);
            doc.AddLogicLine(line);

            Debug.Assert(doc._lines.Count == 1);

            return doc;
        }

        private TextDocument()
        {
            
        }

        #endregion

        #region Public Properties

        public IList<LogicLine> Lines
        {
            get { return _lines.Select(li => li.Value).ToList(); }
        }

        #endregion

        #region Public Methods

        public bool ExistLine(int line)
        {
            return _lines.ContainsKey(line);
        }

        public LogicLine GetLogicLine(int line)
        {
            return _lines[line];
        }

        public void Reset()
        {
            foreach (var line in Lines)
            {
                line.RemoveTextLines();
            }
        }

        /*
         * Guarantee the return TextPosition is always valid.
         */
        public TextPosition GetPreviousTextPosition(TextPosition position)
        {
            var previous = DoGetPreviousTextPosition(position);
            Debug.Assert(VerifyTextPosition(previous));
            return previous;
        }

        /*
         * Guarantee the return TextPosition is always valid.
         */
        public TextPosition GetNextTextPosition(TextPosition position)
        {
            var next = DoGetNextTextPosition(position);
            Debug.Assert(VerifyTextPosition(next));
            return next;
        }

        /*
         * Guarantee the return TextPosition is always valid.
         */
        public TextPosition GetDownLineTextPosition(TextPosition position, Rect caretRect)
        {
            var pos = DoGetVerticalMoveTextPosition(position, caretRect, PositionMoveType.Down);
            Debug.Assert(VerifyTextPosition(pos));
            return pos;
        }

        /*
         * Guarantee the return TextPosition is always valid.
         */
        public TextPosition GetUpLineTextPosition(TextPosition position, Rect caretRect)
        {
            var pos = DoGetVerticalMoveTextPosition(position, caretRect, PositionMoveType.Up);
            Debug.Assert(VerifyTextPosition(pos));
            return pos;
        }

        public bool VerifyTextPosition(TextPosition position)
        {
            if (!ExistLine(position.Line))
                return false;

            var logicLine = GetLogicLine(position.Line);
            return logicLine.Exist(position);
        }

        #endregion

        #region Private Methods

        private void AddLogicLine(LogicLine line)
        {
            _lines.Add(line.Line, line);
        }

        private TextPosition DoGetPreviousTextPosition(TextPosition position)
        {
            if (!VerifyTextPosition(position))
            {
                var firstLine = Lines.First();
                return new TextPosition(firstLine.Line, 0);
            }

            var line = position.Line;
            var previous = new TextPosition(line, position.Column - 1);

            var logicLine = GetLogicLine(line);
            if (logicLine.Exist(previous))
            {
                return previous;
            }
            else
            {
                var newLine = line - 1;
                if (ExistLine(newLine))
                {
                    //jump to previous line
                    var previousLogicLine = GetLogicLine(newLine);
                    return new TextPosition(newLine, previousLogicLine.GetLength() - 1);
                }
                else
                {
                    //since position is already the first line and the first column
                    return position;
                }
            }
        }

        private TextPosition DoGetNextTextPosition(TextPosition position)
        {
            if (!VerifyTextPosition(position))
            {
                var lastLine = Lines.Last();
                return new TextPosition(lastLine.Line, lastLine.GetLength() - 1);
            }

            var line = position.Line;
            var next = new TextPosition(line, position.Column + 1);

            var logicLine = GetLogicLine(line);
            if (logicLine.Exist(next))
            {
                return next;
            }
            else
            {
                var newLine = line + 1;
                if (ExistLine(newLine))
                {
                    //jump to next line
                    return new TextPosition(newLine, 0);
                }
                else
                {
                    //since position is already the last line and the last column
                    return position;
                }
            }
        }

        private TextPosition DoGetVerticalMoveTextPosition(TextPosition position, Rect caretRect, PositionMoveType type)
        {
            var currentLogicLine = GetLogicLine(position.Line);
            var currentTextLine = currentLogicLine.GetTextLine(position.Column);

            TextLine nextTextLine;
            if (type == PositionMoveType.Down)
            {
                nextTextLine = currentLogicLine.GetNextTextLine(currentTextLine);
            }
            else if (type == PositionMoveType.Up)
            {
                nextTextLine = currentLogicLine.GetPreviousTextLine(currentTextLine);
            }
            else
            {
                throw new ArgumentException("PositionMoveType");
            }

            int nextLine = position.Line;
            if (nextTextLine == null)
            {
                if (type == PositionMoveType.Down)
                {
                    nextLine = position.Line + 1;
                }
                else if (type == PositionMoveType.Up)
                {
                    nextLine = position.Line - 1;
                }

                if (!ExistLine(nextLine))
                {
                    //already first or last text line
                    return position;
                }

                var downLogicLine = GetLogicLine(nextLine);

                if (type == PositionMoveType.Down)
                {
                    nextTextLine = downLogicLine.TextLines.First();
                }
                else if (type == PositionMoveType.Up)
                {
                    nextTextLine = downLogicLine.TextLines.Last();
                }
                
            }

            var col = nextTextLine.FindNearestColumn(caretRect);
            return new TextPosition(nextLine, col);
        }

        #endregion

        #region Private Properties And Fields

        private readonly Dictionary<int, LogicLine> _lines = new Dictionary<int, LogicLine>();

        #endregion        
    }
}
