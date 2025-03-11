using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;

namespace Vortex
{
    public partial class MainWindow : Window
    {
        public void ProcessData(Socket socket, byte[] data, int type)
        {
            switch (type)
            {
                case Packet.DATA_TYPE_DATA:
                    string response = BitConverter.ToString(data).Replace("-", "");
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ResponseTextBox.Text = response;
                    });
                    break;
            }
        }
    }
}
