package com.vortex.controller;

import com.vortex.Client;
import javafx.fxml.FXML;
import javafx.fxml.FXMLLoader;
import javafx.scene.control.Tab;
import javafx.scene.control.TabPane;
import javafx.scene.layout.VBox;

import java.io.IOException;

public class MainController
{
    @FXML private TabPane tabPane;

    @FXML private Tab connectionTab;

    @FXML
    private void initialize() throws IOException
    {
        FXMLLoader loader;
        VBox content;

        loader = new FXMLLoader(getClass().getResource("/connectionTab.fxml"));
        content = loader.load();
        connectionTab.setContent(content);
    }
}
