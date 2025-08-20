using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vortex.Services
{
    public class ServiceContext
    {
        public ConnectionService Connection { get; }
        public LoggerService Logger { get; }

        public ServiceContext()
        {
            Connection = new ConnectionService();
            Logger = new LoggerService();
        }
    }
}
