using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using IndigoWord.Core;

namespace IndigoWord.Render
{
    class SelectionRender
    {
        #region Constructor

        public SelectionRender(ILayerProvider layerProvider, WrapState wrapState)
        {
            if (layerProvider == null)
                throw new ArgumentNullException("layerProvider");

            Init(layerProvider);

            if (wrapState == null)
                throw new ArgumentNullException("wrapState");

            _wrapState = wrapState;
        }

        #endregion    
    
        #region Public Methods

        public void Show(TextDocument document, TextRange range)
        {
            CheckTextRange(document, range);
            var rects = GenerateSelectRects(document, range);
            RenderRects(rects);
        }

        public void Clear()
        {
            using (var dc = Visual.RenderOpen())
            {

            }            
        }

        #endregion

        #region Private Methods

        private void Init(ILayerProvider layerProvider)
        {
            var layer =  layerProvider.Get(LayerNames.BackgroundLayer);

            Visual = new DrawingVisual();
            layer.Add(Visual);
        }

        private void CheckTextRange(TextDocument document, TextRange range)
        {
            if (!document.VerifyTextPosition(range.Start) || !document.VerifyTextPosition(range.End))
            {
                throw new Exception("range not in document");
            }
        }

        private IList<Rect> GenerateSelectRects(TextDocument document, TextRange range)
        {
            var totalRects = new List<Rect>();

            foreach (var line in range.Lines)
            {
                var logicLine = document.FindLogicLine(line);

                if (line == range.Start.Line)
                {
                    //First Line

                    if (range.Start.Line == range.End.Line)
                    {
                        //Select only single LogicLine

                        var lineRects = HandleLogicLine(logicLine, range.Start.Column, range.End.Column, true);
                        totalRects.AddRange(lineRects);
                    }
                    else
                    {
                        var lineRects = HandleLogicLine(logicLine, range.Start.Column, logicLine.GetLength() - 1, false);
                        totalRects.AddRange(lineRects);
                    }
                }
                else if (line == range.End.Line)
                {
                    //Last line

                    var lineRects = HandleLogicLine(logicLine, 0, range.End.Column, true);
                    totalRects.AddRange(lineRects);
                }
                else
                {
                    //Middle lines

                    var lineRects = HandleLogicLine(logicLine, 0, logicLine.GetLength() - 1, false);
                    totalRects.AddRange(lineRects);
                }
            }
            return totalRects;
        }

        /*
         * param:
         *  isLast - means the last LogicLine
         */
        private IList<Rect> HandleLogicLine(LogicLine logicLine, int startCol, int endCol, bool isLast)
        {
            var startTextLine = logicLine.FindTextLine(startCol, false);
            var startIndex = logicLine.TextLines.IndexOf(startTextLine);
            var endTextLine = logicLine.FindTextLine(endCol, false);
            var endIndex = logicLine.TextLines.IndexOf(endTextLine);

            var rects = new List<Rect>();

            for (int i = startIndex; i <= endIndex; i++)
            {
                var textLine = logicLine.TextLines[i];
                var info = TextLineInfoManager.Get(textLine);
                Rect? rc;
                if (i == startIndex)
                {
                    if (startIndex == endIndex)
                    {
                        rc = HandleTextLine(textLine, info, startCol, endCol, true, isLast);
                    }
                    else
                    {
                        rc = HandleTextLine(textLine, info, startCol, info.EndCharPos, false, false);
                    }
                }
                else if (i == endIndex)
                {
                    rc = HandleTextLine(textLine, info, info.StartCharPos, endCol, true, isLast);
                }
                else
                {
                    rc = HandleTextLine(textLine, info, info.StartCharPos, info.EndCharPos, false, false);
                }

                if (rc != null)
                {
                    rects.Add(rc.Value);
                }
            }

            return rects;
        }

        /*
         * param:
         *  isLastTextLine - means the last TextLine of a LogicLine
         *  isLastLogicLineTextLine - means the last LogicLine's the last TextLine
         */
        private Rect? HandleTextLine(TextLine textLine, TextLineInfo info, int startCol, int endCol, bool isLastTextLine, bool isLastLogicLineTextLine)
        {
            if (startCol == endCol)
            {
                return null;
            }

            var left = textLine.GetDistanceFromCharacterHit(new CharacterHit(startCol, 0));
            var top = info.AbsoluteTop;
            var bottom = info.AbsoluteTop + textLine.Height;
            var right = textLine.GetDistanceFromCharacterHit(new CharacterHit(isLastLogicLineTextLine ? endCol : endCol + 1, 0));

            //ensure in the wrap lines, the trail white spaces can be rendered.
            if (!isLastTextLine && !isLastLogicLineTextLine && IsWrap)
            {
                var whiteSpace = textLine.WidthIncludingTrailingWhitespace - textLine.Width;
                right += whiteSpace;
            }

            var width = right - left;
            var height = bottom - top;
            //bug: why we must + 1.0, otherwise, a slight gap between lines.
            var rc = new Rect(left, top, width, height + 1.0);
            return rc;
        }

        private void RenderRects(IEnumerable<Rect> rects)
        {
            using (var dc = Visual.RenderOpen())
            {
                foreach (var rc in rects)
                {
                    dc.DrawRectangle(Brush, Pen, rc);
                }
            }
        }

        #endregion

        #region Private Properties

        private DrawingVisual Visual { get; set; }

        private readonly Brush _brush = Brushes.PowderBlue;

        private Brush Brush
        {
            get { return _brush; }
        }

        private readonly Pen _pen = new Pen();

        private Pen Pen
        {
            get { return _pen; }
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
    }
}
