using Microsoft.UI.Xaml.Controls;

using Vortex.ViewModels;

namespace Vortex.Views;

public sealed partial class ConnectionPage : Page
{
    public ConnectionViewModel ViewModel
    {
        get;
    }

    public ConnectionPage()
    {
        ViewModel = App.GetService<ConnectionViewModel>();
        InitializeComponent();
    }
}
