using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using IndigoWord.Render;

namespace IndigoWord.Behaviors
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

        #region Mapper Dependency Property

        public static readonly DependencyProperty MapperProperty =
            DependencyProperty.Register("Mapper", typeof(IMapper), typeof(BehaviorBase<T>), new UIPropertyMetadata(default(IMapper)));

        public IMapper Mapper
        {
            get { return (IMapper)GetValue(MapperProperty); }
            set { SetValue(MapperProperty, value); }
        }

        #endregion
    }
}
