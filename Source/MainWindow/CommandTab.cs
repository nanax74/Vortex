using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Vortex
{
    public partial class MainWindow : Window
    {
        // 送信と受信の順序を保証するためのロック
        private static readonly SemaphoreSlim _sendLockCommand = new SemaphoreSlim(1, 1);

        private async void CommandSendButton_Click(object sender, RoutedEventArgs e)
        {
            if (_socket == null || !_socket.Connected)
            {
                MessageBox.Show("Not connected to server");
                return;
            }

            string command = CommandTextBox.Text;
            byte[] dataInput = ConvertEx.ToBytes(command);

            if (dataInput.Length == 0) return;

            Stopwatch stopwatch = Stopwatch.StartNew(); // 計測開始

            try
            {
                // 他の送信が完了するまで待つ (ロック)
                Debug.WriteLine("Waiting for send lock");
                await _sendLockCommand.WaitAsync();

                try
                {
                    // コマンドを送信
                    Packet sendPacket = new Packet(dataInput, Packet.DATA_TYPE_DATA);
                    await sendPacket.SendPacketAsync(_socket);

                    Packet receivePacket = new Packet();
                    while(receivePacket.Type == Packet.DATA_TYPE_DATA)
                    {
                        await receivePacket.ReceivePacketAsync(_socket);
                        ResponseTextBox.Text = BitConverter.ToString(receivePacket.Data).Replace("-", "");
                    }
                }
                finally
                {
                    // ロックを解放
                    _sendLockCommand.Release();
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
