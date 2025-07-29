package com.vortex;

import com.vortex.data.Packet;

import java.io.*;
import java.net.InetSocketAddress;
import java.net.Socket;
import java.nio.ByteBuffer;
import java.util.ArrayList;
import java.util.List;

public class Client
{
    private Socket socket;
    private BufferedInputStream in;
    private BufferedOutputStream out;
    private final ByteArrayOutputStream receiveBuffer = new ByteArrayOutputStream();
    private final ByteArrayOutputStream sendBuffer = new ByteArrayOutputStream();

    public void connect(String host, int port, int timeoutMillis) throws IOException
    {
        try
        {
            disconnect();

            socket = new Socket();
            socket.connect(new InetSocketAddress(host, port), timeoutMillis); // タイムアウト指定
            in = new BufferedInputStream(socket.getInputStream());
            out = new BufferedOutputStream(socket.getOutputStream());
        }
        catch (IOException e)
        {
            socket = null;
            in = null;
            out = null;
            throw new IOException(e);
        }
    }

    public void disconnect() throws IOException
    {
        try
        {
            if (in != null)
            {
                in.close();
            }

            if (out != null)
            {
                out.close();
            }

            if (socket != null && !socket.isClosed())
            {
                socket.close();
            }
        }
        catch (IOException e)
        {
            throw new IOException(e);
        }

        socket = null;
        in = null;
        out = null;
    }

    // 届いたパケットを全て受け取る
    public List<Packet> receivePackets() throws IOException
    {
        byte[] readBuffer = new byte[1024];
        int bytesRead = in.read(readBuffer);
        if (bytesRead == -1)
        {
            throw new EOFException("Connection closed by server");
        }

        // 読み取ったデータを内部バッファに追加
        receiveBuffer.write(readBuffer, 0, bytesRead);

        // パケット抽出用
        List<Packet> packets = new ArrayList<>();
        byte[] currentBuffer = receiveBuffer.toByteArray();
        int offset = 0;

        while (currentBuffer.length - offset >= 4)
        {
            ByteBuffer bb = ByteBuffer.wrap(currentBuffer, offset, 4);
            int packetSize = bb.getInt();

            if (currentBuffer.length - offset - 4 < packetSize)
            {
                // パケットがまだ全部届いていない
                break;
            }

            byte[] packetData = new byte[packetSize];
            System.arraycopy(currentBuffer, offset + 4, packetData, 0, packetSize);

            packets.add(new Packet(socket, packetData));

            offset += 4 + packetSize;
        }

        // 残った未処理のデータを buffer に残す
        byte[] remaining = new byte[currentBuffer.length - offset];
        System.arraycopy(currentBuffer, offset, remaining, 0, remaining.length);
        receiveBuffer.reset();
        receiveBuffer.write(remaining);

        return packets;
    }

    public Packet getPacket(byte[] data)
    {
        Packet p = new Packet(socket);
        p.writeBytes(data);
        return p;
    }
}
