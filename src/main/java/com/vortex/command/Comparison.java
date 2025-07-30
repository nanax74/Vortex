package com.vortex.command;

public enum Comparison
{
    Equal(0),         // ==
    NotEqual(1),      // !=
    LessThan(2),      // <
    LessOrEqual(3),   // <=
    GreaterThan(4),   // >
    GreaterOrEqual(5); // >=

    private final int value;
    Comparison(int value)
    {
        this.value = value;
    }

    public int getValue()
    {
        return value;
    }
}
