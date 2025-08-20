using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Vortex.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<MainViewModel> Contexts { get; } = new();

    public MainWindowViewModel()
    {
        Contexts.Add(new MainViewModel("Test Context"));
    }
}
