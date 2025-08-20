using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vortex.Services
{
    public class LoggerService
    {
        public ObservableCollection<string> Logs { get; } = [];
        
        public void Log(string message)
        {
            Logs.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
        }
    }
}
