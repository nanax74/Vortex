package com.vortex.controller;

import com.vortex.data.Client;
import javafx.application.Platform;

public class MemoryTabController
{
    public LoggerTabController loggerTabController;

    public void log(String msg)
    {
        Platform.runLater(() -> loggerTabController.append(msg));
    }

    public Client client;
}
