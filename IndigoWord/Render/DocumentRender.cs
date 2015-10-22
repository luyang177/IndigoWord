using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using IndigoWord.Annotations;
using IndigoWord.Behaviors;
using IndigoWord.Core;
using IndigoWord.LowFontApi;
using IndigoWord.Utility;

namespace IndigoWord.Render
{
    class DocumentRender : INotifyPropertyChanged
    {
        #region Constructor

        public DocumentRender(ILayer layer, WrapState wrapState)
        {
            if(layer == null)
                throw new ArgumentNullException("layer");

            Layer = layer;

            if (wrapState == null)
                throw new ArgumentNullException("wrapState");

            _wrapState = wrapState;
            _wrapState.Changed += OnWrapChanged;
        }

        #endregion

        #region Public Properties

        private FontRendering _fontRendering = new FontRendering();
        public FontRendering FontRendering
        {
            get { return _fontRendering; }
            set { _fontRendering = value; }
        }

        private Size _extent;

        public Size Extent
        {
            get { return _extent; }
            set
            {
                if (_extent == value)
                {
                    return;
                }

                _extent = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Public Methods

        public void Show(TextDocument document)
        {
            Debug.Assert(document != null);

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

        public void Show(LogicLine logicLine, bool isPositionBelowMandatory)
        {
            Debug.Assert(logicLine != null);
            Debug.Assert(Document != null);
            Debug.Assert(Document.Contains(logicLine));

            Render(logicLine);

            if (IsWrap || isPositionBelowMandatory)
            {
                //Position those LogicLines which below this given logicLine.
                PositionBelow(logicLine);
            }
        }

        public void Show(IList<LogicLine> logicLines)
        {
            Debug.Assert(logicLines != null);
            if (!logicLines.Any())
            {
                return;
            }

            Debug.Assert(Document != null);
            Debug.Assert(logicLines.All(line => Document.Contains(line)));

            foreach (var line in logicLines)
            {
                Render(line);
            }
            
            PositionBelow(logicLines.First());
        }

        public void Reset()
        {
            TextLineInfoManager.Clear();
            DrawingElements.Clear();
            Layer.Clear();
        }

        public void Remove(LogicLine logicLine)
        {
            Debug.Assert(logicLine != null);

            var drawingElement = DrawingElements.Single(element => element.Exist(logicLine));
            Layer.Remove(drawingElement.Visual);
            DrawingElements.Remove(drawingElement);
        }

        public TextPosition? Hit(VisualParam param)
        {
            Debug.Assert(DrawingElements.All(el => el.Visual != null));

            TextPosition? pos = null;
            if (param.Visual != null)
            {
                pos = FindHittedTextPosition(param);
            }

            if (param.Visual == null ||
                (param.Visual != null && pos == null))
            {
                /*
                 * (1) param.Visual is null, we just find the closest text postition
                 * (2) param.Visual is not null, but param.Visual is not on the TextLayer, so we also need to find the closest text postition
                 */
                pos = FindClosestTextPosition(param.Point);
            }

            return pos;
        }

        public void PositionBelow(LogicLine logicLine)
        {
            var startElement = DrawingElements.First(el => el.Exist(logicLine));
            var index = DrawingElements.IndexOf(startElement);
            var targetDrawingElements = DrawingElements.GetRange(index, DrawingElements.Count - index);

            double pos = logicLine.Top;
            foreach (var drawingElement in targetDrawingElements)
            {
                var visual = drawingElement.Visual;
                var transform = visual.Transform as TranslateTransform;
                if (transform == null || !transform.X.CloseTo(0) || !transform.Y.CloseTo(pos))
                {
                    visual.Transform = new TranslateTransform(0, pos);
                    visual.Transform.Freeze();
                }
                drawingElement.LogicLine.Top = pos;
                pos += drawingElement.Height;
            }

            //TODO width
            Extent = new Size(1000, pos);
        }

        #endregion

        #region Private Methods

        /*
         * Render LogicLine
         * Return the height of LogicLine
         */
        private double Render(LogicLine logicLine)
        {
            var drawingElement = DrawingElements.SingleOrDefault(element => element.Exist(logicLine));
            var height = drawingElement == null ? RenderNewLine(logicLine) : RenderOldLine(logicLine, drawingElement);
            return height;
        }

        private double RenderNewLine(LogicLine logicLine)
        {
            double height = 0;
            var newDrawingElement = new DrawingElement(logicLine);
            DrawingElements.Insert(logicLine.Line, newDrawingElement);
            using (var dc = newDrawingElement.Visual.RenderOpen())
            {
                var textLines = TextRender.Render(dc, logicLine, logicLine.Text, FontRendering, IsWrap);
                height = textLines.Sum(tl => tl.Height);
                newDrawingElement.Height = height;
                logicLine.AddTextLines(textLines);
            }

            Layer.Add(newDrawingElement.Visual);

            return height;
        }

        private double RenderOldLine(LogicLine logicLine, DrawingElement drawingElement)
        {
            Debug.Assert(drawingElement != null);

            logicLine.Dispose();
            TextLineInfoManager.Remove(logicLine);

            double height = 0;
            using (var dc = drawingElement.Visual.RenderOpen())
            {
                var textLines = TextRender.Render(dc, logicLine, logicLine.Text, FontRendering, IsWrap);
                height = textLines.Sum(tl => tl.Height);
                drawingElement.Height = height;
                logicLine.AddTextLines(textLines);
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

            //TODO width
            Extent = new Size(1000, pos);
        }

        private TextPosition? FindHittedTextPosition(VisualParam param)
        {
            Debug.Assert(param.Visual != null);

            //find hitted visual belongs to which LogicLine
            var hitDrawingElement = DrawingElements.SingleOrDefault(el => ReferenceEquals(el.Visual, param.Visual));
            if (hitDrawingElement == null)
            {
                //the given visual is not belong to TextLayer
                return null;
            }

            var logicLine = hitDrawingElement.LogicLine;
            Debug.Assert(logicLine != null);

            //find hitted visual belongs to which TextLine
            var textLine = logicLine.FindTextLine(param.Point);
            Debug.Assert(textLine != null);

            var col = textLine.FindClosestColumn(param.Point, true);
            var info = TextLineInfoManager.Get(textLine);
            var isAtEndOfLine = col == info.EndCharPos + 1;
            return new TextPosition(logicLine.Line, col, isAtEndOfLine);
        }

        private TextPosition FindClosestTextPosition(Point point)
        {
            //find point belongs to which LogicLine by height
            var logicLine = Document.FindLogicLine(point);
            Debug.Assert(logicLine != null);
 
            //find point belongs to which TextLine by height
            var textLine = logicLine.FindTextLine(point);
            Debug.Assert(textLine != null);

            var info = TextLineInfoManager.Get(textLine);
            if (point.X > textLine.WidthIncludingTrailingWhitespace)
            {
                //directly return the last position in this TextLine
                return new TextPosition(logicLine.Line, 
                    info.IsLast ? info.EndCharPos : info.EndCharPos + 1,
                    !info.IsLast);
            }
            else
            {
                var col = textLine.FindClosestColumn(point, true);
                var isAtEndOfLine = col == info.EndCharPos + 1;
                return new TextPosition(logicLine.Line, col, isAtEndOfLine);
            }
        }

        private void OnWrapChanged()
        {
            Document.Dispose();
            TextFormatterFactory.Reset();
            Reset();
            Show(Document);
        }

        #endregion

        #region Private Properties and Fields

        private ILayer Layer { get; set; }

        private TextDocument Document { get; set; }

        private readonly List<DrawingElement> _drawingElements = new List<DrawingElement>();
        private List<DrawingElement> DrawingElements
        {
            get { return _drawingElements; }
        }

        private readonly WrapState _wrapState;

        private WrapState WrapState
        {
            get { return _wrapState; }
        }

        private bool IsWrap
        {
            get { return WrapState.IsWrap; }
        }



        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
