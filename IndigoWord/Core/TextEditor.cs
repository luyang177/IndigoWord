using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using IndigoWord.Annotations;
using IndigoWord.Edit;
using IndigoWord.LowFontApi;
using IndigoWord.Mvvm;
using IndigoWord.Render;
using IndigoWord.Utility;
using IndigoWord.Utility.Bahaviors;
using Microsoft.Win32;

namespace IndigoWord.Core
{
    /*
     * TextEditor knows ILayer via ILayerProvider, not consider MVVM, MVVM is overkill now.
     */
    class TextEditor : INotifyPropertyChanged
    {
        #region Constructor

        public TextEditor(ILayerProvider layerProvider)
        {
            if (layerProvider == null)
            {
                throw new ArgumentNullException("layerProvider");
            }

            InitRender(layerProvider);

            Document = OpenFileAtStartTime();
            DocumentRender.Show(Document);

            InitCaret(layerProvider, Document);

            SelectionRender = new SelectionRender(layerProvider, WrapState);

            //set caret initial position
            Caret.Position = new TextPosition(0,0);

            TextInputProcessorFactory = new TextInputProcessorFactory();
        }

        #endregion

        #region Public Properties

        public TextPosition CaretPosition
        {
            get
            {
                return Caret.Position;
            }
            set
            {
                Caret.Position = value;
                OnPropertyChanged();
            }
        }


        public bool IsWrap
        {
            get
            {
                return WrapState.IsWrap;
            }
            set
            {
                if (WrapState.IsWrap == value)
                {
                    return;
                }

                var lastIsWrap = WrapState.IsWrap;
                WrapState.IsWrap = value;

                SelectionRender.Clear();
                ShowSelectionText();

                var pos = HandleWrapCaretPosition(lastIsWrap);

                //trigger caret redraw
                Caret.Position = pos;
            }
        }

        public ICaretPosition CaretPositionProvider
        {
            get { return Caret; }
        }

        public FontRendering FontRendering
        {
            get { return DocumentRender.FontRendering; }
        }

        #endregion

        #region Commands

        #region Open file command

        private ICommand _openCommand;

        public ICommand OpenCommand
        {
            get { return _openCommand ?? (_openCommand = new RelayCommand(OpenFile)); }
        }

        private void OpenFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = "c:\\", 
                RestoreDirectory = true
            };

