package com.vortex.data;

import java.io.IOException;
import java.io.OutputStream;
import java.net.Socket;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;

public class Packet
{
    private final Socket socket;
    private final ByteBuffer buffer;

    // 書き込み用コンストラクタ（容量は必要に応じて拡張）
    public Packet(Socket socket)
    {
        this.socket = socket;
        this.buffer = ByteBuffer.allocate(1024); // 初期サイズは適宜調整
        this.buffer.order(ByteOrder.BIG_ENDIAN); // 例：WiiU はBE
    }

    // 読み込み用
    public Packet(Socket socket, byte[] data)
    {
        this.socket = socket;
        this.buffer = ByteBuffer.wrap(data);
        this.buffer.order(ByteOrder.BIG_ENDIAN);
    }

    // ---------- 書き込み ----------
    public void writeInt(int value)
    {
        buffer.putInt(value);
    }

    public void writeByte(byte value)
    {
        buffer.put(value);
    }

    public void writeShort(short value)
    {
        buffer.putShort(value);
    }

    public void writeLong(long value)
    {
        buffer.putLong(value);
    }

    public void writeBytes(byte[] values)
    {
        buffer.put(values);
    }

    // ---------- 読み込み ----------
    public byte readByte()
    {
        return buffer.get();
    }

    public short readShort()
    {
        return buffer.getShort();
    }

    public int readInt()
    {
        return buffer.getInt();
    }

    public long readLong()
    {
        return buffer.getLong();
    }

    public byte[] readBytes(int length)
    {
        byte[] data = new byte[length];
        buffer.get(data);
        return data;
    }

    // 送信
    public void send() throws IOException
    {
        byte[] data = toByteArray();
        int size = data.length;

        ByteBuffer bb = ByteBuffer.allocate(4 + size);
        bb.order(ByteOrder.BIG_ENDIAN);
        bb.putInt(size);
        bb.put(data);

        byte[] fullData = bb.array();

        // 出力ストリームを取得して送信
        OutputStream out = socket.getOutputStream();
        out.write(fullData);
        out.flush();
    }

    // ---------- その他 ----------
    public byte[] toByteArray()
    {
        int pos = buffer.position();
        byte[] result = new byte[pos];
        buffer.rewind();           // 先頭に戻す
        buffer.get(result);       // 書き込んだ内容だけ取得
        buffer.position(pos);     // 元の位置に戻す
        return result;
    }

    public void resetCursor()
    {
        buffer.rewind();
    }
}
