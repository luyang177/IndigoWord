using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IndigoWord.Core;
using IndigoWord.Render;

namespace IndigoWord.Edit
{
    class GeneralWithRangeProcessor : TextInputProcessor
    {
        private readonly GeneralProcessor _generalProcessor = new GeneralProcessor();
        private readonly RemoveRangeProcessor _removeRangeProcessor = new RemoveRangeProcessor();
        private TextPosition _positionAfterRemove;

        public override void UpdateDocument(TextDocument document, TextPosition position, TextRange range, string text)
        {
            _positionAfterRemove = _removeRangeProcessor.CalcCaretPosition(document, position, range);

            _removeRangeProcessor.UpdateDocument(document, position, range, text);
            _generalProcessor.UpdateDocument(document, _positionAfterRemove, range, text);
        }

        public override void Render(DocumentRender render)
        {
            _removeRangeProcessor.Render(render);
            _generalProcessor.Render(render);
        }

        public override TextPosition CalcCaretPosition(TextDocument document, TextPosition position, TextRange range)
        {
            return _generalProcessor.CalcCaretPosition(document, _positionAfterRemove, range);
        }

        public override void ResetCore()
        {
            _removeRangeProcessor.Reset();
            _generalProcessor.Reset();
        }
    }
}
