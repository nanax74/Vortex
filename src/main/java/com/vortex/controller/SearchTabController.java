package com.vortex.controller;

import com.vortex.data.PacketDispatcher;
import com.vortex.data.ReadablePacket;
import javafx.fxml.FXML;

public class SearchTabController implements PacketDispatcher.Listener
{
    public void onPacketReceived(ReadablePacket packet)
    {

    }

    @FXML
    private void initialize()
    {
        PacketDispatcher.addListener(this);
    }

    @FXML
    private void onSearchClicked()
    {

    }
}
