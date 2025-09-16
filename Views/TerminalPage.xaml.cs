using Microsoft.UI.Xaml.Controls;
using Vortex.ViewModels;

namespace Vortex.Views;

public sealed partial class TerminalPage : Page
{
    public TerminalPage()
    {
        InitializeComponent();

        DataContext = App.GetService<TerminalViewModel>();
    }
}
