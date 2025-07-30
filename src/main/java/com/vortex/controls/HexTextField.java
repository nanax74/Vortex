package com.vortex.controls;

import javafx.scene.control.TextField;
import javafx.scene.control.TextFormatter;

import java.util.function.UnaryOperator;

public class HexTextField extends TextField
{
    public HexTextField()
    {
        UnaryOperator<TextFormatter.Change> filter = change ->
        {
            String newText = change.getControlNewText();
            if (newText.matches("[0-9a-fA-F]*"))
            {
                // すべて大文字に変換（任意）
                change.setText(change.getText().toUpperCase());
                return change;
            }
            return null;
        };
        setTextFormatter(new TextFormatter<>(filter));
    }

    public int getIntValue()
    {
        return getText().isEmpty() ? 0 : Integer.parseInt(getText(), 16);
    }

    public void setIntValue(int value)
    {
        setText(String.format("%X", value));
    }

    public byte[] getBytes()
    {
        String text = getText();
        if (text.length() % 2 != 0)
        {
            // 偶数桁に調整（例: "F" → "0F"）
            text = "0" + text;
        }

        int len = text.length();
        byte[] result = new byte[len / 2];
        for (int i = 0; i < len; i += 2)
        {
            String byteStr = text.substring(i, i + 2);
            result[i / 2] = (byte) Integer.parseInt(byteStr, 16);
        }
        return result;
    }

    public void setBytes(byte[] bytes)
    {
        StringBuilder sb = new StringBuilder();
        for (byte b : bytes)
        {
            sb.append(String.format("%02X", b));
        }
        setText(sb.toString());
    }
}
