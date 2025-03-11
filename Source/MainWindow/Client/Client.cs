using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Vortex
{
    public partial class MainWindow : Window
    {
        public void DisconnectServer()
        {
            if (_socket != null && _socket.Connected)
            {
                // キャンセル処理
                _ctsDisconnect?.Cancel();

                // ソケットのシャットダウンとクローズ
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket = null;
            }
            MessageBox.Show("Disconnected from server");
        }

        public async Task CheckConnection(CancellationToken token)
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
                            DisconnectServer();
                            return;
                        }

                        Debug.WriteLine("Checking connection");
                    }
                    catch
                    {
                        Debug.WriteLine("Connection is dead");
                        DisconnectServer();
                        return;
                    }
                }
                await Task.Delay(500);
            }
        }
    }
}