            var ret = openFileDialog.ShowDialog();
            if (ret.HasValue && ret.Value)
            {
                var path = openFileDialog.FileName;

                DocumentRender.Reset();
                Document = TextDocument.Open(path);
                DocumentRender.Show(Document);
                Caret.Document = Document;
                CaretPosition = new TextPosition(0, 0);
            }
        }

        #endregion

        #region Hit Command

        private ICommand _hitCommand;

        public ICommand HitCommand
        {
            get { return _hitCommand ?? (_hitCommand = new RelayCommand<VisualParam>(Hit)); }
        }

        private void Hit(VisualParam param)
        {
            var textPosition = DocumentRender.Hit(param);
            if (textPosition == null)
            {
                return;
            }

            var pos = textPosition.Value;

            CaretPosition = pos;
            _selectionRange = new TextRange(pos);
            SelectionRender.Clear();
        }

        #endregion

        #region MouseMove Command

        private ICommand _mouseMoveCommand;

        public ICommand MouseMoveCommand
        {
            get { return _mouseMoveCommand ?? (_mouseMoveCommand = new RelayCommand<VisualParam>(MouseMove)); }
        }

        private void MouseMove(VisualParam param)
        {
            var textPosition = DocumentRender.Hit(param);
            if (textPosition == null)
            {
                return;
            }

            var pos = textPosition.Value;

            CaretPosition = pos;
            _selectionRange.Change(pos);

            ShowSelectionText();
        }

        #endregion

        #region Test Command

        private ICommand _testCommand;

        public ICommand TestCommand
        {
            get { return _testCommand ?? (_testCommand = new RelayCommand(Test)); }
        }

        private void Test()
        {
            _selectionRange = new TextRange(new TextPosition(0, 3), new TextPosition(2, 3));
            ShowSelectionText();
        }

        #endregion

        #endregion

        #region Public Methods

        public void OnKeyDown(Key key)
        {
            if (key == Key.Left || key == Key.Right || key == Key.Down || key == Key.Up)
            {
                CaretPosition = CaretTraveller.DirectionKey(Document, Caret, key);
            }
            else if (key == Key.Home)
            {
                CaretPosition = CaretTraveller.Home(Document, CaretPosition);
            }
            else if (key == Key.End)
            {
                CaretPosition = CaretTraveller.End(Document, CaretPosition, IsWrap);
            }
            else if (key == Key.Insert)
            {
                MessageBox.Show("Not support Key Insert");
            }
            else
            {
                //those keys doesn't trigger OnTextInput like Key.Delete
                ProcessText(key);
            }
        }

        public void OnTextInput(TextCompositionEventArgs e)
        {
            var text = e.Text;
            if (text == null)
            {
                return;
            }

            ProcessText(text);
        }

        #endregion

        #region Private Properties and Fields

        private Caret Caret { get; set; }

        private TextDocument Document { get; set; }

        private DocumentRender DocumentRender { get; set; }

        private SelectionRender SelectionRender { get; set; }

        /*
         * TextRange is a mutable struct, using property is a trap
         * because we will call getter to get a copy(remember this is a struct), and change the copy, not the real one
         * so use field instead.
         */
        private TextRange _selectionRange;

        private TextInputProcessorFactory TextInputProcessorFactory { get; set; }


        private readonly WrapState _wrapState = new WrapState();

        private WrapState WrapState
        {
            get { return _wrapState; }
        }

        private bool IsSelectionRange
        {
            get { return _selectionRange.IsRange; }
        }

        #endregion

        #region Private Methods

        private void InitRender(ILayerProvider layerProvider)
        {
            var layer = layerProvider.Get(LayerNames.TextLayer);
            DocumentRender = new DocumentRender(layer, WrapState);
        }

        private void InitCaret(ILayerProvider layerProvider, TextDocument document)
        {
            Caret = new Caret(layerProvider, document);
        }

        private TextDocument OpenFileAtStartTime()
        {
            var latestFile = CommonSetting.Instance.LatestDocPath;

            if (latestFile == null || !File.Exists(latestFile))
            {
                return TextDocument.Empty();
            }
            else
            {
                return TextDocument.Open(latestFile);
            }
        }


        private void ProcessText(string text)
        {
            var textProcessor = TextInputProcessorFactory.Get(text, IsSelectionRange);
            if (textProcessor == null)
            {
                throw new NullReferenceException("textProcessor");
            }
            
            DoProcessText(textProcessor, text);
        }

        private void ProcessText(Key key)
        {
            var textProcessor = TextInputProcessorFactory.Get(key, IsSelectionRange);
            if (textProcessor == null)
            {
                return;
            }

            DoProcessText(textProcessor, null);
        }

        private void DoProcessText(TextInputProcessor processor, string text)
        {
            var param = new TextInputProcessorParam
            {
                Document = Document,
                Render = DocumentRender,
                Position = Caret.Position,
                TextRange = _selectionRange,
                Text = text
            };
            CaretPosition = processor.Process(param);

            _selectionRange = new TextRange();
            SelectionRender.Clear();
        }

        private void ShowSelectionText()
        {
            if (IsSelectionRange)
            {
                SelectionRender.Show(Document, _selectionRange);
            }
        }

        /*
         * Make caret position in the right position when at the head of TextLine.
         */
        private TextPosition HandleWrapCaretPosition(bool lastIsWrap)
        {
            TextPosition pos;
            if (lastIsWrap)
            {
                pos = new TextPosition(Caret.Position.Line, Caret.Position.Column, false);
            }
            else
            {
                var logicLine = Document.FindLogicLine(Caret.Position.Line);
                if (logicLine.IsHeadOfTextLine(Caret.Position))
                {
                    pos = new TextPosition(Caret.Position.Line, Caret.Position.Column, true);
                }
                else
                {
                    pos = new TextPosition(Caret.Position.Line, Caret.Position.Column, false);
                }
            }

            return pos;
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) 
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
