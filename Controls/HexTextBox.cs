using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Text.RegularExpressions;

namespace Vortex.Controls
{
    public class HexTextBox : TextBox
    {
        private static readonly Regex HexRegex = new Regex("^[0-9a-fA-F]+$", RegexOptions.Compiled);

        protected override Type StyleKeyOverride => typeof(TextBox);

        protected override void OnTextInput(TextInputEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Text) || !HexRegex.IsMatch(e.Text))
            {
                e.Handled = true; // 無効な文字は入力させない
                return;
            }

            var upperText = e.Text.ToUpperInvariant();

            var selectionStart = SelectionStart;
            var selectionEnd = SelectionEnd;

            var currentText = Text ?? string.Empty;

            var newText = currentText.Remove(selectionStart, selectionEnd - selectionStart).Insert(selectionStart, upperText);

            Text = newText;

            CaretIndex = selectionStart + upperText.Length;

            e.Handled = true; // 入力イベントを処理済みにする
        }
    }
}
