using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace tone.Views
{
    public class AddItemView : UserControl
    {
        public AddItemView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}