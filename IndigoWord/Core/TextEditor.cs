using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using IndigoWord.Annotations;
using IndigoWord.LowFontApi;
using IndigoWord.Mvvm;
using IndigoWord.Operation.Behaviors;
using IndigoWord.Render;
using IndigoWord.Utility;
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
            Show(Document);

            InitCaret(layerProvider, Document);

            //TODO
            DocumentRender.Caret = Caret;

            //set caret initial position
            Caret.Position = new TextPosition(0,0);
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
                Document = OpenDocument(path);
                Show(Document);
                Caret.Document = Document;
                Caret.Position = new TextPosition(0, 0);
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
            if (key == Key.Left)
            {
                var pos = Document.GetPreviousTextPosition(Caret.Position);
                CaretPosition = pos;
            }
            else if (key == Key.Right)
            {
                var pos = Document.GetNextTextPosition(Caret.Position);
                CaretPosition = pos;
            }
            else if (key == Key.Down)
            {
                var pos = Document.GetDownLineTextPosition(Caret.Position, Caret.CaretRect);
                CaretPosition = pos;
            }
            else if (key == Key.Up)
            {
                var pos = Document.GetUpLineTextPosition(Caret.Position, Caret.CaretRect);
                CaretPosition = pos;
            }

        }

        #endregion

        #region Private Properties

        private Caret Caret { get; set; }

        private TextDocument Document { get; set; }

        private DocumentRender DocumentRender { get; set; }

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

        private TextDocument OpenDocument(string path)
        {
            if (!File.Exists(path))
            {
                MessageBox.Show("File: {0} isn't exist",path);
                return null;
            }

            var lines = new List<LogicLine>();
            using (var sr = File.OpenText(path))
            {
                var s = "";
                int line = 0;
                while ((s = sr.ReadLine()) != null)
                {
                    s += Environment.NewLine;

                    var logicLine = new LogicLine(line, s);
                    lines.Add(logicLine);
                    
                    line++;
                }
            }

            CommonSetting.Instance.LatestDocPath = path;
            CommonSetting.Save();

            if (lines.Count == 0)
            {
                return TextDocument.Empty();
            }
            else
            {
                var document = new TextDocument(lines);
                return document;
            }
        }

        private void Show(TextDocument document)
        {
            if(document == null)
                return;

            DocumentRender.Show(document);
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
                return OpenDocument(latestFile);
            }
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
