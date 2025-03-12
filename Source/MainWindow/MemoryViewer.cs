using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Vortex
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource? _ctsUpdateMemory;

        private async void AutoUpdateCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(_ctsDisconnect != null)
            {
                _ctsUpdateMemory = new CancellationTokenSource();
                await Task.Run(() => UpdateMemory(_ctsDisconnect.Token, _ctsUpdateMemory.Token));
            }
        }

        private void AutoUpdateCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _ctsUpdateMemory?.Cancel();
        }

        private async Task UpdateMemory(CancellationToken tokenDisconnect, CancellationToken tokenUpdateMemory)
        {
            if(_socket != null && _socket.Connected)
            {
                while (!tokenDisconnect.IsCancellationRequested && !tokenUpdateMemory.IsCancellationRequested)
                {
                    uint address = 0;
                    this.Dispatcher.Invoke(() =>
                    {
                        address = ConvertEx.ToUInt32(AddressTextBox.Text);
                    });

                    string commandStr = string.Format("00{0:X8}00000004", address);
                    byte[] command = ConvertEx.ToBytes(commandStr);
                    Packet sendPacket = new Packet(command, Packet.DATA_TYPE_DATA);
                    await sendPacket.SendPacketAsync(_socket);
                    Packet receivePacket = new Packet();
                    await receivePacket.ReceivePacketAsync(_socket);
                    Dispatcher.Invoke(() =>
                    {
                        ValueTextBox.Text = BitConverter.ToString(receivePacket.Data).Replace("-", "");
                    });
                }
            }
        }
    }
}
