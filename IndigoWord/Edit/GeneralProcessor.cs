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

        public override void UpdateDocument(TextDocument document, TextPosition position, TextRange range, string text)
        {
            _logicLine = document.FindLogicLine(position.Line);
            _logicLine.Text = _logicLine.Text.Insert(position.Column, text);
        }

        public override void Render(DocumentRender render)
        {
            render.Show(_logicLine, false);
        }

        public override TextPosition CalcCaretPosition(TextDocument document, TextPosition position, TextRange range)
        {
            return document.GetNextTextPosition(position);
        }

        public override void ResetCore()
        {
            _logicLine = null;
        }
    }
}
