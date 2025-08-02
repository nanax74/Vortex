package com.vortex.data;

import javafx.beans.property.IntegerProperty;
import javafx.beans.property.SimpleIntegerProperty;

public class SearchResult
{
    private final IntegerProperty address = new SimpleIntegerProperty();
    private final IntegerProperty value = new SimpleIntegerProperty();

    public SearchResult(int address, int value)
    {
        this.address.set(address);
        this.value.set(value);
    }

    // Property アクセス（TableView に必要）
    public IntegerProperty addressProperty()
    {
        return address;
    }

    public IntegerProperty valueProperty()
    {
        return value;
    }

    // 通常の getter/setter
    public int getAddress()
    {
        return address.get();
    }

    public void setAddress(int address)
    {
        this.address.set(address);
    }

    public int getValue()
    {
        return value.get();
    }

    public void setValue(int value)
    {
        this.value.set(value);
    }
}
