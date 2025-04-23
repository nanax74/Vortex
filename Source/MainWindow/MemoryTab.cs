using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Vortex
{
    public partial class MainWindow : Window
    {
        bool _isMemoryUpdate = false;

        int memoryColumnCount = 4;
        int memoryRowCount = 0x10;

        // メモリタブのメインループ
        private async Task MemoryTabMain()
        {
            while(true)
            {
                await UpdateMemory();
                await Task.Delay(1000 / 120);
            }
        }

        // チェックボックスの状態に応じてメモリの自動更新フラグを設定
        private void AutoUpdateCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _isMemoryUpdate = true;
        }
        private void AutoUpdateCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _isMemoryUpdate = false;
        }

        // DataGridの行数、列数を指定してメモリセルを生成
        private void MemoryDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateMemoryCell(memoryColumnCount, memoryRowCount);
            SetMemoryDataGridRowHeight();
        }

        // DataGridの行数を指定して高さを設定
        private void SetMemoryDataGridRowHeight()
        {
            // DataGrid の ColumnHeader の高さを取得
            var headersPresenter = General.FindVisualChild<DataGridColumnHeadersPresenter>(MemoryDataGrid);
            if (headersPresenter != null)
            {
                double headerHeight = headersPresenter.ActualHeight;
                double totalHeight = MemoryDataGrid.ActualHeight;

                // 利用可能な高さから行数分で割る
                double availableHeight = totalHeight - headerHeight;
                MemoryDataGrid.RowHeight = availableHeight / memoryRowCount;
            }
        }

        // メモリの開始アドレスを格納する変数
        private uint memoryStartAddress = 0x10000000;

        // 無限ループを防ぐためのフラグ
        private bool isMemoryScrollBarValueChangedByTextBox = false;
        private bool isMemoryStartAddressTextChangedByScrollBar = false;

        // テキストボックスから開始アドレスを変更した際に呼ばれる
        private void MemoryStartAddressTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // テキストをユーザーが直接操作したかどうかを判断
            if (isMemoryStartAddressTextChangedByScrollBar) return;

            // フラグを有効化
            isMemoryScrollBarValueChangedByTextBox = true;

            // テキストボックスの値をスクロールバーに反映
            MemoryScrollBar.Value = ConvertEx.ToUInt32(MemoryStartAddressTextBox.Text);
            OnMemoryStartAddressChanged();

            // フラグをリセット
            isMemoryScrollBarValueChangedByTextBox = false;
        }

        // スクロールバーで開始アドレスを変更した際に呼ばれる
        private void MemoryScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // スクロールバーの値をユーザーが直接操作したかどうかを判断
            if (isMemoryScrollBarValueChangedByTextBox) return;

            // フラグを有効化
            isMemoryStartAddressTextChangedByScrollBar = true;

            // スクロールバーの値をテキストボックスに反映
            uint address = (uint)(e.NewValue);
            MemoryStartAddressTextBox.Text = $"{address:X8}";
            OnMemoryStartAddressChanged();

            // フラグをリセット
            isMemoryStartAddressTextChangedByScrollBar = false;
        }

        // 開始アドレスを変更した際に呼ぶメソッド
        private void OnMemoryStartAddressChanged()
        {
            memoryStartAddress = ConvertEx.ToUInt32(MemoryStartAddressTextBox.Text);
        }

        // MemoryTabが選択されているかどうかをチェックするメソッド
        private bool IsMemoryTabSelected()
        {
            bool flag = false;
            Dispatcher.Invoke(() =>
            {
                flag = MainTab.SelectedItem == MemoryTab;
            });
            return flag;
        }

        private void GenerateMemoryCell(int columnCount, int rowCount)
        {
            DataTable table = new DataTable();

            // 列の追加
            table.Columns.Add($"Address", typeof(string));  // アドレス
            for (int col = 0; col < columnCount; col++)
            {
                table.Columns.Add($"+{(col * 4):X}", typeof(string));  // 値(可変長)
            }

            // 行の追加
            for (int row = 0; row < rowCount; row++)
            {
                table.Rows.Add(table.NewRow());
            }

            // DataGrid にバインド
            MemoryDataGrid.ItemsSource = table.DefaultView;
        }

        private async Task UpdateMemory()
        {
            // 自動更新が無効な場合はスキップ
            if (!_isMemoryUpdate) return;

            // 接続されていない場合はスキップ
            if (_socket == null || !_socket.Connected) return;

            // メモリタブが選択されていない場合はスキップ
            if (!IsMemoryTabSelected()) return;

            uint address = memoryStartAddress;
            int size = memoryColumnCount * memoryRowCount * 4;

            uint[] result = await Command.ReadUInt32s(_socket, address, size);
            UpdateMemoryCells(address, result);
        }

        private DataTable? GetMemoryTable()
        {
            DataTable? table = null;
            // UIスレッドで一括更新
            Dispatcher.Invoke(() =>
            {
                if (MemoryDataGrid.ItemsSource is DataView dataView)
                {
                    table = dataView.Table;
                }
            });
            return table;
        }

        private void UpdateMemoryCells(uint address, uint[] result)
        {
            // メモリテーブルを取得
            var table = GetMemoryTable();
            if (table == null)
            {
                // メモリテーブルが取得できない場合は処理を終了
                Debug.WriteLine("Memory table is null.");
                return;
            }

            Dispatcher.Invoke(() =>
            {
                int rowCount = memoryRowCount; // 行数
                int colCount = memoryColumnCount; // 列数

                // 行数分だけループ
                for (int row = 0; row < rowCount; row++)
                {
                    // アドレスを表示
                    table.Rows[row][0] = $"{address + (uint)(row * memoryColumnCount * 4):X8}";

                    // 列数分だけループ
                    for (int col = 0; col < colCount; col++)
                    {
                        int resultIndex = row * colCount + col;

                        if (resultIndex < result.Length)
                        {
                            table.Rows[row][col + 1] = $"{result[resultIndex]:X8}";
                        }
                        else
                        {
                            table.Rows[row][col + 1] = "--------";
                        }
                    }
                }
            });
        }
    }
}
