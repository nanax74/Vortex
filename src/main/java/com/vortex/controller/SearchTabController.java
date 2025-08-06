package com.vortex.controller;

import com.vortex.command.*;
import com.vortex.controls.HexTextField;
import com.vortex.data.*;
import javafx.animation.KeyFrame;
import javafx.animation.Timeline;
import javafx.application.Platform;
import javafx.beans.binding.Bindings;
import javafx.collections.FXCollections;
import javafx.collections.ListChangeListener;
import javafx.collections.ObservableList;
import javafx.fxml.FXML;
import javafx.geometry.Pos;
import javafx.scene.control.*;
import javafx.scene.control.Button;
import javafx.scene.control.Label;
import javafx.scene.control.MenuItem;
import javafx.scene.control.CheckBox;
import javafx.scene.input.Clipboard;
import javafx.scene.input.ClipboardContent;
import javafx.util.Duration;

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

    public Tab tab;

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
    private HexTextField valueField;

    @FXML
    private ListView<String> selectedAddressesView;

    @FXML
    private HexTextField valueWriteField;

    @FXML
    private TableView<SearchResult> searchResultsView;

    @FXML
    private TableColumn<SearchResult, Integer> addressColumn;

    @FXML
    private TableColumn<SearchResult, Long> valueColumn;

    @FXML
    private CheckBox autoUpdateCheckBox;

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
        searchTypeComboBox.valueProperty().addListener((observable, oldValue, newValue) ->
        {
            Platform.runLater(() ->
            {
                switch (newValue)
                {
                    case Specific ->
                    {
                        valueField.setDisable(false);
                    }
                    case Unknown ->
                    {
                        valueField.setDisable(true);
                    }
                }
            });
        });

        valueTypeComboBox.getItems().addAll(ValueType.values());
        valueTypeComboBox.setValue(ValueType.Float);
        valueTypeComboBox.valueProperty().addListener((observable, oldValue, newValue) ->
        {
            switch (newValue)
            {
                case Int8, UInt8 ->
                {
                    valueField.setMaxLength(2);
                }
                case Int16, UInt16 ->
                {
                    valueField.setMaxLength(4);
                }
                case Int32, UInt32, Float ->
                {
                    valueField.setMaxLength(8);
                }
                case Int64, UInt64, Double ->
                {
                    valueField.setMaxLength(16);
                }
            }
            searchResultsView.refresh(); // 型が変わったら表示を更新
        });

        comparisonComboBox.getItems().addAll(Comparison.values());
        comparisonComboBox.setValue(Comparison.Equal);

        selectedAddressesView.setCellFactory(lv -> new ListCell<String>()
        {
            @Override
            protected void updateItem(String item, boolean empty)
            {
                super.updateItem(item, empty);
                if (empty || item == null)
                {
                    setText(null);
                }
                else
                {
                    setText(item);
                    setAlignment(Pos.CENTER); // 中央寄せにする
                }
            }
        });

        searchResultsView.getSelectionModel().setSelectionMode(SelectionMode.MULTIPLE);
        searchResultsView.getSelectionModel().getSelectedItems().addListener((ListChangeListener<SearchResult>) change ->
        {
            ObservableList<String> selectedAddresses = FXCollections.observableArrayList();
            for (SearchResult item : searchResultsView.getSelectionModel().getSelectedItems())
            {
                String address = format("0x%08X", item.getAddress());
                selectedAddresses.add(address);
            }
            Platform.runLater(() -> selectedAddressesView.setItems(selectedAddresses));
        });

        addressColumn.setCellValueFactory(cellData -> cellData.getValue().addressProperty().asObject());
        valueColumn.setCellValueFactory(cellData -> cellData.getValue().valueProperty().asObject());

        addressColumn.setCellFactory(column -> new TableCell<>()
        {
            {
                setAlignment(Pos.CENTER);
            }

            @Override
            protected void updateItem(Integer item, boolean empty)
            {
                super.updateItem(item, empty);
                setText(empty || item == null ? null : String.format("0x%08X", item));
            }
        });

        valueColumn.setCellFactory(column -> new TableCell<>()
        {
            {
                setAlignment(Pos.CENTER);
            }

            @Override
            protected void updateItem(Long item, boolean empty)
            {
                super.updateItem(item, empty);
                if (empty || item == null)
                {
                    setText(null);
                }
                else
                {
                    SearchResult result = getTableRow().getItem();
                    if (result != null)
                    {
                        setText(result.getHexValue(valueTypeComboBox.getValue()));
                    }
                }
            }
        });

        searchResultsView.setItems(searchResults);

        searchResultsView.widthProperty().addListener((obs, oldWidth, newWidth) ->
        {
            // カラム数を取得
            int columnCount = searchResultsView.getColumns().size();
            if (columnCount == 0)
            {
                return;
            }

            // TableViewの幅から、微調整用の余白（境界線やスクロールバー対策）を引く
            double availableWidth = newWidth.doubleValue() - 2;

            // 均等な幅に設定し、リサイズを禁止
            double columnWidth = availableWidth / columnCount;
            for (TableColumn<?, ?> column : searchResultsView.getColumns())
            {
                column.setPrefWidth(columnWidth);
            }
        });

        searchResultsView.setRowFactory(tv ->
        {
            TableRow<SearchResult> row = new TableRow<>();
            ContextMenu contextMenu = new ContextMenu();

            MenuItem copyAddressItem = createCopyAddressItem(row);
            MenuItem copyValueItem = createCopyValueItem(row);

            contextMenu.getItems().addAll(copyAddressItem, copyValueItem);

            row.contextMenuProperty().bind(Bindings.when(row.emptyProperty()).then((ContextMenu) null).otherwise(contextMenu));

            return row;
        });

        autoUpdateCheckBox.selectedProperty().addListener((observable, oldValue, newValue) ->
        {
            if (newValue)
            {
                subscribeToUpdates();
            }
            else
            {
                unsubscribeFromUpdates();
            }
        });

        Platform.runLater(() ->
        {
            searchTypeComboBox.setDisable(true);
            comparisonComboBox.setDisable(true);
            valueField.setDisable(true);
        });

        // 0.01秒ごとに更新
        stopwatchTimeline = new Timeline(new KeyFrame(Duration.millis(10), e -> updateStopwatchLabel()));
        stopwatchTimeline.setCycleCount(Timeline.INDEFINITE); // 無限に繰り返す
    }

    private void subscribeToUpdates()
    {
        try
        {
            WritablePacket packet = new WritablePacket();
            packet.writeInt(Command.SubscribeAddress.getValue());
            packet.writeInt(searchResults.size());
            for (SearchResult result : searchResults)
            {
                packet.writeInt(result.getAddress());
                packet.writeInt(valueTypeComboBox.getValue().getValue());
            }
            client.sendPacket(packet);
        }
        catch (IOException e)
        {
            log("Failed to subscribe to updates: " + e.getMessage());
        }
    }

    private void unsubscribeFromUpdates()
    {
        try
        {
            WritablePacket packet = new WritablePacket();
            packet.writeInt(Command.UnsubscribeAddress.getValue());
            client.sendPacket(packet);
        }
        catch (IOException e)
        {
            log("Failed to unsubscribe from updates: " + e.getMessage());
        }
    }

    private MenuItem createCopyValueItem(TableRow<SearchResult> row)
    {
        MenuItem copyValueItem = new MenuItem("値をコピー");
        copyValueItem.setOnAction(event ->
        {
            SearchResult entry = row.getItem();
            if (entry != null)
            {
                Clipboard clipboard = Clipboard.getSystemClipboard();
                ClipboardContent content = new ClipboardContent();
                content.putString(entry.getHexValue(valueTypeComboBox.getValue()));
                clipboard.setContent(content);
            }
        });
        return copyValueItem;
    }

    private MenuItem createCopyAddressItem(TableRow<SearchResult> row)
    {
        MenuItem copyAddressItem = new MenuItem("アドレスをコピー");
        copyAddressItem.setOnAction(event ->
        {
            SearchResult entry = row.getItem();
            if (entry != null)
            {
                Clipboard clipboard = Clipboard.getSystemClipboard();
                ClipboardContent content = new ClipboardContent();
                content.putString(String.format("0x%08X", entry.getAddress()));
                clipboard.setContent(content);
            }
        });
        return copyAddressItem;
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
            if (commandId == Command.PushAddressValue.getValue())
            {
                int address = packet.readInt();
                int type = packet.readInt();
                long value = packet.readLong(); // Stardustからはuint64_tで送られてくる

                Platform.runLater(() ->
                {
                    for (SearchResult result : searchResults)
                    {
                        if (result.getAddress() == address)
                        {
                            result.setValue(value);
                            break;
                        }
                    }
                });
            }
            else if (commandId == Command.DumpForSearch.getValue())
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

                        searchTypeComboBox.setDisable(false);
                        comparisonComboBox.setDisable(false);
                        startAddressField.setDisable(true);
                        endAddressField.setDisable(true);
                        valueTypeComboBox.setDisable(true);
                        valueField.setDisable(true);

                        Toolkit.getDefaultToolkit().beep();
                    });
                    onEndDumpOrSearch();

                    try
                    {
                        WritablePacket resultsCount = new WritablePacket();
                        resultsCount.writeInt(Command.GetSearchResultsCount.getValue());
                        client.sendPacket(resultsCount);
                    }
                    catch (IOException e)
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
                        if (searchTypeComboBox.getValue() == SearchType.Specific)
                        {
                            valueField.setDisable(false);
                        }

                        Toolkit.getDefaultToolkit().beep();
                    });
                    onEndDumpOrSearch();

                    try
                    {
                        WritablePacket resultsCount = new WritablePacket();
                        resultsCount.writeInt(Command.GetSearchResultsCount.getValue());
                        client.sendPacket(resultsCount);
                    }
                    catch (IOException e)
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
                    onEndDumpOrSearch();

                    try
                    {
                        WritablePacket resultsCount = new WritablePacket();
                        resultsCount.writeInt(Command.GetSearchResultsCount.getValue());
                        client.sendPacket(resultsCount);
                    }
                    catch (IOException e)
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
                    isExistFile = Bool.False;
                    Platform.runLater(() ->
                    {
                        searchButton.setText("Dump");
                        searchTypeComboBox.setDisable(true);
                        comparisonComboBox.setDisable(true);
                        startAddressField.setDisable(false);
                        endAddressField.setDisable(false);
                        valueTypeComboBox.setDisable(false);
                        valueField.setDisable(false);
                    });
                    searchResults.clear();
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
                    Platform.runLater(() ->
                    {
                        searchButton.setText("Search");
                        searchTypeComboBox.setDisable(false);
                        comparisonComboBox.setDisable(false);
                        valueField.setDisable(false);
                        startAddressField.setDisable(true);
                        endAddressField.setDisable(true);
                        valueTypeComboBox.setDisable(true);
                    });
                    isExistFile = Bool.True;
                }
                else if (value == Bool.False.getValue())
                {
                    Platform.runLater(() ->
                    {
                        searchButton.setText("Dump");
                        searchTypeComboBox.setDisable(true);
                        comparisonComboBox.setDisable(true);
                        valueField.setDisable(true);
                        startAddressField.setDisable(false);
                        endAddressField.setDisable(false);
                        valueTypeComboBox.setDisable(false);
                    });
                    isExistFile = Bool.False;
                }
            }
            else if (commandId == Command.GetSearchResults.getValue())
            {
                int responseId = packet.readInt();
                if (responseId == Response.Data.getValue())
                {
                    int address = packet.readInt();
                    long value = packet.readInt();
                    tempResults.add(new SearchResult(address, value));
                }
                else if (responseId == Response.Success.getValue())
                {
                    searchResults.setAll(tempResults);
                    tempResults.clear();
                }
            }
            else if (commandId == Command.GetSearchResultsCount.getValue())
            {
                int responseId = packet.readInt();
                if (responseId == Response.Success.getValue())
                {
                    int resultCount = packet.readInt();
                    Platform.runLater(() -> resultLabel.setText(format("Results: %d", resultCount)));
                    searchResults.clear();
                    if (resultCount < 1024)
                    {
                        try
                        {
                            WritablePacket resultsCount = new WritablePacket();
                            resultsCount.writeInt(Command.GetSearchResults.getValue());
                            client.sendPacket(resultsCount);
                        }
                        catch (IOException e)
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
        onStartDumpOrSearch();

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
                        writePacketValueFromHexField(packet, valueField); // 任意の型の値を書き込み
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

    @FXML
    private void onWriteValueClicked()
    {
        try
        {
            ObservableList<String> selectedItems = selectedAddressesView.getItems();
            int[] addresses = selectedItems.stream().mapToInt(s -> Integer.parseInt(s.replace("0x", ""), 16)).toArray();
            for (int address : addresses)
            {
                WritablePacket packet = new WritablePacket();
                packet.writeInt(Command.WriteMemory.getValue());
                packet.writeInt(address);
                packet.writeInt(valueTypeComboBox.getValue().getValue());
                writePacketValueFromHexField(packet, valueWriteField);
                client.sendPacket(packet);
            }
        }
        catch (Exception e)
        {
            log(e.getMessage());
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

    private void writePacketValueFromHexField(WritablePacket packet, HexTextField hexField)
    {
        try
        {
            switch (valueTypeComboBox.getValue())
            {
                case Int8, UInt8 ->
                {
                    packet.writeByte(hexField.getByteValue());
                }
                case Int16, UInt16 ->
                {
                    packet.writeShort(hexField.getShortValue());
                }
                case Int32, UInt32 ->
                {
                    packet.writeInt(hexField.getIntValue());
                }
                case Float ->
                {
                    packet.writeFloat(hexField.getFloatValue());
                }
                case Double ->
                {
                    packet.writeDouble(hexField.getDoubleValue());
                }
            }
        }
        catch (IOException e)
        {
            log(e.getMessage());
        }
    }

    private void onStartDumpOrSearch()
    {

    }

    private void onEndDumpOrSearch()
    {

    }
}
