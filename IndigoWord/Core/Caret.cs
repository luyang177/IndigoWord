﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using IndigoWord.Render;
using IndigoWord.Utility;

namespace IndigoWord.Core
{
    class Caret
    {
        #region Constructor

        public Caret(ILayerProvider layerProvider, TextDocument document)
        {
            if (layerProvider == null)
                throw new ArgumentNullException("layerProvider");
            
            var layer = InitLayer(layerProvider);
            Init(layer);

            if (document == null)
                throw new ArgumentNullException("document");

            Document = document;
        }

        #endregion

        #region Public Properties

        private TextPosition _position = new TextPosition(0, 0);
        public TextPosition Position
        {
            get { return _position; }
            set
            {
                //not use equal guard, cause even same position can influence something eg. influence blink animation
                _position = value;

                var rc = CalcCaretRect(_position);
                UpdateCaretRect(rc);
            }
        }

        public TextDocument Document { get; set; }

        public Rect CaretRect { get; private set; }

        #endregion

        #region Public Methods

        public void Init(ILayer layer)
        {
            StartBlinkAnimation();

            Visual = new DrawingVisual();
            layer.Add(Visual);

            Render();
        }

        public void UpdateCaretRect(Rect rc)
        {
            CaretRect = rc;
            Blink = true;
            Render();
        }

        public bool IsCaret(Visual visual)
        {
            return Visual == visual;
        }

        #endregion

        #region Private Properties And Fields

        private DrawingVisual Visual { get; set; }

        private bool Blink { get; set; }

        private readonly DispatcherTimer _caretBlinkTimer = new DispatcherTimer();

        #endregion

        #region Private Methods

        private ILayer InitLayer(ILayerProvider layerProvider)
        {
            var layer = layerProvider.Get(LayerNames.Adorner);
            return layer;
        }

        private void StartBlinkAnimation()
        {
            var blinkTime = Win32.CaretBlinkTime;
            Blink = true; 
            // This is important if blinking is disabled (system reports a negative blinkTime)
            if (blinkTime.TotalMilliseconds > 0)
            {
                _caretBlinkTimer.Tick += OnBlinkTimerTick;
                _caretBlinkTimer.Interval = blinkTime;
                _caretBlinkTimer.Start();
            }
        }

        private void OnBlinkTimerTick(object sender, EventArgs eventArgs)
        {
            Blink = !Blink;
            Render();
        }

        private void Render()
        {
            using (var dc = Visual.RenderOpen())
            {
                if (!Blink)
                    return;

                var pen = new Pen();
                dc.DrawRectangle(Brushes.Indigo, pen, CaretRect);
            }
        }

        private Rect CalcCaretRect(TextPosition position)
        {
            Debug.Assert(Document.VerifyTextPosition(position));

            var logicLine = Document.GetLogicLine(position.Line);

            int column = position.Column;
            var xPos = logicLine.GetXPosition(column);
            double lineTop = logicLine.GetTop(column);
            double lineBottom = logicLine.GetBottom(column);

            return new Rect(xPos,
                            lineTop,
                            SystemParameters.CaretWidth,
                            lineBottom - lineTop);
        }

        #endregion
    }
}
