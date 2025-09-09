using Microsoft.UI.Xaml.Controls;

using Vortex.ViewModels;

namespace Vortex.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }
}
