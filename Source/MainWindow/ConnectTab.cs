using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Vortex
{
    public partial class MainWindow : Window
    {
        // 通信するためのソケット
        private Socket? _socket;

        // 切断処理を実行するためのトークン
        private CancellationTokenSource? _ctsDisconnect;

        // 接続
        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            await ConnectToServer();
        }

        // 切断
        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            DisconnectFromServer();
        }
    }
}
