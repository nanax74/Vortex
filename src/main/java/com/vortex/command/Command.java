package com.vortex.command;

public enum Command
{
    // メモリ読み書き
    WriteMemory(0x0),
    ReadMemory(0x1),
    ReadMultipleMemory(0x2),

    // メモリサーチ
    DumpForSearch(0x10),
    SpecificSearch(0x11),
    UnknownSearch(0x12),
    ClearSearchFile(0x13),
    CheckSearchFile(0x14),
    GetSearchResults(0x15),
    GetSearchResultsCount(0x16),
    SubscribeAddress(0x17),
    UnsubscribeAddress(0x18),

    // Server -> Client
    PushAddressValue(0x10000),

    // 応答要求(切断検出用)
    Respond(0x1000),

    // ダミー
    Dummy(0xFFFFFFFF);

    private final int value;
    Command(int value)
    {
        this.value = value;
    }

    public int getValue()
    {
        return value;
    }
}
