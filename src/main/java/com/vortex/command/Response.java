package com.vortex.command;

public enum Response
{
    Failure(0x0),
    Success(0x1),
    Progress(0x2),
    Data(0x3);

    private final int value;
    Response(int value)
    {
        this.value = value;
    }

    public int getValue()
    {
        return value;
    }
}
