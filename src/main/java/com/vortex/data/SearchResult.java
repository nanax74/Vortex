package com.vortex.data;

import com.vortex.command.ValueType;
import javafx.beans.binding.Bindings;
import javafx.beans.binding.StringBinding;
import javafx.beans.property.IntegerProperty;
import javafx.beans.property.LongProperty;
import javafx.beans.property.SimpleIntegerProperty;
import javafx.beans.property.SimpleLongProperty;

public class SearchResult
{
    private final IntegerProperty address = new SimpleIntegerProperty();
    private final LongProperty value = new SimpleLongProperty();

    public SearchResult(int address, long value)
    {
        setAddress(address);
        setValue(value);
    }

    // Address
    public int getAddress()
    {
        return address.get();
    }

    public void setAddress(int address)
    {
        this.address.set(address);
    }

    public IntegerProperty addressProperty()
    {
        return address;
    }

    // Value
    public long getValue()
    {
        return value.get();
    }

    public void setValue(long value)
    {
        this.value.set(value);
    }

    public LongProperty valueProperty()
    {
        return value;
    }

    // ---- Formatted Getters ----

    public String getHexValue(ValueType type)
    {
        return switch (type)
        {
            case Int8, UInt8 -> String.format("%02X", (byte) getValue());
            case Int16, UInt16 -> String.format("%04X", (short) getValue());
            case Int32, UInt32, Float -> String.format("%08X", (int) getValue());
            case Int64, UInt64, Double -> String.format("%016X", getValue());
            default -> "";
        };
    }

    public float getFloatValue()
    {
        return Float.intBitsToFloat((int) getValue());
    }

    public double getDoubleValue()
    {
        return Double.longBitsToDouble(getValue());
    }
}
