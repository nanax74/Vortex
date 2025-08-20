using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Vortex.Services;

namespace Vortex.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public ObservableCollection<IFeatureViewModel> Features { get; }
        public string DisplayName { get; }

        public MainViewModel(string name)
        {
            DisplayName = name;
            ServiceContext context = new ServiceContext();
            Features = new ObservableCollection<IFeatureViewModel>
            {
                new ConnectionViewModel(context),
                new LoggerViewModel(context)
            };
        }
    }
}