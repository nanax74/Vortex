package com.vortex.controller;

import com.vortex.data.PacketDispatcher;
import com.vortex.data.ReadablePacket;
import javafx.fxml.FXML;
import javafx.scene.control.TextArea;

import java.io.IOException;

public class LoggerTabController implements PacketDispatcher.Listener
{
    public void onPacketReceived(ReadablePacket packet) throws IOException
    {

    }

    @FXML
    private void initialize()
    {
        PacketDispatcher.addListener(this);
    }

    @FXML private TextArea logArea;

    @FXML
    private void onClearClicked()
    {
        logArea.clear();
    }

    public void append(String msg)
    {
        logArea.appendText(msg + "\n");
    }
}
