using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using IndigoWord.Core;
using IndigoWord.LowFontApi;
using IndigoWord.Operation.Behaviors;
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

        private FontRendering _fontRendering = new FontRendering();
        public FontRendering FontRendering
        {
            get { return _fontRendering; }
            set { _fontRendering = value; }
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

        //TODO
        public Caret Caret { get; set; }

        public TextPosition Hit(HitVisualParam param)
        {
            Debug.Assert(DrawingElements.All(el => el.Visual != null));

            if (param.Visual != null)
            {
                return FindHittedTextPosition(param);
            }
            else
            {
                //since param.Visual is null, we just find the closest text postition
                return FindClosestTextPosition(param.Position);
            }
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
                    var textLines = TextRender.Render(dc, logicLine.Text, FontRendering, IsWrap);
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

        private TextPosition FindHittedTextPosition(HitVisualParam param)
        {
            Debug.Assert(param.Visual != null);

            if (Caret.IsCaret(param.Visual))
            {
                return Caret.Position;
            }

            //find hitted visual belongs to which LogicLine
            var hitDrawingElement = DrawingElements.Single(el => el.Visual == param.Visual);
            var logicLine = hitDrawingElement.LogicLine;
            Debug.Assert(logicLine != null);

            //find hitted visual belongs to which TextLine
            var textLine = logicLine.FindTextLine(param.Position);
            Debug.Assert(textLine != null);

            var col = textLine.FindClosestColumn(param.Position);
            return new TextPosition(logicLine.Line, col);            
        }

        private TextPosition FindClosestTextPosition(Point point)
        {
            //find point belongs to which LogicLine by height
            var logicLine = Document.FindLogicLine(point);
            Debug.Assert(logicLine != null);
 
            //find point belongs to which TextLine by height
            var textLine = logicLine.FindTextLine(point);
            Debug.Assert(textLine != null);

            if (point.X > textLine.WidthIncludingTrailingWhitespace)
            {
                //directly return the last position in this TextLine

                var info = TextLineInfoManager.Get(textLine);

                /*
                 * "info.IsLast ? info.EndCharPos : info.EndCharPos + 1"
                 * is used for selection multiple lines by draging mouse
                 * like notepad++
                 */
                return new TextPosition(logicLine.Line, info.IsLast ? info.EndCharPos : info.EndCharPos + 1);
            }
            else
            {
                var col = textLine.FindClosestColumn(point);
                return new TextPosition(logicLine.Line, col);
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
