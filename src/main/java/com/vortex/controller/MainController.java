package com.vortex.controller;

import com.vortex.controls.DraggableTabPane;
import javafx.application.Platform;
import javafx.fxml.FXML;
import javafx.fxml.FXMLLoader;
import javafx.scene.control.Menu;
import javafx.scene.control.MenuItem;
import javafx.scene.control.Tab;
import javafx.scene.layout.VBox;
import javafx.stage.Stage;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

public class MainController
{
    @FXML
    private Menu tabMenu;

    @FXML
    private DraggableTabPane tabPane;

    @FXML
    private void initialize() throws IOException
    {
        FXMLLoader loader;
        VBox content;

        loader = new FXMLLoader(getClass().getResource("/connectionTab.fxml"));
        content = loader.load();
        Tab connectionTab = new Tab("Connect");
        connectionTab.setContent(content);
        connectionTab.setClosable(true);
        ConnectionTabController connectionTabController = loader.getController();

        loader = new FXMLLoader(getClass().getResource("/loggerTab.fxml"));
        content = loader.load();
        Tab loggerTab = new Tab("Logger");
        loggerTab.setContent(content);
        loggerTab.setClosable(true);
        LoggerTabController loggerTabController = loader.getController();

        tabPane.getTabs().addAll(connectionTab, loggerTab);
        tabPane.setMainPane(true);
        tabPane.setOnTabClosed(this::onTabClosed);  // 重要

        connectionTabController.loggerTabController = loggerTabController;

        Platform.runLater(() ->
        {
            Stage stage = (Stage) tabPane.getScene().getWindow();
            stage.setOnCloseRequest(e ->
            {
                DraggableTabPane.closeAllDetachedStages();
            });
        });
    }

    private void onTabClosed(Tab tab)
    {
        MenuItem item = new MenuItem(tab.getText());
        item.setOnAction(e ->
        {
            tabPane.getTabs().add(tab);
            tabPane.getSelectionModel().select(tab);
            tabMenu.getItems().remove(item);
        });
        tabMenu.getItems().add(item);
    }
}
