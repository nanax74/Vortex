using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Vortex
{
    public partial class MainWindow : Window
    {
        public async Task UpdateVortex()
        {
            while (true)
            {
                await ReceivePacketFromServer();
                await Task.Delay(1000);
            }
        }

        public async Task ReceivePacketFromServer()
        {

        }
    }
}
