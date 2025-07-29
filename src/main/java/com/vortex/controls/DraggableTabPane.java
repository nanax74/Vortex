// DraggableTabPane.java
package com.vortex.controls;

import javafx.application.Platform;
import javafx.collections.ListChangeListener;
import javafx.scene.Scene;
import javafx.scene.control.*;
import javafx.scene.input.MouseEvent;
import javafx.scene.layout.StackPane;
import javafx.stage.Stage;
import javafx.stage.StageStyle;

import java.util.*;
import java.util.function.Consumer;

public class DraggableTabPane extends TabPane
{
    private static Tab draggedTab = null;
    private static Stage previewStage = null;
    private static final WeakHashMap<DraggableTabPane, Boolean> allTabPanes = new WeakHashMap<>();
    private static final List<Stage> detachedStages = new ArrayList<>();

    private Consumer<Tab> onTabClosed;
    private boolean isMainPane = false;

    public DraggableTabPane()
    {
        allTabPanes.put(this, true);

        setOnDragDetected(this::startDrag);
        setOnMouseDragged(this::handleDrag);
        setOnMouseReleased(this::handleDrop);

        getTabs().addListener((ListChangeListener<Tab>) change ->
        {
            while (change.next())
            {
                if (change.wasAdded())
                {
                    for (Tab added : change.getAddedSubList())
                    {
                        added.setOnCloseRequest(e ->
                        {
                            if (onTabClosed != null)
                            {
                                onTabClosed.accept(added);
                            }
                        });
                    }
                }

                if (change.wasRemoved())
                {
                    if (getTabs().isEmpty() && !isMainPane())
                    {
                        Stage stage = (Stage) getScene().getWindow();
                        stage.close();
                    }
                }
            }
        });
    }

    private void startDrag(MouseEvent event)
    {
        draggedTab = getSelectionModel().getSelectedItem();

        if (draggedTab != null)
        {
            createPreviewStage(draggedTab, event.getScreenX(), event.getScreenY());
        }
    }

    private void handleDrag(MouseEvent event)
    {
        if (previewStage != null)
        {
            previewStage.setX(event.getScreenX());
            previewStage.setY(event.getScreenY());
        }
    }

    private void handleDrop(MouseEvent event)
    {
        if (draggedTab == null)
        {
            return;
        }

        if (previewStage != null)
        {
            previewStage.close();
            previewStage = null;
        }

        DraggableTabPane targetPane = null;
        for (DraggableTabPane pane : allTabPanes.keySet())
        {
            if (pane.getScene().getWindow().isShowing() && pane.localToScreen(pane.getBoundsInLocal()).contains(event.getScreenX(), event.getScreenY()))
            {
                targetPane = pane;
                break;
            }
        }

        final Tab dragged = draggedTab;

        if (targetPane != null && targetPane != this)
        {
            getTabs().remove(draggedTab);
            targetPane.getTabs().add(draggedTab);
            final DraggableTabPane finalTargetPane = targetPane;
            Platform.runLater(() -> finalTargetPane.getSelectionModel().select(dragged));
        }
        else if (targetPane == this)
        {
            // 同じ TabPane 内で並び替え
            int targetIndex = getInsertionIndex(event.getScreenX());
            int currentIndex = getTabs().indexOf(draggedTab);
            if (targetIndex != currentIndex)
            {
                getTabs().remove(draggedTab);
                if (targetIndex > getTabs().size())
                {
                    getTabs().add(draggedTab);
                }
                else
                {
                    getTabs().add(targetIndex, draggedTab);
                }
                Platform.runLater(() -> getSelectionModel().select(dragged));
            }
        }
        else
        {
            DraggableTabPane newPane = new DraggableTabPane();
            newPane.setMainPane(false);
            newPane.setOnTabClosed(onTabClosed);
            newPane.getTabs().add(draggedTab);

            Stage newStage = getNewStage(event, newPane);
            detachedStages.add(newStage);
            getTabs().remove(draggedTab);

            newStage.show(); // ここでウィンドウを表示
            Platform.runLater(() ->
            {
                newPane.getSelectionModel().select(dragged);
                if (dragged.getContent() != null)
                {
                    dragged.getContent().requestFocus();
                }
            });
        }

        draggedTab = null;
    }

    private Stage getNewStage(MouseEvent event, DraggableTabPane newPane)
    {
        Stage newStage = new Stage();
        newStage.setTitle("Vortex Tab");
        newStage.setScene(new Scene(newPane, 400, 300));
        newStage.setX(event.getScreenX());
        newStage.setY(event.getScreenY());

        newStage.setOnCloseRequest(e ->
        {
            List<Tab> tabsToClose = new ArrayList<>(newPane.getTabs());
            newPane.getTabs().clear(); // ここで change.wasRemoved が反応
            for (Tab tab : tabsToClose)
            {
                if (tab.isClosable() && onTabClosed != null)
                {
                    onTabClosed.accept(tab);
                }
            }
        });

        newStage.show();
        return newStage;
    }

    private void createPreviewStage(Tab tab, double x, double y)
    {
        previewStage = new Stage(StageStyle.UNDECORATED);
        StackPane root = new StackPane(new Label(tab.getText()));
        previewStage.setScene(new Scene(root));
        previewStage.setX(x);
        previewStage.setY(y);
        previewStage.setAlwaysOnTop(true);
        previewStage.show();
    }

    private int getInsertionIndex(double screenX)
    {
        double localX = screenX - localToScreen(0, 0).getX();
        double totalWidth = 0;
        for (int i = 0; i < getTabs().size(); i++)
        {
            totalWidth += 100; // 仮想的なタブ幅
            if (localX < totalWidth)
            {
                return i;
            }
        }
        return getTabs().size();
    }


    public void setOnTabClosed(Consumer<Tab> handler)
    {
        this.onTabClosed = handler;
    }

    public void setMainPane(boolean isMain)
    {
        this.isMainPane = isMain;
    }

    public boolean isMainPane()
    {
        return isMainPane;
    }

    public static void closeAllDetachedStages()
    {
        for (Stage s : detachedStages)
        {
            s.close();
        }
        detachedStages.clear();
    }
}
