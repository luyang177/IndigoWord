using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using IndigoWord.Core;
using IndigoWord.LowFontApi;
using IndigoWord.Utility;

namespace IndigoWord.Render
{
    class DocumentRender
    {

        #region Constructor

        public DocumentRender(ILayer layer)
        {
            if(layer == null)
                throw new ArgumentNullException("layer");

            Layer = layer;
        }

        #endregion

        #region Public Properties

        private bool _isWrap;
        public bool IsWrap
        {
            get { return _isWrap; }
            set
            {
                _isWrap = value;

                Document.Reset();
                Reset();
                Show(Document);
            }
        }

        #endregion

        #region Public Methods

        public void Show(TextDocument document)
        {
            double pos = 0;
            foreach (var logicLine in document.Lines)
            {
                logicLine.Top = pos;
                var h = Render(logicLine);
                pos += h;
            }

            Position();

            Document = document;
        }

        public void Reset()
        {
            TextLineInfoManager.Clear();
            DrawingElements.Clear();
            Layer.Clear();
        }

        #endregion

        #region Private Methods

        /*
         * Render LogicLine
         * Return the height of LogicLine
         */
        private double Render(LogicLine logicLine)
        {
            double height = 0;
            var drawingElement = DrawingElements.SingleOrDefault(element => element.Exist(logicLine));
            if (drawingElement == null)
            {
                var newDrawingElement = new DrawingElement(logicLine);
                DrawingElements.Add(newDrawingElement);
                using (var dc = newDrawingElement.Visual.RenderOpen())
                {
                    var textLines = TextRender.Render(dc, logicLine.Text, IsWrap);
                    height = textLines.Sum(tl => tl.Height);
                    newDrawingElement.Height = height;
                    logicLine.AddTextLines(textLines);
                }

                Layer.Add(newDrawingElement.Visual);
            }

            return height;
        }

        private void Position()
        {
            double pos = 0;
            foreach (var drawingElement in DrawingElements)
            {
                var visual = drawingElement.Visual;
                var transform = visual.Transform as TranslateTransform;
                if (transform == null || !transform.X.CloseTo(0) || !transform.Y.CloseTo(pos))
                {
                    visual.Transform = new TranslateTransform(0, pos);
                    visual.Transform.Freeze();
                }
                pos += drawingElement.Height;
            }
        }



        #endregion

        #region Private Properties and Fields

        private ILayer Layer { get; set; }

        private TextDocument Document { get; set; }

        private List<DrawingElement> _drawingElements = new List<DrawingElement>();
        public List<DrawingElement> DrawingElements
        {
            get { return _drawingElements; }
            set { _drawingElements = value; }
        }

        #endregion
    }
}
