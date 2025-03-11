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
                _cancellationTokenSource?.Cancel();

                // ソケットのシャットダウンとクローズ
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket = null;
            }
            MessageBox.Show("Disconnected from server");
        }

        public async Task ReceivePacketFromServer(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_socket != null && _socket.Connected)
                {
                    Packet packet = new Packet();
                    int result = await packet.ReceivePacketAsync(_socket);
                    if(result == 0)
                    {
                        DisconnectServer();
                    }
                    ProcessData(_socket, packet.Data, packet.Type);
                }
                await Task.Delay(10);
            }
        }
    }
}
