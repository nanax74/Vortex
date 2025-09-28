using Microsoft.UI.Xaml.Controls;
using Vortex.ViewModels;

namespace Vortex.Views;

public sealed partial class StreamPage : Page
{
    public StreamPage()
    {
        InitializeComponent();

        DataContext = App.GetService<StreamViewModel>();
    }
}
