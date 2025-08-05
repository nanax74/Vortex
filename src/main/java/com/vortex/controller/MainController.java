package com.vortex.controller;

import com.vortex.controls.DraggableTabPane;
import com.vortex.data.Client;
import javafx.application.Platform;
import javafx.fxml.FXML;
import javafx.fxml.FXMLLoader;
import javafx.scene.control.Menu;
import javafx.scene.control.MenuItem;
import javafx.scene.control.Tab;
import javafx.scene.layout.Priority;
import javafx.scene.layout.VBox;
import javafx.stage.Stage;

import java.io.IOException;

public class MainController
{
    @FXML
    private Menu tabMenu;

    @FXML
    private DraggableTabPane tabPane;

    Tab searchTab;

    @FXML
    private void initialize() throws IOException
    {
        FXMLLoader loader;
        VBox content;
        Client client = new Client();

        loader = new FXMLLoader(getClass().getResource("/connectionTab.fxml"));
        content = loader.load();
        VBox.setVgrow(content, Priority.ALWAYS);
        Tab connectionTab = new Tab("Connect");
        connectionTab.setContent(content);
        connectionTab.setClosable(true);
        ConnectionTabController connectionTabController = loader.getController();

        loader = new FXMLLoader(getClass().getResource("/loggerTab.fxml"));
        content = loader.load();
        VBox.setVgrow(content, Priority.ALWAYS);
        Tab loggerTab = new Tab("Logger");
        loggerTab.setContent(content);
        loggerTab.setClosable(true);
        LoggerTabController loggerTabController = loader.getController();

        loader = new FXMLLoader(getClass().getResource("/searchTab.fxml"));
        content = loader.load();
        VBox.setVgrow(content, Priority.ALWAYS);
        searchTab = new Tab("Search");
        searchTab.setContent(content);
        searchTab.setClosable(true);
        searchTab.setDisable(true);
        SearchTabController searchTabController = loader.getController();

        tabPane.getTabs().addAll(connectionTab, loggerTab, searchTab);
        tabPane.setMainPane(true);
        tabPane.setOnTabClosed(this::onTabClosed);  // 重要

        client.loggerTabController = loggerTabController;
        connectionTabController.loggerTabController = loggerTabController;
        searchTabController.loggerTabController = loggerTabController;

        connectionTabController.mainController = this;

        connectionTabController.client = client;
        searchTabController.client = client;

        searchTab.selectedProperty().addListener((obs, oldVal, newVal) ->
        {
            if (newVal)
            {
                searchTabController.onSelected();
            }
        });

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

    public void onConnected()
    {
        searchTab.setDisable(false);
    }

    public void onDisconnected()
    {
        searchTab.setDisable(true);
    }
}
