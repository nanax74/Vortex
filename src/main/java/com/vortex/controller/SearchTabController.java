package com.vortex.controller;

import com.vortex.command.*;
import com.vortex.controls.HexTextField;
import com.vortex.data.*;
import javafx.animation.Timeline;
import javafx.application.Platform;
import javafx.collections.FXCollections;
import javafx.collections.ObservableList;
import javafx.fxml.FXML;
import javafx.scene.control.*;
import javafx.scene.control.Button;
import javafx.scene.control.Label;

import java.awt.*;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

import static java.lang.String.format;

public class SearchTabController implements PacketDispatcher.Listener
{
    public LoggerTabController loggerTabController;

    public void log(String msg)
    {
        Platform.runLater(() -> loggerTabController.append(msg));
    }

    public Client client;

    private Bool isExistFile = Bool.False;

    ObservableList<SearchResult> searchResults = FXCollections.observableArrayList();
    private final List<SearchResult> tempResults = new ArrayList<>();

    private long searchStartTime = 0L;
    private Timeline stopwatchTimeline;

    @FXML
    private ComboBox<SearchType> searchTypeComboBox;

    @FXML
    private HexTextField startAddressField;

    @FXML
    private HexTextField endAddressField;

    @FXML
    private ComboBox<ValueType> valueTypeComboBox;

    @FXML
    private ComboBox<Comparison> comparisonComboBox;

    @FXML
    private Label timeLabel;

    @FXML
    private Label resultLabel;

    @FXML
    private TableView<SearchResult> searchResultsView;

    @FXML
    private TableColumn<SearchResult, Integer> addressColumn;

    @FXML
    private TableColumn<SearchResult, Integer> valueColumn;

    @FXML
    private Button searchButton;

    @FXML
    private ProgressBar searchProgressBar;

    @FXML
    private Label searchProgressLabel;

    @FXML
    private void initialize()
    {
        PacketDispatcher.addListener(this);

        searchTypeComboBox.getItems().addAll(SearchType.Specific, SearchType.Unknown);
        searchTypeComboBox.setValue(SearchType.Specific);

        valueTypeComboBox.getItems().addAll(ValueType.values());
        valueTypeComboBox.setValue(ValueType.Float);

        comparisonComboBox.getItems().addAll(Comparison.values());
        comparisonComboBox.setValue(Comparison.Equal);

        addressColumn.setCellValueFactory(cellData -> cellData.getValue().addressProperty().asObject());
        valueColumn.setCellValueFactory(cellData -> cellData.getValue().valueProperty().asObject());

        addressColumn.setCellFactory(column -> new TableCell<>()
        {
            @Override
            protected void updateItem(Integer item, boolean empty)
            {
                super.updateItem(item, empty);
                setText(empty || item == null ? null : String.format("0x%08X", item));
            }
        });

        valueColumn.setCellFactory(column -> new TableCell<>()
        {
            @Override
            protected void updateItem(Integer item, boolean empty)
            {
                super.updateItem(item, empty);
                setText(empty || item == null ? null : String.format("0x%08X", item));
            }
        });

        searchResultsView.setItems(searchResults);

        stopwatchTimeline = new Timeline(new javafx.animation.KeyFrame(javafx.util.Duration.millis(10), // 0.1秒ごとに更新
                e -> updateStopwatchLabel()));
        stopwatchTimeline.setCycleCount(Timeline.INDEFINITE); // 無限に繰り返す
    }

    private void updateStopwatchLabel()
    {
        long now = System.nanoTime();
        double elapsed = (now - searchStartTime) / 1_000_000_000.0;
        Platform.runLater(() -> timeLabel.setText(String.format("Time: %.2fs", elapsed)));
    }

