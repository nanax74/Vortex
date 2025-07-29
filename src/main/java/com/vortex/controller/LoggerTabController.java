package com.vortex.controller;

import javafx.fxml.FXML;
import javafx.scene.control.TextArea;

public class LoggerTabController
{
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
