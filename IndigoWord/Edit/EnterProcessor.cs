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

        public override void UpdateDocument(TextDocument document, TextPosition position, TextRange range, string text)
        {
            _logicLine = document.FindLogicLine(position.Line);
            _logicLine.Text = _logicLine.Text.Insert(position.Column, text);

            _renderLines = _logicLine.Split();
            document.InsertLines(_logicLine.Line + 1, _renderLines.Skip(1).ToList());
        }

        public override void Render(DocumentRender render)
        {
            render.Show(_renderLines);
        }

        public override TextPosition CalcCaretPosition(TextDocument document, TextPosition position, TextRange range)
        {
            return document.GetNextTextPosition(position);
        }

        public override void ResetCore()
        {
            _logicLine = null;
            _renderLines = null;
        }
    }
}
