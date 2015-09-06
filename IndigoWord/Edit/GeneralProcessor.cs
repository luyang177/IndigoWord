using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IndigoWord.Core;
using IndigoWord.Render;

namespace IndigoWord.Edit
{
    class GeneralProcessor : TextInputProcessor
    {
        private LogicLine _logicLine;

        protected override void UpdateDocument(TextDocument document, TextPosition position, string text)
        {
            _logicLine = document.FindLogicLine(position.Line);
            _logicLine.Text = _logicLine.Text.Insert(position.Column, text);
        }

        protected override void Render(DocumentRender render)
        {
            render.Show(_logicLine, false);
        }

        protected override TextPosition CalcCaretPosition(TextDocument document, Core.TextPosition position)
        {
            return document.GetNextTextPosition(position);
        }

        protected override void ResetCore()
        {
            _logicLine = null;
        }
    }
}
