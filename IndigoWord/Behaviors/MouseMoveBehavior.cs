using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace IndigoWord.Behaviors
{
    class MouseMoveBehavior : BehaviorBase<UIElement>
    {
        private bool _isMove;

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject == null)
                return;

            AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject == null)
                return;

            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            AssociatedObject.MouseMove -= OnMouseMove;
            AssociatedObject.MouseLeftButtonUp -= OnMouseLeftButtonUp;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isMove = true;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMove)
            {
                return;
            }

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

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            _isMove = false;
        }
    }
}