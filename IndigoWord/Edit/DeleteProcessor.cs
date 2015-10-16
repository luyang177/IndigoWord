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
     * Key Delete TextInputProcessor
     */
    class DeleteProcessor : TextInputProcessor
    {
        private LogicLine _logicLine;
        private LogicLine _deletedLine;
        private bool _needRender = true;

        public override void UpdateDocument(TextDocument document, TextPosition position, TextRange range, string text)
        {
            if (position.Equals(document.LastPosition))
            {
                //already at the LastPosition of the document, do nothing
                _needRender = false;
                return;
            }

            _logicLine = document.FindLogicLine(position.Line);

            if (position.Column == _logicLine.GetLength() - 1)
            {
                var nextLineIndex = position.Line + 1;
                var nextLine = document.FindLogicLine(nextLineIndex);
                _logicLine.ClearNewLineChars();
                _logicLine.Text += nextLine.Text;

                document.RemoveLines(nextLineIndex, 1);

                _deletedLine = nextLine;
            }
            else
            {
                //just delete single character
                _logicLine.Text = _logicLine.Text.Remove(position.Column, 1);
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
                return document.LastPosition;
            }

            return new TextPosition(position.Line, position.Column, false);
        }

        public override void ResetCore()
        {
            _logicLine = null;
            _deletedLine = null;
            _needRender = true;
        }
    }
}
