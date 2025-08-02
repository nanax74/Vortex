// DraggableTabPane.java
package com.vortex.controls;

import javafx.application.Platform;
import javafx.collections.ListChangeListener;
import javafx.geometry.Bounds;
import javafx.scene.Node;
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

    private boolean suppressTabClosed = false;

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
                            if (!suppressTabClosed && onTabClosed != null)
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

        double screenX = event.getScreenX();
        DraggableTabPane targetPane = null;

        for (DraggableTabPane pane : allTabPanes.keySet())
        {
            if (pane.getScene().getWindow().isShowing() && pane.localToScreen(pane.getBoundsInLocal()).contains(screenX, event.getScreenY()))
            {
                targetPane = pane;
                break;
            }
        }

        final Tab finalDraggedTab = draggedTab;
        final DraggableTabPane finalTargetPane = targetPane;

        if (targetPane == this)
        {
            // 同じ TabPane にドロップされた場合の並び替え処理
            int fromIndex = getTabs().indexOf(draggedTab);
            int toIndex = getInsertionIndex(screenX);

            if (toIndex == fromIndex || toIndex == fromIndex + 1)
            {
                // 自分自身にドロップしただけなら何もしない
                draggedTab = null;
                return;
            }

            // 並び替え
            getTabs().remove(draggedTab);
            if (toIndex > getTabs().size())
            {
                toIndex = getTabs().size();
            }
            getTabs().add(toIndex, draggedTab);
            Platform.runLater(() -> getSelectionModel().select(finalDraggedTab));
        }
        else if (targetPane != null)
        {
            // 他の TabPane に移動
            suppressTabClosed = true;
            this.getTabs().remove(draggedTab);
            suppressTabClosed = false;
            targetPane.getTabs().add(draggedTab);
            Platform.runLater(() -> finalTargetPane.getSelectionModel().select(finalDraggedTab));
        }
        else
        {
            // 新しいウィンドウに分離
            DraggableTabPane newPane = new DraggableTabPane();
            newPane.setMainPane(false);
            newPane.setOnTabClosed(onTabClosed);
            newPane.getTabs().add(draggedTab);

            Stage newStage = getNewStage(event, newPane);
            detachedStages.add(newStage);
            getTabs().remove(draggedTab);

            newStage.show();

            // ウィンドウ表示後に選択状態にする
            Platform.runLater(() ->
            {
                newPane.getSelectionModel().select(finalDraggedTab);
                if (finalDraggedTab.getContent() != null)
                {
                    finalDraggedTab.getContent().requestFocus();
                }
            });
        }

        draggedTab = null;
    }

    private Stage getNewStage(MouseEvent event, DraggableTabPane newPane)
    {
        Stage newStage = new Stage();
        newStage.setTitle("Vortex Tab");
        newStage.setScene(new Scene(newPane, 854, 480));
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
        int index = 0;

        // タブヘッダ（タブラベル）のノードをすべて取得
        for (Node node : this.lookupAll(".tab"))
        {
            if (!node.isVisible())
            {
                continue; // 非表示のものはスキップ
            }

            Bounds bounds = node.localToScreen(node.getBoundsInLocal());
            if (bounds == null)
            {
                continue; // 表示前だと null の可能性がある
            }

            double midX = (bounds.getMinX() + bounds.getMaxX()) / 2;

            // screenX がこのタブの中央より左ならこの位置に挿入
            if (screenX < midX)
            {
                return index;
            }

            index++;
        }

        return index; // 最後尾
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
