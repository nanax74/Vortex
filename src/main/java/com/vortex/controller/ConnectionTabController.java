package com.vortex.controller;

import com.vortex.data.Client;
import com.vortex.controls.HexTextField;
import com.vortex.data.PacketDispatcher;
import com.vortex.data.ReadablePacket;
import com.vortex.data.WritablePacket;
import javafx.application.Platform;
import javafx.fxml.FXML;
import javafx.scene.control.*;

import java.io.*;

public class ConnectionTabController
{
    public LoggerTabController loggerTabController;
    public void log(String msg)
    {
        loggerTabController.append(msg);
    }

    public Client client;
    private final int PORT = 50000;
    private Thread receiveThread;

    @FXML
    private TextField ipField;
    @FXML
    private Button connectButton;
    @FXML
    private Button disconnectButton;
    @FXML
    private HexTextField inputField;

    @FXML
    private void onConnectClicked()
    {
        String ip = ipField.getText().trim();
        if (ip.isEmpty())
        {
            log("IPアドレスを入力してください");
            return;
        }

        new Thread(() ->
        {
            try
            {
                client.connect(ip, PORT, 3000);

                Platform.runLater(() ->
                {
                    log("接続成功: " + ip + ":" + PORT);
                    connectButton.setDisable(true);
                    disconnectButton.setDisable(false);
                });

                receiveThread = new Thread(this::receiveLoop);
                receiveThread.setDaemon(true);
                receiveThread.start();
            }
            catch (IOException e)
            {
                Platform.runLater(() -> log("接続失敗: " + e.getMessage()));
            }
        }).start();
    }

    @FXML
    private void onDisconnectClicked()
    {
        new Thread(() ->
        {
            try
            {
                client.disconnect();
                log("切断成功");
            }
            catch (IOException e)
            {
                Platform.runLater(() -> log("切断エラー: " + e.getMessage()));
            }
            finally
            {
                Platform.runLater(() ->
                {
                    connectButton.setDisable(false);
                    disconnectButton.setDisable(true);
                });
            }
        }).start();
    }

    @FXML
    private void onSendClicked()
    {
        byte[] command = inputField.getBytes();
        WritablePacket packet = new WritablePacket(command);

        new Thread(() ->
        {
            try
            {
                // 送信
                client.sendPacket(packet);
            }
            catch (IOException e)
            {
                Platform.runLater(() -> log("送信エラー: " + e.getMessage()));
            }
        }).start();
    }

    private void receiveLoop()
    {
        Thread thread = new Thread(() ->
        {
            try
            {
                while (true) // 常に受信
                {
                    ReadablePacket packet = client.receivePacket();
                    PacketDispatcher.dispatch(packet);
                }
            }
            catch (IOException e)
            {
                Platform.runLater(() -> log("受信エラー: " + e.getMessage()));
            }
        });
        thread.setDaemon(true); // アプリ終了時にスレッドも終了
        thread.start();
    }
}
