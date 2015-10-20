using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IndigoWord.Core;
using IndigoWord.Render;

namespace IndigoWord
{
    /// <summary>
    /// IndigoWordEdit.xaml 的交互逻辑
    /// </summary>
    public partial class IndigoWordEdit : UserControl
    {
        private SimpleLayerProvider LayerProvider { get; set; }

        private ImeSupport _ime;

        private TextEditor TextEditor { get; set; }


        private SimpleMapper _simpleMapper;

        public SimpleMapper SimpleMapper
        {
            get { return _simpleMapper ?? (_simpleMapper = new SimpleMapper(MyScrollGrid)); }
        }

        public IndigoWordEdit()
        {
            InitializeComponent();
        }

        private void Init()
        {
            LayerProvider = new SimpleLayerProvider();
            LayerProvider.Register(LayerNames.BackgroundLayer, BackgroundLayer);
            LayerProvider.Register(LayerNames.TextLayer, TextLayer);
            var adorner = InitAdorner();
            LayerProvider.Register(LayerNames.Adorner, adorner);

            TextEditor = new TextEditor(LayerProvider);

            /*
             * I don't know why I can't receive message WM_IME_COMPOSITION for just the Grid of text
             * but it's ok for this(IndigoWordEdit), so I have to pass MenuView.ActualHeight to ImeSupport, and calculate offset by myself.
             */
            _ime = new ImeSupport(this, MenuView.ActualHeight, TextEditor.CaretPositionProvider, TextEditor.FontRendering);

            DataContext = TextEditor;
        }

        private VisualAdorner InitAdorner()
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(LayerContainer);
            var adorner = new VisualAdorner(LayerContainer);
            adornerLayer.Add(adorner);

            return adorner;
        }

        /*
         * This is important to indicate the size to IndigoWordEdit.
         * TODO Make Height to support large text file
         */
        protected override Size MeasureOverride(Size constraint)
        {
            var size = constraint;
            size.Width = size.Width > 30000 ? 30000 : size.Width;
            size.Height = size.Height > 30000 ? 30000 : size.Height;

            return size;
        }

        #region Event Handlers

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextEditor.OnKeyDown(e.Key);
        }

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);
            TextEditor.OnTextInput(e);
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsFocused)
            {
                //KeyDown can be triggered
                Focus();
            }
        }

        #endregion
    }
}
