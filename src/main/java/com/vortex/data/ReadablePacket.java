package com.vortex.data;

import java.io.*;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;

public class ReadablePacket
{
    private final byte[] inputData;
    private final ByteArrayInputStream inputStream;
    private final DataInputStream dataIn;

    public static final int SIZE_MAX = 1024 * 1024;

    // バッファから読み取りパケットを生成
    public ReadablePacket(byte[] data)
    {
        if (data == null)
        {
            throw new IllegalArgumentException("data is null");
        }
        this.inputData = data.clone();
        this.inputStream = new ByteArrayInputStream(this.inputData);
        this.dataIn = new DataInputStream(this.inputStream);
    }

    // WritablePacketから生成（直接バッファコピー）
    public ReadablePacket(WritablePacket packet)
    {
        this(packet.toByteArray());
    }

    public static ReadablePacket receive(InputStream in) throws IOException
    {
        byte[] header = new byte[4];
        readFully(in, header);
        int size = ByteBuffer.wrap(header).order(ByteOrder.BIG_ENDIAN).getInt();

        if (size < 0 || size > SIZE_MAX)
        {
            throw new IOException("パケットサイズが不正です: " + size);
        }

        byte[] body = new byte[size];
        readFully(in, body);
        return new ReadablePacket(body);
    }

    private static void readFully(InputStream in, byte[] buffer) throws IOException
    {
        int offset = 0;
        while (offset < buffer.length)
        {
            int bytesRead = in.read(buffer, offset, buffer.length - offset);
            if (bytesRead == -1)
            {
                throw new EOFException("Stream ended unexpectedly");
            }
            offset += bytesRead;
        }
    }

    public byte readByte() throws IOException
    {
        return dataIn.readByte();
    }

    public short readShort() throws IOException
    {
        return dataIn.readShort();
    }

    public int readInt() throws IOException
    {
        return dataIn.readInt();
    }

    public long readLong() throws IOException
    {
        return dataIn.readLong();
    }

    public byte[] readByteArray(int length) throws IOException
    {
        byte[] buffer = new byte[length];
        int offset = 0;
        while (offset < length)
        {
            int read = dataIn.read(buffer, offset, length - offset);
            if (read == -1)
            {
                throw new EOFException("Not enough data");
            }
            offset += read;
        }
        return buffer;
    }

    public byte[] toByteArray()
    {
        return inputData.clone();
    }
}
