using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using static MaterialDesignThemes.Wpf.Theme;

namespace Vortex
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource? _ctsUpdateMemory;

        int memoryColumnCount = 4;
        int memoryRowCount = 0x100;

        private void MemoryDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateMemoryCell(memoryColumnCount, memoryRowCount);
        }

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

        private void GenerateMemoryCell(int columnCount, int rowCount)
        {
            DataTable table = new DataTable();

            // 列の追加
            table.Columns.Add($"Address", typeof(string));  // 文字列型の列
            for (int col = 0; col < columnCount; col++)
            {
                table.Columns.Add($"+{(col * 4):X}", typeof(string));  // 文字列型の列
            }

            // 行の追加
            for (int row = 0; row < rowCount; row++)
            {
                table.Rows.Add(table.NewRow());
            }

            // DataGrid にバインド
            MemoryDataGrid.ItemsSource = table.DefaultView;
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

                    uint[] result = await Command.ReadUInt32(_socket, address, memoryRowCount * 4);

                    UpdateMemoryCells(address, result);

                    await Task.Delay(10); // 更新間隔を調整
                }
            }
        }

        private void UpdateMemoryCells(uint address, uint[] result)
        {
            // UIスレッドで一括更新
            Dispatcher.Invoke(() =>
            {
                if (MemoryDataGrid.ItemsSource is DataView dataView)
                {
                    DataTable? table = dataView.Table;
                    if (table != null)
                    {
                        int rowCount = table.Rows.Count;

                        for (int row = 0; row < rowCount; row++)
                        {
                            table.Rows[row][0] = $"{address + (uint)(row * 4 * memoryColumnCount):X8}"; // アドレスを更新

                            for (int col = 0; col < memoryColumnCount; col++)
                            {
                                int index = row * memoryColumnCount + col;
                                if (index < result.Length)
                                {
                                    table.Rows[row][col + 1] = $"{result[index]:X8}";
                                }
                                else
                                {
                                    table.Rows[row][col + 1] = "--------"; // データがない場合の埋め
                                }
                            }
                        }
                    }
                }

            });
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
