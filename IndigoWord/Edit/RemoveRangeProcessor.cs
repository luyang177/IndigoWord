using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IndigoWord.Core;
using IndigoWord.Render;

namespace IndigoWord.Edit
{
    class RemoveRangeProcessor : TextInputProcessor
    {
        private LogicLine _renderLine;
        private readonly List<LogicLine> _deleteLines = new List<LogicLine>();

        public override void UpdateDocument(TextDocument document, TextPosition position, TextRange range, string text)
        {
            foreach (var line in range.Lines)
            {
                var logicLine = document.FindLogicLine(line);
                UpdateLine(range, line, logicLine);
            }

            if (_deleteLines.Any())
            {
                var firstLine = _deleteLines.First();
                document.RemoveLines(firstLine.Line, _deleteLines.Count);
            }
        }

        private void UpdateLine(TextRange range, int line, LogicLine logicLine)
        {
            if (line == range.Start.Line)
            {
                if (line == range.End.Line)
                {
                    //selection in single line

                    var size = range.End.Column - range.Start.Column;
                    logicLine.Text = logicLine.Text.Remove(range.Start.Column, size);
                }
                else
                {
                    var size = logicLine.Text.Length - range.Start.Column;
                    logicLine.Text = logicLine.Text.Remove(range.Start.Column, size);
                }

                //_renderLine is always the first LogicLine
                _renderLine = logicLine;
            }
            else if (line == range.End.Line)
            {
                var size = range.End.Column;
                logicLine.Text = logicLine.Text.Remove(0, size);
                _renderLine.Text += logicLine.Text;

                _deleteLines.Add(logicLine);
            }
            else
            {
                _deleteLines.Add(logicLine);
            }
        }


        public override void Render(DocumentRender render)
        {
            foreach (var deleteLine in _deleteLines)
            {
                render.Remove(deleteLine);
            }

            render.Show(_renderLine, _deleteLines.Any());
        }

        public override TextPosition CalcCaretPosition(TextDocument document, TextPosition position, TextRange range)
        {
            Debug.Assert(range.IsInRange(position));

            //remember isAtEndOfLine set false, because after remove range, the Start posotion is not still the end of the line.
            var pos = new TextPosition(range.Start.Line, range.Start.Column, false);
            return pos;
        }

        public override void ResetCore()
        {
            _renderLine = null;
            _deleteLines.Clear();
        }
    }
}