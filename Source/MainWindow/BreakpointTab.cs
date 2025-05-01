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
        private async void SetDataBreakpointButton_Click(object sender, RoutedEventArgs e)
        {
            if (_socket != null && _socket.Connected)
            {
                uint address = ConvertEx.ToUInt32(DataBreakpointAddressTextBox.Text);
                address = address & 0xFFFFFFF8; // 8バイト境界に揃える
                bool writeFlag = WriteFlagCheckBox.IsChecked == true;
                bool readFlag = ReadFlagCheckBox.IsChecked == true;
                uint value = address; // 有効フラグ（bit2）は常に立てる

                if (readFlag) value |= (1 << 0); // 読み取りフラグ
                if (writeFlag) value |= (1 << 1); // 書き込みフラグ
                value |= (1 << 2); // 有効化

                await Command.SetDatabreakPoint(_socket, value);
            }
        }
    }
}
