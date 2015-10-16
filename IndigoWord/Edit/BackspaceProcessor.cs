using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IndigoWord.Core;
using IndigoWord.Render;

namespace IndigoWord.Edit
{
    /*
     * Key Backspace TextInputProcessor
     */
    class BackspaceProcessor : TextInputProcessor
    {
        private LogicLine _logicLine;
        private LogicLine _deletedLine;
        private int _lastCol = -1;
        private bool _needRender = true;

        public override void UpdateDocument(TextDocument document, TextPosition position, TextRange range, string text)
        {
            if (position.Equals(document.FirstPosition))
            {
                //already at the FirstPosition of the document, do nothing
                _needRender = false;
                return;
            }

            _logicLine = document.FindLogicLine(position.Line);
            if (position.Column == 0)
            {
                //combine this line to the previous line

                var previousLogicLine = document.FindLogicLine(position.Line - 1);

                _lastCol = previousLogicLine.GetLength() - 1;

                previousLogicLine.ClearNewLineChars();
                previousLogicLine.Text += _logicLine.Text;

                _deletedLine = _logicLine;
                _logicLine = previousLogicLine;

                var index = _deletedLine.Line;
                document.RemoveLines(index, 1);
            }
            else
            {
                //just delete single character
                _logicLine.Text = _logicLine.Text.Remove(position.Column - 1, 1);
            }
        }

        public override void Render(DocumentRender render)
        {
            if (!_needRender)
            {
                return;
            }

            if (_deletedLine != null)
            {
                //combine this line to the previous line

                render.Remove(_deletedLine);

                /*
                 *  we pass true for [isPositionBelowMandatory]
                 *  because we remove _deletedLine, so the lines below _logicLine need re-position
                 */
                render.Show(_logicLine, true);
            }
            else
            {
                render.Show(_logicLine, false);
            }
        }

        public override TextPosition CalcCaretPosition(TextDocument document, TextPosition position, TextRange range)
        {
            if (!_needRender)
            {
                return document.FirstPosition;
            }

            TextPosition pos;
            if (_deletedLine != null)
            {
                pos = new TextPosition(_logicLine.Line, _lastCol);
            }
            else
            {
                pos = document.GetPreviousTextPosition(position);
            }

            return pos;
        }

        public override void ResetCore()
        {
            _logicLine = null;
            _deletedLine = null;
            _lastCol = -1;
            _needRender = true;
        }
    }
}
