using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Vortex.Services;

namespace Vortex.ViewModels
{
    public class LoggerViewModel : ObservableObject, IFeatureViewModel
    {
        public string DisplayName { get; } = "Logger";

        private readonly ServiceContext _context;

        public ObservableCollection<string> Logs => _context.Logger.Logs;

        public LoggerViewModel(ServiceContext context)
        {
            _context = context;
        }
    }
}
