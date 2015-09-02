using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace IndigoWord.Utility.Bahaviors
{
    public sealed class HitVisualParam
    {
        public DrawingVisual Visual { get; set; }

        public Point Position { get; set; }
    }

    sealed class HitDrawingVisualBehavior : BehaviorBase<UIElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject == null)
                return;

            AssociatedObject.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject == null)
                return;

            AssociatedObject.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DoHitTest(sender, e);
        }

        private void DoHitTest(object sender, MouseButtonEventArgs e)
        {
            if(Command == null)
                return;

            var el = sender as UIElement;
            if (el == null)
                return;

            var pt = e.GetPosition(el);

            var hitResult = VisualTreeHelper.HitTest(el, pt);
            if (hitResult == null)
                return;

            var drawingVisual = hitResult.VisualHit as DrawingVisual;
            var param = new HitVisualParam
            {
                Visual = drawingVisual,
                Position = pt
            };

            Command.Execute(param);
            
        }

    }
}
