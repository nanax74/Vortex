package com.vortex.data;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

public class PacketDispatcher
{
    public interface Listener
    {
        void onPacketReceived(ReadablePacket packet) throws IOException;
    }

    private static final List<Listener> listeners = new ArrayList<>();

    public static synchronized void addListener(Listener listener)
    {
        listeners.add(listener);
    }

    public static synchronized void removeListener(Listener listener)
    {
        listeners.remove(listener);
    }

    public static synchronized void dispatch(ReadablePacket packet) throws IOException
    {
        for (Listener listener : listeners)
        {
            listener.onPacketReceived(packet);
        }
    }
}
