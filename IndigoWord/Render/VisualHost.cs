using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace IndigoWord.Render
{
    class VisualHost : FrameworkElement, ILayer
    {
        #region Constructor

        public VisualHost()
        {
            _children = new VisualCollection(this);
        }

        #endregion

        #region Public Methods

        public VisualAdorner InitAdorner()
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(this);
            Adorner = new VisualAdorner(this);
            adornerLayer.Add(Adorner);

            return Adorner;
        }

        #endregion

        #region Implementation of ILayer

        public void Add(Visual visual)
        {
            _children.Add(visual);
        }

        public void Remove(Visual visual)
        {
            _children.Remove(visual);
        }

        public void Clear()
        {
            _children.Clear();
        }

        #endregion

        #region Fields

        private readonly VisualCollection _children;

        #endregion

        #region Private Properties

        private VisualAdorner Adorner { get; set; }

        #endregion

        #region Override FrameworkElement

        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount
        {
            get { return _children.Count; }
        }

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }

        #endregion
    }
}
