using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;

namespace Vortex
{
    public partial class MainWindow : Window
    {
        private Socket? _socket;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateVortex();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string serverIp = ServerIpTextBox.Text;
            int port = int.Parse(PortTextBox.Text);

            try
            {
                if (_socket == null || !_socket.Connected)
                {
                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    await _socket.ConnectAsync(serverIp, port);
                    MessageBox.Show("Connected to server");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}");
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_socket != null && _socket.Connected)
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket = null;
            }
            MessageBox.Show("Disconnected from server");
        }
    }
}
