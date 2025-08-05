package com.vortex.controls;

import javafx.scene.control.TextField;
import javafx.scene.control.TextFormatter;

import java.util.function.UnaryOperator;

public class HexTextField extends TextField
{
    private int maxLength = 8; // デフォルトは int (8桁)

    public HexTextField()
    {
        setFormatter();
    }

    public void setMaxLength(int length)
    {
        this.maxLength = length;
        setFormatter();

        // テキストが長すぎる場合は切り詰める
        if (getText().length() > maxLength)
        {
            setText(getText().substring(0, maxLength));
        }
    }

    private void setFormatter()
    {
        UnaryOperator<TextFormatter.Change> filter = change ->
        {
            String newText = change.getControlNewText();
            if (newText.matches("[0-9a-fA-F]*") && newText.length() <= maxLength)
            {
                change.setText(change.getText().toUpperCase());
                return change;
            }
            return null;
        };
        setTextFormatter(new TextFormatter<>(filter));
    }

    private String getNormalizedText()
    {
        String text = getText();
        if (text == null || text.isEmpty())
        {
            return "0";
        }

        // パディング（偶数桁など）
        if (text.length() % 2 != 0)
        {
            text = "0" + text;
        }
        return text;
    }

    public byte getByteValue()
    {
        return (byte) Byte.parseByte(getNormalizedText(), 16);
    }

    public void setByteValue(byte value)
    {
        setText(String.format("%02X", value));
    }

    public short getShortValue()
    {
        return (short) Short.parseShort(getNormalizedText(), 16);
    }

    public void setShortValue(short value)
    {
        setText(String.format("%04X", value));
    }

    public int getIntValue()
    {
        return Integer.parseInt(getNormalizedText(), 16);
    }

    public void setIntValue(int value)
    {
        setText(String.format("%08X", value));
    }

    public long getLongValue()
    {
        return Long.parseLong(getNormalizedText(), 16);
    }

    public void setLongValue(long value)
    {
        setText(String.format("%016X", value));
    }

    public float getFloatValue()
    {
        int bits = getIntValue();
        return Float.intBitsToFloat(bits);
    }

    public void setFloatValue(float value)
    {
        int bits = Float.floatToIntBits(value);
        setText(String.format("%08X", bits));
    }

    public double getDoubleValue()
    {
        long bits = getLongValue();
        return Double.longBitsToDouble(bits);
    }

    public void setDoubleValue(double value)
    {
        long bits = Double.doubleToLongBits(value);
        setText(String.format("%016X", bits));
    }

    public byte[] getBytes()
    {
        String hex = getNormalizedText();
        int len = hex.length();
        byte[] result = new byte[len / 2];
        for (int i = 0; i < len; i += 2)
        {
            result[i / 2] = (byte) Integer.parseInt(hex.substring(i, i + 2), 16);
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
