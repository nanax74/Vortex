using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Vortex
{
    public partial class MainWindow : Window
    {
        private async Task ConnectToServer()
        {
            // IPアドレスとポートを取得
            string serverIp = ServerIpTextBox.Text;
            int port = int.Parse(PortTextBox.Text);

            try
            {
                // 接続されていなかったら接続を試みる
                if (_socket == null || !_socket.Connected)
                {
                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    await _socket.ConnectAsync(serverIp, port);
                    _socket.NoDelay = true; // 遅延を緩和

                    UpdateConnectButtonState(true);

                    // キャンセルトークンを生成して、無限ループを管理
                    _ctsDisconnect = new CancellationTokenSource();
                    await Task.Run(() => CheckConnection(_ctsDisconnect.Token));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}");
            }
        }

        private void DisconnectFromServer()
        {
            if (_socket != null && _socket.Connected)
            {
                // キャンセル処理
                _ctsDisconnect?.Cancel();

                // ソケットのシャットダウンとクローズ
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket = null;

                UpdateConnectButtonState(false);
            }
        }

        private async Task CheckConnection(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_socket != null && _socket.Connected)
                {
                    try
                    {
                        // Pollを使って切断をチェック
                        if (_socket.Poll(1000, SelectMode.SelectRead) && _socket.Available == 0)
                        {
                            Debug.WriteLine("Connection is dead");
                            DisconnectFromServer();
                            return;
                        }

                        Debug.WriteLine("Checking connection");
                    }
                    catch
                    {
                        Debug.WriteLine("Connection is dead");
                        DisconnectFromServer();
                        return;
                    }
                }
                await Task.Delay(500);
            }
        }

        private void UpdateConnectButtonState(bool connected)
        {
            if (connected)
            {
                ConnectButton.IsEnabled = false;
                ConnectButton.Background = Brushes.Gray;

                DisconnectButton.IsEnabled = true;
                DisconnectButton.Background = Brushes.Red;
            }
            else
            {
                ConnectButton.IsEnabled = true;
                ConnectButton.Background = Brushes.Green;

                DisconnectButton.IsEnabled = false;
                DisconnectButton.Background = Brushes.Gray;
            }
        }
    }
}
