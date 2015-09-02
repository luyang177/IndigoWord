using System;
using System.IO;
using System.Resources;
using System.Windows.Documents;
using IndigoWord.Core;
using IndigoWord.Render;

namespace IndigoWord.Edit
{
    class TextInputProcessorParam
    {
        public TextDocument Document { get; set; }

        public DocumentRender Render { get; set; }

        public TextPosition Position { get; set; }

        public string Text { get; set; }
    }

    /*
     * TODO Add assert or unit test for abstract methods
     */
    abstract class TextInputProcessor
    {
        #region Public Methods

        /*
         * return the new Caret Position.
         */
        public TextPosition Process(TextInputProcessorParam param)
        {
            CheckParam(param);

            AddText(param.Document, param.Position, param.Text);
            Render(param.Render);
            var caretPos = CalcCaretPosition(param.Document, param.Position);

            return caretPos;
        }

        public void Reset()
        {

            ResetCore();
        }

        #endregion

        #region Abstract Methods

        protected abstract void AddText(TextDocument document, TextPosition position, string text);

        protected abstract void Render(DocumentRender render);

        protected abstract TextPosition CalcCaretPosition(TextDocument document, TextPosition position);

        protected abstract void ResetCore();

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

            if (param.Position == null)
            {
                throw new ArgumentNullException("param.Position");
            }
        }

        #endregion
    }
}
