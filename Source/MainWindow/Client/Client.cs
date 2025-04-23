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
        private bool _isConnecting = false;

        private void ConnectTabInit()
        {
            EnableTabPages(false);
        }

        private async Task ConnectToServer()
        {
            if (_isConnecting) return;

            // IPアドレスとポートを取得
            string serverIp = ServerIpTextBox.Text;
            int port = int.Parse(PortTextBox.Text);

            try
            {
                _isConnecting = true;

                // 接続されていなかったら接続を試みる
                if (_socket == null || !_socket.Connected)
                {
                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    // タイムアウト3秒で接続を試みる
                    using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(3)))
                    {
                        UpdateConnectButtonState(ConnectionState.Connecting);
                        await _socket.ConnectAsync(serverIp, port, cts.Token);
                    }
                    _socket.NoDelay = true; // 遅延を緩和

                    UpdateConnectButtonState(ConnectionState.Connected);

                    // キャンセルトークンを生成して、無限ループを管理
                    _ctsDisconnect = new CancellationTokenSource();
                    await Task.Run(() => CheckConnection(_ctsDisconnect.Token));
                }
            }
            catch (OperationCanceledException)
            {
                UpdateConnectButtonState(ConnectionState.Disconnected);
                MessageBox.Show("接続がタイムアウトしました（3秒以内に応答なし）");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}");
            }
            finally
            {
                _isConnecting = false;
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

                Dispatcher.Invoke(() =>
                {
                    UpdateConnectButtonState(ConnectionState.Disconnected);
                });

                Debug.Write("Disconnection is done");
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

        private void UpdateConnectButtonState(ConnectionState connectionState)
        {
            switch (connectionState)
            {
                case ConnectionState.Connecting:
                    ConnectButton.IsEnabled = false;
                    ConnectButton.Background = Brushes.Gray;
                    DisconnectButton.IsEnabled = false;
                    DisconnectButton.Background = Brushes.Gray;
                    break;
                case ConnectionState.Connected:
                    ConnectButton.IsEnabled = false;
                    ConnectButton.Background = Brushes.Gray;
                    DisconnectButton.IsEnabled = true;
                    DisconnectButton.Background = Brushes.Red;
                    EnableTabPages(true);
                    break;
                case ConnectionState.Disconnected:
                    ConnectButton.IsEnabled = true;
                    ConnectButton.Background = Brushes.Green;
                    DisconnectButton.IsEnabled = false;
                    DisconnectButton.Background = Brushes.Gray;
                    EnableTabPages(false);
                    break;
            }
        }

        private void EnableTabPages(bool enabled)
        {
            CommandTab.IsEnabled = enabled;
            MemoryTab.IsEnabled = enabled;
            DisassemblerTab.IsEnabled = enabled;
        }

        private enum ConnectionState
        {
            Connecting,
            Connected,
            Disconnected,
        }
    }
}
