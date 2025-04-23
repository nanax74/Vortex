using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;
using Gee.External.Capstone;
using Gee.External.Capstone.PowerPc;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Net;

namespace Vortex
{
    public partial class MainWindow : Window
    {
        private int disasmRowCount = 0x100;
        private void DisasmDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var table = GetDisasmTable();
            if (table == null || table.Rows.Count != disasmRowCount)
            {
                GenerateDisassemblerCell(disasmRowCount);
            }
        }

        private void DisasmAddressTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateDisassembler();
            }
        }

        private void DisasmDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var table = GetDisasmTable();
            if (table != null)
            {
                int selectedRow = DisasmDataGrid.SelectedIndex;
                DisasmTargetAddressTextBox.Text = table.Rows[selectedRow][0].ToString() ?? string.Empty;
                DisasmInstructionTextBox.Text = table.Rows[selectedRow][2].ToString() ?? string.Empty;
            }
        }

        private async void DisasmInstructionAssembleButton_Click(object sender, RoutedEventArgs e)
        {
            string instruction = DisasmInstructionTextBox.Text;
            if (_socket != null && !string.IsNullOrEmpty(instruction))
            {
                // null命令を処理するために、"null"を".long 0"に変換
                if (instruction == "null") instruction = ".long 0";

                // 書き換える対象のアドレスを取得
                string addressString = DisasmTargetAddressTextBox.Text;
                uint address = ConvertEx.ToUInt32(addressString);

                // 絶対参照を相対参照に書き換える
                instruction = Assembler.RewriteBranchToRelative(instruction, address);

                // アセンブリ命令があるかどうかをチェック
                if (instruction != "")
                {
                    // アセンブル
                    string machineCodeString;
                    string errorCode;
                    (machineCodeString, errorCode) = await Assembler.AssemblePowerPC(instruction);

                    // 書き込み
                    if (errorCode == "")
                    {
                        machineCodeString = machineCodeString.Replace(" ", "").Replace("\n", "");
                        uint[] machineCode = ConvertEx.ToUInt32Array(machineCodeString);

                        // 書き込み
                        await Command.WriteKernel(_socket, address, machineCode);
                        UpdateDisassembler();
                    }
                    else
                    {
                        Debug.WriteLine($"Error: {errorCode}");
                    }
                }
            }
        }

        private void GenerateDisassemblerCell(int rowCount)
        {
            DataTable table = new DataTable();

            // 列の追加
            table.Columns.Add($"Address", typeof(string));  // 文字列型の列
            table.Columns.Add($"Binary Code", typeof(string));  // 文字列型の列
            table.Columns.Add($"Instruction", typeof(string));  // 文字列型の列

            // 行の追加
            for (int row = 0; row < rowCount; row++)
            {
                table.Rows.Add(table.NewRow());
            }

            // DataGrid にバインド
            DisasmDataGrid.ItemsSource = table.DefaultView;
        }

        SemaphoreSlim _semaphoreUpdateDisasm = new SemaphoreSlim(1);

        private async void UpdateDisassembler()
        {
            if (_socket == null || !_socket.Connected)
            {
                Debug.WriteLine("Not connected to server");
                return;
            }

            _semaphoreUpdateDisasm.Wait();

            try
            {
                uint address = ConvertEx.ToUInt32(DisasmStartAddressTextBox.Text);

                byte[] binaryCode = await Command.ReadBytesBigEndian(_socket, address, disasmRowCount * 4);

                UpdateDisassemblerCells(binaryCode, address);
            }
            finally
            {
                _semaphoreUpdateDisasm.Release();
            }
        }

        private DataTable? GetDisasmTable()
        {
            DataTable? table = null;
            // UIスレッドで一括更新
            Dispatcher.Invoke(() =>
            {
                if (DisasmDataGrid.ItemsSource is DataView dataView)
                {
                    table = dataView.Table;
                }
            });
            return table;
        }

        private void UpdateDisassemblerCells(byte[] binaryCode, uint address)
        {
            var table = GetDisasmTable();

            if (table == null)
            {
                Debug.WriteLine("Disassembler table is null");
                return;
            }

            var disassembler = CapstoneDisassembler.CreatePowerPcDisassembler(PowerPcDisassembleMode.Bit32);
            disassembler.EnableInstructionDetails = true; // これを入れるのがベスト

            int i = 0;
            while (i < disasmRowCount * 4)
            {
                if (binaryCode.Length >= i + 4)
                {
                    // 4バイトを1命令として逆アセンブルを行う
                    var instructionBytes = binaryCode.Skip(i).Take(4).ToArray();
                    var binaryString = BitConverter.ToString(ConvertEx.ReverseEndianPerBlock(instructionBytes, 4)).Replace("-", "");
                    var instruction = disassembler.Disassemble(instructionBytes, address + (uint)i);

                    int index = i / 4;

                    table.Rows[index][0] = $"{(address + (uint)i):X8}"; // アドレスを更新
                    table.Rows[index][1] = binaryString; // バイナリコードを更新

                    if (BitConverter.ToUInt32(instructionBytes, 0) == 0)
                    {
                        table.Rows[index][2] = "null";

                    }
                    else if (instruction.Length == 0)
                    {
                        uint value = BitConverter.ToUInt32(instructionBytes, 0);
                        table.Rows[index][2] = $".long 0x{value:X8}";
                    }
                    else
                    {
                        // 有効な命令はアセンブリ命令として表示
                        var ins = instruction[0];

                        // オペランドを大文字に変換（a-fのみ）
                        string operands = ins.Operand.ToString();
                        operands = Regex.Replace(operands, @"[a-f]", m => m.Value.ToUpper());

                        string instructionString;
                        if (operands == string.Empty)
                        {
                            instructionString = $"{ins.Mnemonic}";
                        }
                        else
                        {
                            instructionString = $"{ins.Mnemonic} {operands}";
                        }
                        table.Rows[index][2] = instructionString;
                    }
                }
                else
                {
                    // バイナリコードが不足している場合は空白を設定
                    int index = i / 4;
                    table.Rows[index][0] = $"{(address + (uint)i):X8}"; // アドレスを更新
                    table.Rows[index][1] = "--------";
                    table.Rows[index][2] = "None";
                }
                // 1命令分進める
                i += 4;
            }
        }
    }
}