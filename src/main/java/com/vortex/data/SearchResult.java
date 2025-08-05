package com.vortex.data;

import javafx.beans.property.*;

public class SearchResult
{
    private final IntegerProperty address = new SimpleIntegerProperty();
    private final StringProperty hexValue = new SimpleStringProperty("0");

    public SearchResult(int address, int value)
    {
        this.address.set(address);
        setInt(value);
    }

    // Address
    public IntegerProperty addressProperty()
    {
        return address;
    }

    public int getAddress()
    {
        return address.get();
    }

    public void setAddress(int address)
    {
        this.address.set(address);
    }

    // Hex Value Property (バインディング向け)
    public StringProperty hexValueProperty()
    {
        return hexValue;
    }

    public String getHexValue()
    {
        return hexValue.get();
    }

    public void setHexValue(String hex)
    {
        this.hexValue.set(normalize(hex));
    }

    // ---- Getter ----
    public byte getByte()
    {
        return (byte) Byte.parseByte(normalize(hexValue.get()), 16);
    }

    public short getShort()
    {
        return (short) Short.parseShort(normalize(hexValue.get()), 16);
    }

    public int getInt()
    {
        return Integer.parseInt(normalize(hexValue.get()), 16);
    }

    public long getLong()
    {
        return Long.parseLong(normalize(hexValue.get()), 16);
    }

    public float getFloat()
    {
        return Float.intBitsToFloat(getInt());
    }

    public double getDouble()
    {
        return Double.longBitsToDouble(getLong());
    }

    // ---- Setter ----
    public void setByte(byte value)
    {
        setHexValue(String.format("0x%02X", value));
    }

    public void setShort(short value)
    {
        setHexValue(String.format("0x%04X", value));
    }

    public void setInt(int value)
    {
        setHexValue(String.format("0x%08X", value));
    }

    public void setLong(long value)
    {
        setHexValue(String.format("0x%016X", value));
    }

    public void setFloat(float value)
    {
        setInt(Float.floatToIntBits(value));
    }

    public void setDouble(double value)
    {
        setLong(Double.doubleToLongBits(value));
    }

    // ---- Utility ----
    private String normalize(String hex)
    {
        if (hex == null || hex.isEmpty())
        {
            return "0";
        }
        hex = hex.replaceAll("[^0-9a-fA-F]", "");
        return hex.toUpperCase();
    }
}
