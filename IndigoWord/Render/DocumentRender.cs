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

        public void Hit(HitVisualParam param)
        {
            Debug.Assert(param.Visual != null);
            Debug.Assert(DrawingElements.All(el => el.Visual != null));

            var hitDrawingElement = DrawingElements.Single(el => el.Visual == param.Visual);
            var logicLine = hitDrawingElement.LogicLine;

            var neareatItem = logicLine.TextLines.SelectMany(tl =>
            {
                var info = TextLineInfoManager.Get(tl);
                var top = logicLine.Top + info.Top;
                return Enumerable.Range(0, tl.Length)
                    .Select(n =>
                    {
                        var pos = info.StartCharPos + n;
                        var bound = tl.GetTextBounds(pos, 1)[0];
                        var rc = new Rect(bound.Rectangle.Left,
                                          top,
                                          bound.Rectangle.Width,
                                          bound.Rectangle.Height);
                        return new
                        {
                            Rect = rc,
                            Column = pos
                        };
                    });
            })
            .OrderBy(obj => MathHelper.Distance(param.Position, obj.Rect.Center()))
            .First();


            Caret.Position = new TextPosition(logicLine.Line, neareatItem.Column);

            //TODO update column info in status bar
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
