package com.vortex.command;

public enum Bool
{
    False(0x0),
    True(0x1);

    private final int value;

    Bool(int value)
    {
        this.value = value;
    }

    public int getValue()
    {
        return value;
    }
}
