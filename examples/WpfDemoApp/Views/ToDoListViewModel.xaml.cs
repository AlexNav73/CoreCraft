using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfDemoApp.Views
{
    /// <summary>
    /// Interaction logic for ToDoListViewModel.xaml
    /// </summary>
    public partial class ToDoListViewModel : UserControl
    {
        public ToDoListViewModel()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(ToDoListViewModel), new PropertyMetadata(null));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(ToDoListViewModel), new PropertyMetadata(null));

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Command.Execute(CommandParameter);
        }
    }
}
