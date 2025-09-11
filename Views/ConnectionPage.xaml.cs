using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;
using Vortex.ViewModels;

namespace Vortex.Views;

public sealed partial class ConnectionPage : Page
{
    public ConnectionPage()
    {
        InitializeComponent();

        DataContext = App.GetService<ConnectionViewModel>();
    }
}
