package com.vortex.command;

import com.vortex.controller.SearchTabController;

import java.lang.invoke.VarHandle;

public enum ValueType
{
    Int8(0),
    UInt8(1),
    Int16(2),
    UInt16(3),
    Int32(4),
    UInt32(5),
    Int64(6),
    UInt64(7),
    Float(8),
    Double(9),
    Unknown(10);

    private final int value;
    ValueType(int value)
    {
        this.value = value;
    }

    public int getValue()
    {
        return value;
    }
}
