package com.vortex.data;

import java.io.*;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;

public class WritablePacket
{
    private final ByteArrayOutputStream outputStream;
    private final DataOutputStream dataOut;

    public static final int SIZE_MAX = 1024 * 1024; // 1MB

    // 新規パケット（空の書き込み用）
    public WritablePacket()
    {
        this.outputStream = new ByteArrayOutputStream();
        this.dataOut = new DataOutputStream(outputStream);
    }

    // バッファから再構築（読み取り専用にはしない設計）
    public WritablePacket(byte[] data)
    {
        this.outputStream = new ByteArrayOutputStream();
        this.dataOut = new DataOutputStream(outputStream);
        try
        {
            this.outputStream.write(data);
        }
        catch (IOException e)
        {
            throw new RuntimeException("バッファの書き込みに失敗しました", e);
        }
    }

    public void writeByte(byte value) throws IOException
    {
        dataOut.writeByte(value);
    }

    public void writeShort(short value) throws IOException
    {
        dataOut.writeShort(value);
    }

    public void writeInt(int value) throws IOException
    {
        dataOut.writeInt(value);
    }

    public void writeLong(long value) throws IOException
    {
        dataOut.writeLong(value);
    }

    public void writeFloat(float value) throws IOException
    {
        dataOut.writeFloat(value);
    }

    public void writeDouble(double value) throws IOException
    {
        dataOut.writeDouble(value);
    }

    public void writeByteArray(byte[] data) throws IOException
    {
        if (data == null)
        {
            throw new IllegalArgumentException("data is null");
        }
        dataOut.write(data);
    }

    public void send(OutputStream out) throws IOException
    {
        byte[] body = outputStream.toByteArray();

        if (body.length > SIZE_MAX)
        {
            throw new IOException("パケットサイズが大きすぎます: " + body.length);
        }

        ByteBuffer header = ByteBuffer.allocate(4).order(ByteOrder.BIG_ENDIAN);
        header.putInt(body.length);

        out.write(header.array()); // サイズヘッダー
        out.write(body);           // 本体
        out.flush();
    }

    public byte[] toByteArray()
    {
        return outputStream.toByteArray();
    }
}
