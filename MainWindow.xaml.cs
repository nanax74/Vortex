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

        const int DATA_TYPE_COMMAND = 0x01;
        const int DATA_TYPE_RESPONSE = 0x02;
        const int DATA_TYPE_PROGRESS = 0x03;

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (_socket == null || !_socket.Connected)
            {
                MessageBox.Show("Not connected to server");
                return;
            }

            string command = CommandTextBox.Text;
            byte[] dataInput = ConvertEx.ToBytes(command);

            if (dataInput.Length < 1) return;

            Stopwatch stopwatch = Stopwatch.StartNew(); // 計測開始

            try
            {
                // 他の送信が完了するまで待つ (ロック)
                Debug.WriteLine("Waiting for send lock");
                await _sendLock.WaitAsync();

                try
                {
                    // コマンドを送信
                    await Data.SendPacketAsync(_socket, dataInput, DATA_TYPE_COMMAND);

                    // 応答を受信
                    int result;
                    byte[] dataOutput = Array.Empty<byte>();
                    int type = 0;
                    while (type != DATA_TYPE_RESPONSE)
                    {
                        (result, dataOutput, type) = await Data.ReceivePacketAsync(_socket);
                        if (type == DATA_TYPE_PROGRESS)
                        {
                            string progress = BitConverter.ToString(dataOutput).Replace("-", "");

                            // 応答を表示
                            ResponseTextBox.Text = progress;
                        }
                    }

                    // 受け取ったデータを文字列に変換
                    string response = BitConverter.ToString(dataOutput).Replace("-", "");

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
            finally
            {
                stopwatch.Stop(); // 計測終了
                Debug.WriteLine($"SendButton_Click executed in {stopwatch.ElapsedMilliseconds} ms");
            }
        }
    }
}
