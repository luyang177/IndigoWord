using System;
using System.IO;
using IndigoWord.Core;
using IndigoWord.Render;

namespace IndigoWord.Edit
{
    class TextInputProcessorParam
    {
        public TextDocument Document { get; set; }

        public DocumentRender Render { get; set; }

        public TextPosition Position { get; set; }

        public TextRange TextRange { get; set; }

        public string Text { get; set; }        
    }

    abstract class TextInputProcessor
    {
        #region Public Methods

        /*
         * return the new Caret Position.
         */
        public TextPosition Process(TextInputProcessorParam param)
        {
            CheckParam(param);

            UpdateDocument(param.Document, param.Position, param.TextRange, param.Text);
            Render(param.Render);
            var caretPos = CalcCaretPosition(param.Document, param.Position, param.TextRange);

            return caretPos;
        }

        public void Reset()
        {
            ResetCore();
        }

        #endregion

        #region Abstract Methods

        public abstract void UpdateDocument(TextDocument document, TextPosition position, TextRange range, string text);

        public abstract void Render(DocumentRender render);

        public abstract TextPosition CalcCaretPosition(TextDocument document, TextPosition position, TextRange range);

        public abstract void ResetCore();

        #endregion

        #region Private Methods

        private void CheckParam(TextInputProcessorParam param)
        {
            if (param == null)
            {
                throw new ArgumentNullException("param");
            }

            if (param.Document == null)
            {
                throw new ArgumentNullException("param.Document");
            }

            if (param.Render == null)
            {
                throw new ArgumentNullException("param.Render");
            }
        }

        #endregion
    }
}
