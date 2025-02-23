using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using Microsoft.VisualBasic;

namespace Vortex
{
    public partial class MainWindow : Window
    {
        private TcpClient _client = null!;
        private NetworkStream _stream = null!;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string serverIp = ServerIpTextBox.Text;
            int port = int.Parse(PortTextBox.Text);

            try
            {
                if(_client == null)
                {
                    _client = new TcpClient(serverIp, port);
                }

                if(_client != null && _stream == null)
                {
                    _stream = _client.GetStream();
                }

                MessageBox.Show("Connected to server");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}");
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_client != null)
            {
                _client.Close();
                _client = null!;
            }
            if (_stream != null)
            {
                _stream.Close();
                _stream = null!;
            }
            MessageBox.Show("Disconnected from server");
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (_client == null || !_client.Connected)
            {
                MessageBox.Show("Not connected to server");
                return;
            }

            string command = CommandTextBox.Text;
            byte[] data = ConvertEx.ToBytes(command);

            try
            {
                await _stream.WriteAsync(data, 0, data.Length);
                await _stream.FlushAsync();

                if (_client.Available == 0)
                {
                    MessageBox.Show("No response from server");
                    return;
                }

                byte[] responseData = new byte[_client.Available];
                int bytes = await _stream.ReadAsync(responseData, 0, responseData.Length);
                string response = BitConverter.ToString(responseData, 0, bytes);

                ResponseTextBox.Text = response;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending data: {ex.Message}");
            }
        }
    }
}
