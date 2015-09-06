using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
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

            //TODO
            DocumentRender.Caret = Caret;

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
                return DocumentRender.IsWrap;
            }
            set
            {
                DocumentRender.IsWrap = value;

                //trigger caret redraw
                Caret.Position = Caret.Position;
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
            get { return _hitCommand ?? (_hitCommand = new RelayCommand<HitVisualParam>(Hit)); }
        }

        private void Hit(HitVisualParam param)
        {
            var textPosition = DocumentRender.Hit(param);
            if (textPosition == null)
            {
                return;
            }

            CaretPosition = textPosition;
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

        #region Private Properties

        private Caret Caret { get; set; }

        private TextDocument Document { get; set; }

        private DocumentRender DocumentRender { get; set; }

        private TextInputProcessorFactory TextInputProcessorFactory { get; set; }

        #endregion

        #region Private Methods

        private void InitRender(ILayerProvider layerProvider)
        {
            var layer = layerProvider.Get(LayerNames.Host);
            DocumentRender = new DocumentRender(layer);
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
            var textProcessor = TextInputProcessorFactory.Get(text);
            if (textProcessor == null)
            {
                throw new NullReferenceException("textProcessor");
            }
            var param = new TextInputProcessorParam
            {
                Document = Document,
                Render = DocumentRender,
                Position = Caret.Position,
                Text = text
            };
            CaretPosition = textProcessor.Process(param);
        }

        private void ProcessText(Key key)
        {            
            var textProcessor = TextInputProcessorFactory.Get(key);
            if (textProcessor == null)
            {
                return;
            }
            var param = new TextInputProcessorParam
            {
                Document = Document,
                Render = DocumentRender,
                Position = Caret.Position
            };
            CaretPosition = textProcessor.Process(param);
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
