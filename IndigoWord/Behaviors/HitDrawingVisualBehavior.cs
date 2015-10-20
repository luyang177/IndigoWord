using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace IndigoWord.Behaviors
{
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
            var param = new VisualParam
            {
                Visual = drawingVisual,
                Point = Mapper.MapScreen2Origin(pt)
            };

            Command.Execute(param);
        }
    }
}
