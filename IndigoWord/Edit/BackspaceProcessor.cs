using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IndigoWord.Core;
using IndigoWord.Render;

namespace IndigoWord.Edit
{
    class BackspaceProcessor : TextInputProcessor
    {
        private LogicLine _logicLine;
        private LogicLine _deletedLine;
        private int _lastCol = -1;
        private bool _needRender = true;

        protected override void AddText(TextDocument document, TextPosition position, string text)
        {
            _logicLine = document.FindLogicLine(position.Line);
            if (position.Column == 0)
            {
                if (position.Line == 0)
                {
                    //already at the TextPosition(0, 0), do nothing
                    _needRender = false;
                    return;
                }
                else
                {
                    //combine this line to the previous line

                    var previousLogicLine = document.FindLogicLine(position.Line - 1);

                    if (previousLogicLine.Text.EndsWith("\r\n"))
                    {
                        _lastCol = previousLogicLine.Text.Length - 2;
                    }
                    else
                    {
                        _lastCol = previousLogicLine.Text.Length - 1;
                    }

                    previousLogicLine.Text = previousLogicLine.Text.Replace(Environment.NewLine, "");
                    previousLogicLine.Text += _logicLine.Text;

                    _deletedLine = _logicLine;
                    _logicLine = previousLogicLine;

                    var index = _deletedLine.Line;
                    document.RemoveLines(index, 1);
                }
            }
            else
            {
                //just delete single character
                _logicLine.Text = _logicLine.Text.Remove(position.Column - 1, 1);
            }
        }

        protected override void Render(DocumentRender render)
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

        protected override TextPosition CalcCaretPosition(TextDocument document, TextPosition position)
        {
            if (!_needRender)
            {
                return position;
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

        protected override void ResetCore()
        {
            _logicLine = null;
            _deletedLine = null;
            _lastCol = -1;
            _needRender = true;
        }
    }
}
