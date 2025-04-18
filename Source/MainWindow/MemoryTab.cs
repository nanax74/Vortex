using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace Vortex
{
    public class MemoryEntry : INotifyPropertyChanged
    {
        private uint _address;
        private uint _value;

        public uint Address
        {
            get => _address;
            set
            {
                if (_address != value)
                {
                    _address = value;
                    OnPropertyChanged(nameof(Address));
                }
            }
        }

        public uint Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }

    public class MemoryViewModel
    {
        public ObservableCollection<MemoryEntry> MemoryEntries { get; set; }

        public MemoryViewModel()
        {
            MemoryEntries = new ObservableCollection<MemoryEntry>();

            // 0x100 (256) 個の空エントリを作成
            for (int i = 0; i < 0x100; i++)
            {
                MemoryEntries.Add(new MemoryEntry
                {
                    Address = (uint)(0x10000000 + i * 4), // 例: 0x10000000 から4バイト間隔で
                    Value = 0
                });
            }
        }
    }

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
            if (_socket != null && _socket.Connected)
            {
                while (!tokenDisconnect.IsCancellationRequested && !tokenUpdateMemory.IsCancellationRequested)
                {
                    uint address = 0;
                    Dispatcher.Invoke(() =>
                    {
                        address = ConvertEx.ToUInt32(AddressTextBox.Text);
                    });

                    Debug.WriteLine($"Address: {address:X8}");

                    uint[] result = await Command.ReadUInt32(_socket, address, 0x400);

                    // UIスレッドで一括更新
                    Dispatcher.Invoke(() =>
                    {
                        // バックグラウンドでデータを準備
                        for (int i = 0; i < result.Length; i++)
                        {
                            viewModel.MemoryEntries[i].Address = address + (uint)(i * 4);
                            viewModel.MemoryEntries[i].Value = result[i];
                        }
                    });

                    await Task.Delay(10); // 更新間隔を調整
                }
            }
        }

        private void HexOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 16進数に使える文字か判定（0-9, A-F, a-f）
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(
                e.Text,
                @"^[0-9a-fA-F]+$"
            );
        }

    }
}