    public void onPacketReceived(ReadablePacket packet)
    {
        try
        {
            // コマンドID
            int commandId = packet.readInt();
            if (commandId == Command.DumpForSearch.getValue())
            {
                int responseId = packet.readInt();
                if (responseId == Response.Progress.getValue())
                {
                    float progress = packet.readFloat();
                    Platform.runLater(() ->
                    {
                        searchProgressBar.setProgress(progress);
                        searchProgressLabel.setText(format("%.1f%%", progress * 100.0f));
                    });
                }
                else if (responseId == Response.Success.getValue())
                {
                    stopwatchTimeline.stop();

                    Platform.runLater(() ->
                    {
                        updateStopwatchLabel();

                        searchProgressBar.setProgress(0.0f);
                        searchProgressLabel.setText(format("%.1f%%", 0.0f));
                        searchButton.setText("Search");
                        searchButton.setDisable(false);
                        isExistFile = Bool.True;
                        Toolkit.getDefaultToolkit().beep();
                    });

                    try
                    {
                        WritablePacket resultsCount = new WritablePacket();
                        resultsCount.writeInt(Command.GetSearchResultsCount.getValue());
                        client.sendPacket(resultsCount);
                    }
                    catch(IOException e)
                    {
                        log(e.getMessage());
                    }
                }
                else if (responseId == Response.Failure.getValue())
                {
                    searchProgressBar.setProgress(0.0f);
                    searchProgressLabel.setText(format("%.1f%%", 0.0f));
                    searchButton.setDisable(false);
                    log("[Error] Dump is Failed");
                    Toolkit.getDefaultToolkit().beep();
                }
            }
            else if (commandId == Command.SpecificSearch.getValue())
            {
                int responseId = packet.readInt();
                if (responseId == Response.Progress.getValue())
                {
                    float progress = packet.readFloat();
                    Platform.runLater(() ->
                    {
                        searchProgressBar.setProgress(progress);
                        searchProgressLabel.setText(format("%.1f%%", progress * 100.0f));
                    });
                }
                else if (responseId == Response.Success.getValue())
                {
                    stopwatchTimeline.stop();

                    Platform.runLater(() ->
                    {
                        updateStopwatchLabel();

                        searchProgressBar.setProgress(0.0f);
                        searchProgressLabel.setText(format("%.1f%%", 0.0f));
                        searchButton.setDisable(false);
                        Toolkit.getDefaultToolkit().beep();
                    });

                    try
                    {
                        WritablePacket resultsCount = new WritablePacket();
                        resultsCount.writeInt(Command.GetSearchResultsCount.getValue());
                        client.sendPacket(resultsCount);
                    }
                    catch(IOException e)
                    {
                        log(e.getMessage());
                    }
                }
            }
            else if (commandId == Command.UnknownSearch.getValue())
            {
                int responseId = packet.readInt();
                if (responseId == Response.Progress.getValue())
                {
                    float progress = packet.readFloat();
                    Platform.runLater(() ->
                    {
                        searchProgressBar.setProgress(progress);
                        searchProgressLabel.setText(format("%.1f%%", progress * 100.0f));
                    });
                }
                else if (responseId == Response.Success.getValue())
                {
                    stopwatchTimeline.stop();

                    Platform.runLater(() ->
                    {
                        updateStopwatchLabel();

                        searchProgressBar.setProgress(0.0f);
                        searchProgressLabel.setText(format("%.1f%%", 0.0f));
                        searchButton.setDisable(false);
                        Toolkit.getDefaultToolkit().beep();
                    });

                    try
                    {
                        WritablePacket resultsCount = new WritablePacket();
                        resultsCount.writeInt(Command.GetSearchResultsCount.getValue());
                        client.sendPacket(resultsCount);
                    }
                    catch(IOException e)
                    {
                        log(e.getMessage());
                    }
                }
            }
            else if (commandId == Command.ClearSearchFile.getValue())
            {
                int responseId = packet.readInt();
                if (responseId == Response.Success.getValue())
                {
                    Platform.runLater(() ->
                    {
                        searchButton.setText("Dump");
                        isExistFile = Bool.False;
                    });
                }
                else if (responseId == Response.Failure.getValue())
                {
                    log("[Search Tab] Failed to clear search files");
                }
            }
            else if (commandId == Command.CheckSearchFile.getValue())
            {
                // 存在するかどうか
                int value = packet.readInt();
                if (value == Bool.True.getValue())
                {
                    Platform.runLater(() -> searchButton.setText("Search"));
                    isExistFile = Bool.True;
                }
                else if (value == Bool.False.getValue())
                {
                    Platform.runLater(() -> searchButton.setText("Dump"));
                    isExistFile = Bool.False;
                }
            }
            else if(commandId == Command.GetSearchResults.getValue())
            {
                int responseId = packet.readInt();
                if(responseId == Response.Data.getValue())
                {
                    int address = packet.readInt();
                    int value = packet.readInt();
                    tempResults.add(new SearchResult(address, value));
                }
                else if(responseId == Response.Success.getValue())
                {
                    searchResults.setAll(tempResults);
                    tempResults.clear();
                }
            }
            else if(commandId == Command.GetSearchResultsCount.getValue())
            {
                int responseId = packet.readInt();
                if(responseId == Response.Success.getValue())
                {
                    int resultCount = packet.readInt();
                    Platform.runLater(() -> resultLabel.setText(format("Results: %d", resultCount)));
                    searchResults.clear();
                    if(resultCount < 1024)
                    {
                        try
                        {
                            WritablePacket resultsCount = new WritablePacket();
                            resultsCount.writeInt(Command.GetSearchResults.getValue());
                            client.sendPacket(resultsCount);
                        }
                        catch(IOException e)
                        {
                            log(e.getMessage());
                        }
                    }
                }
            }
        }
        catch (IOException e)
        {
            log("読み取り失敗: " + e.getMessage());
        }
    }

