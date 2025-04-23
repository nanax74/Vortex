using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace Vortex
{
    public class HexadecimalTextBox : TextBox
    {
        private bool _isComposing = false;

        public HexadecimalTextBox()
        {
            // IME入力の検出
            TextCompositionManager.AddPreviewTextInputStartHandler(this, OnTextInputStart);
            TextCompositionManager.AddPreviewTextInputUpdateHandler(this, OnTextInputUpdate);
            TextCompositionManager.AddPreviewTextInputHandler(this, OnTextInput);

            // テキスト変更時
            TextChanged += OnTextChanged;

            // 貼り付けの検出
            DataObject.AddPastingHandler(this, OnPaste);
        }

        private void OnTextInputStart(object sender, TextCompositionEventArgs e)
        {
            _isComposing = true;
        }

        private void OnTextInputUpdate(object sender, TextCompositionEventArgs e)
        {
            _isComposing = true;
        }

        private void OnTextInput(object sender, TextCompositionEventArgs e)
        {
            _isComposing = false; // IME入力確定後
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isComposing) return; // 変換中は無視

            int caret = SelectionStart;

            string filtered = Regex.Replace(Text, "[^0-9A-Fa-f]", "").ToUpper();

            if (filtered != Text)
            {
                Text = filtered;
                SelectionStart = Math.Min(caret, Text.Length);
            }
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pasted = (string)e.DataObject.GetData(typeof(string));
                if (!Regex.IsMatch(pasted, "^[0-9A-Fa-f]+$"))
                {
                    e.CancelCommand();
                }
            }
        }
    }
}