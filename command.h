#ifndef COMMAND_H
#define COMMAND_H

#include <QWidget>

namespace Command
{
    enum class ID : quint32
    {
        // メモリ読み書き
        WriteMemory = 0x0,
        ReadMemory = 0x1,

        // メモリサーチ
        DumpForSearch = 0x10,
        SpecificSearch = 0x11,
        UnknownSearch = 0x12,
        ClearSearchFile = 0x13,
        CheckSearchFile = 0x14,

        // 応答要求(切断検出用)
        Respond = 0x1000,

        // ダミー
        Dummy = 0xFFFFFFFF
    };

    enum class Comparison : quint32
    {
        Equal = 0,         // ==
        NotEqual = 1,      // !=
        LessThan = 2,      // <
        LessOrEqual = 3,   // <=
        GreaterThan = 4,   // >
        GreaterOrEqual = 5 // >=
    };

    enum class ValueType : quint32
    {
        Int8   = 0,
        UInt8  = 1,
        Int16  = 2,
        UInt16 = 3,
        Int32  = 4,
        UInt32 = 5,
        Int64  = 6,
        UInt64 = 7,
        Float  = 8,
        Double = 9,
        Unknown = 10
    };

    enum class Response : quint32
    {
        Failure = 0,
        Success = 1,
        Progress = 2,
        Data
    };
}

#endif // COMMAND_H
