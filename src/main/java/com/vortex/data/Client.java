package com.vortex.data;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.InetSocketAddress;
import java.net.Socket;

public class Client
{
    private Socket socket;
    private InputStream input;
    private OutputStream output;

    private final Object readLock = new Object();
    private final Object writeLock = new Object();

    /**
     * タイムアウト付きで指定ホスト・ポートへ接続します。
     *
     * @param host          接続先ホスト
     * @param port          ポート番号
     * @param timeoutMillis タイムアウト（ミリ秒）
     * @throws IOException 接続に失敗した場合
     */
    public void connect(String host, int port, int timeoutMillis) throws IOException
    {
        disconnect(); // 既に接続されている場合は切断

        socket = new Socket();
        socket.connect(new InetSocketAddress(host, port), timeoutMillis);
        input = socket.getInputStream();
        output = socket.getOutputStream();
    }

    /**
     * パケットを送信します。
     */
    public void sendPacket(WritablePacket packet) throws IOException
    {
        synchronized (writeLock)
        {
            if (!isConnected())
            {
                throw new IOException("接続されていません");
            }
            packet.send(output);
        }
    }

    /**
     * パケットを受信します。
     */
    public ReadablePacket receivePacket() throws IOException
    {
        synchronized (readLock)
        {
            if (!isConnected())
            {
                throw new IOException("接続されていません");
            }
            return ReadablePacket.receive(input);
        }
    }

    /**
     * 接続を閉じます。
     */
    public void disconnect() throws IOException
    {
        try
        {
            if (socket != null)
            {
                socket.close();
            }
        }
        finally
        {
            socket = null;
            input = null;
            output = null;
        }
    }

    /**
     * 現在接続されているかどうかを返します。
     */
    public boolean isConnected()
    {
        return socket != null && socket.isConnected() && !socket.isClosed();
    }
}
