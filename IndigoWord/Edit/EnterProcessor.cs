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
     * Key Enter TextInputProcessor
     */
    class EnterProcessor : TextInputProcessor
    {
        private LogicLine _logicLine;

        private IList<LogicLine> _renderLines; 

        protected override void UpdateDocument(TextDocument document, TextPosition position, string text)
        {
            _logicLine = document.FindLogicLine(position.Line);
            _logicLine.Text = _logicLine.Text.Insert(position.Column, text);

            _renderLines = _logicLine.Split();
            document.InsertLines(_logicLine.Line + 1, _renderLines.Skip(1).ToList());
        }

        protected override void Render(DocumentRender render)
        {
            render.Show(_renderLines);
        }

        protected override TextPosition CalcCaretPosition(TextDocument document, Core.TextPosition position)
        {
            return document.GetNextTextPosition(position);
        }

        protected override void ResetCore()
        {
            _logicLine = null;
            _renderLines = null;
        }
    }
}
