package com.vortex.controller;

import com.vortex.command.Command;
import com.vortex.command.Response;
import com.vortex.command.ValueType;
import com.vortex.data.Client;
import com.vortex.data.PacketDispatcher;
import com.vortex.data.ReadablePacket;
import com.vortex.data.WritablePacket;
import javafx.application.Platform;
import javafx.fxml.FXML;
import javafx.scene.control.Button;
import javafx.scene.control.Label;
import javafx.scene.control.ProgressBar;

import java.awt.*;
import java.io.IOException;

import static java.lang.String.format;

public class SearchTabController implements PacketDispatcher.Listener
{
    public LoggerTabController loggerTabController;

    public void log(String msg)
    {
        loggerTabController.append(msg);
    }

    public Client client;

    private boolean isExistFile = false;

    @FXML
    private Button searchButton;

    @FXML
    private ProgressBar searchProgressBar;

    @FXML
    private Label searchProgressLabel;

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
                        if (progress >= 1.0f)
                        {
                            searchProgressBar.setProgress(0.0f);
                            searchProgressLabel.setText(format("%.1f%%", 0.0f));
                            searchButton.setText("Search");
                            Toolkit.getDefaultToolkit().beep();
                        }
                    });
                }
            }
            else if (commandId == Command.CheckSearchFile.getValue())
            {
                // 存在するかどうか
                if (packet.readInt() == 1)
                {
                    Platform.runLater(() -> searchButton.setText("Search"));
                    isExistFile = true;
                }
                else
                {
                    Platform.runLater(() -> searchButton.setText("Dump"));
                    isExistFile = false;
                }
            }
        }
        catch (IOException e)
        {
            log("読み取り失敗: " + e.getMessage());
        }
    }

    @FXML
    private void initialize()
    {
        PacketDispatcher.addListener(this);
    }

    @FXML
    private void onSearchOrDumpClicked()
    {
        // サーチ実行
        try
        {
            if (isExistFile)
            {
                WritablePacket packet = new WritablePacket();
                packet.writeInt(Command.UnknownSearch.getValue());
                packet.writeInt(ValueType.Int32.getValue());
                client.sendPacket(packet);
            }
            else
            {
                WritablePacket packet = new WritablePacket();
                packet.writeInt(Command.DumpForSearch.getValue());
                packet.writeInt(0x10000000);
                packet.writeInt(0x20000000);
                packet.writeInt(ValueType.Int32.getValue());
                client.sendPacket(packet);
            }
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
}
