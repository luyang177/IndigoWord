using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using IndigoWord.Render;
using IndigoWord.Utility;

namespace IndigoWord.Controls
{
    class ScrollGrid : Grid, IScrollInfo, IOffset
    {
        #region Constructor

        public ScrollGrid()
        {
            InitTransform();
        }

        #endregion

        #region Public Properties

        private TranslateTransform Trans { get; set; }

        #endregion

        #region Dependency Properties

        #region TargetArea Dependency Properties

        public static readonly DependencyProperty TargetAreaProperty = DependencyProperty.Register(
            "TargetArea", typeof (UIElement), typeof (ScrollGrid), new PropertyMetadata(default(UIElement), OnTargetAreaChanged));

        public UIElement TargetArea
        {
            get { return (UIElement) GetValue(TargetAreaProperty); }
            set { SetValue(TargetAreaProperty, value); }
        }

        private static void OnTargetAreaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as ScrollGrid;
            if (self == null)
            {
                return;
            }

            var element = e.NewValue as UIElement;
            if (element == null)
            {
                return;
            }

            element.RenderTransform = self.Trans;
        }

        #endregion

        #region Extent Dependency Properties

        public static readonly DependencyProperty ExtentProperty = DependencyProperty.Register(
            "Extent", typeof (Size), typeof (ScrollGrid), new PropertyMetadata(default(Size), OnExtentChanged));

        public Size Extent
        {
            get { return (Size) GetValue(ExtentProperty); }
            set { SetValue(ExtentProperty, value); }
        }

        private static void OnExtentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as ScrollGrid;
            if (self == null)
            {
                return;
            }

            self._extent = (Size)e.NewValue;
            self.ScrollOwner.InvalidateScrollInfo();
        }

        #endregion

        #endregion

        #region Private Methods

        private void InitTransform()
        {            
            Trans = new TranslateTransform();
        }

        private void CalcScrollInfo(Size actualDisplay)
        {
            _viewport = actualDisplay;
        }

        #endregion

        #region Implementation of FrameworkElement

        protected override Size MeasureOverride(Size availableSize)
        {
            //CalcScrollInfo(availableSize);
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            CalcScrollInfo(finalSize);
            return base.ArrangeOverride(finalSize);
        }

        #endregion

        #region Implementation of IScrollInfo

        /*
         *  Offset in the total size
         *  Document vary from top to bottom means this offset vary from 0 to a positive number.
         */
        private Vector _offset = new Vector(0, 0);

        //Total size
        private Size _extent;

        //Single screen size
        private Size _viewport;

        private double _mouseWheelStep = 50;

        public bool CanHorizontallyScroll { get; set; }

        public bool CanVerticallyScroll { get; set; }

        public double ExtentHeight
        {
            get { return _extent.Height; }
        }

        public double ExtentWidth
        {
            get { return _extent.Width; }
        }

        public double HorizontalOffset
        {
            get { return _offset.X; }
        }

        public void LineDown()
        {
            
        }

        public void LineLeft()
        {
            
        }

        public void LineRight()
        {
            
        }

        public void LineUp()
        {
            
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            return rectangle;
        }

        public void MouseWheelDown()
        {
            SetVerticalOffset(VerticalOffset + _mouseWheelStep);
        }

        public void MouseWheelLeft()
        {
            
        }

        public void MouseWheelRight()
        {
            
        }

        public void MouseWheelUp()
        {
            SetVerticalOffset(VerticalOffset - _mouseWheelStep);
        }

        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + _viewport.Height);
        }

        public void PageLeft()
        {
            
        }

        public void PageRight()
        {
            
        }

        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - _viewport.Height);
        }

        public ScrollViewer ScrollOwner { get; set; }

        public void SetHorizontalOffset(double offset)
        {
            _offset.X = offset;
            Trans.X = 0 - offset;
        }

        public void SetVerticalOffset(double offset)
        {
            var min = 0.0;
            var max = ExtentHeight - _viewport.Height;

            var finalOffset = offset.Clamp(min, max);

            _offset.Y = finalOffset;
            Trans.Y = 0 - finalOffset;

            ScrollOwner.InvalidateScrollInfo();
        }

        public double VerticalOffset
        {
            get { return _offset.Y; }
        }

        public double ViewportHeight
        {
            get { return _viewport.Height; }
        }

        public double ViewportWidth
        {
            get { return _viewport.Width; }
        }

        #endregion

        #region Implementation of IOffset

        //Already implemented by IScrollInfo

        //double HorizontalOffset { get; }

        //double VerticalOffset { get; }

        #endregion
    }
}
