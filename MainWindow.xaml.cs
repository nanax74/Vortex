using System;
using System.Diagnostics;
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

        // 送信と受信の順序を保証するためのロック
        private static readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (_socket == null || !_socket.Connected)
            {
                MessageBox.Show("Not connected to server");
                return;
            }

            string command = CommandTextBox.Text;
            byte[] data = ConvertEx.ToBytes(command);

            try
            {
                // 他の送信が完了するまで待つ (ロック)
                Debug.WriteLine("Waiting for send lock");
                await _sendLock.WaitAsync();

                try
                {
                    // コマンドを送信
                    await Data.SendDataAsync(_socket, data);

                    // 応答を受信
                    byte[] receivedData = await Data.ReceiveDataAsync(_socket);

                    // 受け取ったデータを文字列に変換
                    string response = BitConverter.ToString(receivedData);

                    // 応答を表示
                    ResponseTextBox.Text = response;
                }
                finally
                {
                    // ロックを解放
                    _sendLock.Release();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending data: {ex.Message}");
            }
        }
    }
}
