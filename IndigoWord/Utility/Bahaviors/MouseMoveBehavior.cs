using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace IndigoWord.Utility.Bahaviors
{
    class MouseMoveBehavior : BehaviorBase<UIElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject == null)
                return;

            AssociatedObject.MouseMove += OnMouseMove;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject == null)
                return;

            AssociatedObject.MouseMove -= OnMouseMove;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
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
                Point = pt
            };

            Command.Execute(param);
        }
    }
}