using System;
using System.Globalization;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Vortex.Controls;

public class HexTextBox : TextBox
{
    private bool _isUpdatingInternally;

    public HexTextBox()
    {
        this.TextChanging += OnTextChanging;
        this.TextChanged += OnTextChanged;
    }

    // ===========================
    // uint? HexValue
    // ===========================
    public static readonly DependencyProperty HexValueProperty =
        DependencyProperty.Register(
            nameof(HexValue),
            typeof(uint?),
            typeof(HexTextBox),
            new PropertyMetadata(null, OnHexValueChanged));

    public uint? HexValue
    {
        get => (uint?)GetValue(HexValueProperty);
        set => SetValue(HexValueProperty, value);
    }

    private static void OnHexValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var tb = (HexTextBox)d;
        if (tb._isUpdatingInternally) return;

        tb._isUpdatingInternally = true;
        if (e.NewValue is uint value)
        {
            tb.Text = value.ToString("X"); // 大文字16進
        }
        else
        {
            tb.Text = string.Empty;
        }
        tb._isUpdatingInternally = false;
    }

    // ===========================
    // byte[] Bytes
    // ===========================
    public static readonly DependencyProperty BytesProperty =
        DependencyProperty.Register(
            nameof(Bytes),
            typeof(byte[]),
            typeof(HexTextBox),
            new PropertyMetadata(null, OnBytesChanged));

    public byte[]? Bytes
    {
        get => (byte[]?)GetValue(BytesProperty);
        set => SetValue(BytesProperty, value);
    }

    private static void OnBytesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var tb = (HexTextBox)d;
        if (tb._isUpdatingInternally) return;

        tb._isUpdatingInternally = true;
        if (e.NewValue is byte[] arr)
        {
            tb.Text = BitConverter.ToString(arr).Replace("-", "");
        }
        else
        {
            tb.Text = string.Empty;
        }
        tb._isUpdatingInternally = false;
    }

    // ===========================
    // 入力フィルタ + 大文字化
    // ===========================
    private void OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        var text = sender.Text;
        var sb = new StringBuilder(text.Length);

        foreach (var c in text)
        {
            if (IsHexChar(c))
                sb.Append(char.ToUpperInvariant(c));
        }

        var filtered = sb.ToString();
        if (filtered != text)
        {
            var pos = sender.SelectionStart;
            sender.Text = filtered;
            sender.SelectionStart = Math.Min(pos, filtered.Length);
        }
    }

    private static bool IsHexChar(char c) =>
        ('0' <= c && c <= '9') ||
        ('a' <= c && c <= 'f') ||
        ('A' <= c && c <= 'F');

    // ===========================
    // Text → HexValue / Bytes
    // ===========================
    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingInternally) return;

        _isUpdatingInternally = true;
        var text = this.Text;

        if (uint.TryParse(text, NumberStyles.HexNumber, null, out var value))
            HexValue = value;
        else
            HexValue = null;

        var byteCount = text.Length / 2; // 途中までのペアだけ
        if (byteCount > 0)
        {
            try
            {
                var arr = new byte[byteCount];
                for (var i = 0; i < byteCount; i++)
                    arr[i] = Convert.ToByte(text.Substring(i * 2, 2), 16);
                Bytes = arr;
            }
            catch { Bytes = null; }
        }
        else
        {
            Bytes = null;
        }

        _isUpdatingInternally = false;
    }
}
