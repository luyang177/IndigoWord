using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace IndigoWord.Utility.Bahaviors
{
    class BehaviorBase<T> : Behavior<T> where T : UIElement
    {
        #region Command Dependency Property

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(BehaviorBase<T>)
            );

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        #endregion
    }
}
