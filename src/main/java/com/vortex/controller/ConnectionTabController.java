package com.vortex.controller;

import com.vortex.Client;
import com.vortex.controls.HexTextField;
import com.vortex.data.Packet;
import javafx.application.Platform;
import javafx.fxml.FXML;
import javafx.scene.control.*;

import java.io.*;
import java.util.List;

public class ConnectionTabController
{
    public LoggerTabController loggerTabController;

    private Client client;
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
    private void initialize()
    {
        client = new Client();
    }

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
        Packet packet = client.getPacket(command);

        new Thread(() ->
        {
            try
            {
                // 送信
                packet.send();

                // ログ
                StringBuilder sb = new StringBuilder();
                for (byte c : command)
                {
                    sb.append(String.format("%02X", c));
                }
                Platform.runLater(() ->
                {
                    log("送信: " + sb);
                    inputField.clear();
                });
            }
            catch (IOException e)
            {
                Platform.runLater(() -> log("送信エラー: " + e.getMessage()));
            }
        }).start();
    }

    private void receiveLoop()
    {
        try
        {
            List<Packet> packets = client.receivePackets();

            for (Packet packet : packets)
            {
                byte[] data = packet.toByteArray(); // Packetからバイト列を取得

                // ログ出力用にHex文字列を作成
                StringBuilder sb = new StringBuilder();
                for (byte d : data)
                {
                    sb.append(String.format("%02X", d));
                }

                String log = "受信: " + sb;
                Platform.runLater(() -> log(log));
            }
        }
        catch (IOException e)
        {
            Platform.runLater(() -> log("受信エラー: " + e.getMessage()));
        }
    }

    public void log(String msg)
    {
        loggerTabController.append(msg);
    }
}