    @FXML
    private void onSearchOrDumpClicked()
    {
        Platform.runLater(() ->
        {
            searchButton.setDisable(true);
        });

        searchStartTime = System.nanoTime();
        stopwatchTimeline.playFromStart();

        // サーチ実行
        try
        {
            if (isExistFile == Bool.True)
            {
                switch (searchTypeComboBox.getValue())
                {
                    case SearchType.Unknown ->
                    {
                        WritablePacket packet = new WritablePacket();
                        packet.writeInt(Command.UnknownSearch.getValue());
                        packet.writeInt(comparisonComboBox.getValue().getValue());
                        client.sendPacket(packet);
                    }
                    case SearchType.Specific ->
                    {
                        WritablePacket packet = new WritablePacket();
                        packet.writeInt(Command.SpecificSearch.getValue());
                        packet.writeInt(comparisonComboBox.getValue().getValue());
                        packet.writeInt(10);
                        client.sendPacket(packet);
                    }
                }
            }
            else
            {
                WritablePacket packet = new WritablePacket();
                packet.writeInt(Command.DumpForSearch.getValue());
                packet.writeInt(startAddressField.getIntValue());
                packet.writeInt(endAddressField.getIntValue());
                packet.writeInt(valueTypeComboBox.getValue().getValue());
                client.sendPacket(packet);
            }
        }
        catch (IOException e)
        {
            log("送信失敗: " + e.getMessage());
        }
    }

    @FXML
    private void onClearClicked()
    {
        // サーチ実行
        try
        {
            WritablePacket packet = new WritablePacket();
            packet.writeInt(Command.ClearSearchFile.getValue());
            client.sendPacket(packet);
        }
        catch (IOException e)
        {
            log("送信失敗: " + e.getMessage());
        }
    }

    public void onSelected()
    {
        // サーチファイルが既に存在するかを確認
        try
        {
            WritablePacket packet = new WritablePacket();
            packet.writeInt(Command.CheckSearchFile.getValue());
            client.sendPacket(packet);
        }
        catch (IOException e)
        {
            log("送信失敗: " + e.getMessage());
        }
    }

    private enum SearchType
    {
        Unknown, Specific, None;

        public static SearchType fromString(String str)
        {
            switch (str)
            {
                case "Unknown" ->
                {
                    return Unknown;
                }
                case "Specific" ->
                {
                    return Specific;
                }
            }
            return None;
        }
    }
}
