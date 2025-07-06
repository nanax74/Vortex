#ifndef CONNECTIONTAB_H
#define CONNECTIONTAB_H

#include <QElapsedTimer>
#include <QTcpSocket>
#include <QWidget>

#include "packet.h"

constexpr quint16 STARDUST_PORT = 50000;

namespace Ui {
class ConnectionTab;
}

class ConnectionTab : public QWidget
{
    Q_OBJECT
public:
    explicit ConnectionTab(QWidget *parent = nullptr);
    ~ConnectionTab();

private slots:
    void onConnectButtonClicked();
    void onDisconnectButtonClicked();
    void onConnected();
    void onDisconnected();
    void onReadyRead();
    void onErrorOccurred();
    void onHeartBeat();
    void onHeartbeatResponseReceived(const Packet& packet);

public slots:
    void onSendDataRequested(const Packet& text); // 外部から受け取る

private:
    Ui::ConnectionTab *ui;
    QTcpSocket *socket;
    QByteArray recvBuffer;
    QTimer *heartbeatTimer = nullptr;
    QElapsedTimer lastResponseTimer;

signals:
    void connected();
    void disconnected();
    void dataSent(const Packet& packet);

    // Command-specific signals
    void heartbeatResponseReceived(const Packet& packet);
    void consoleDataReceived(const Packet& packet);
    void searchDataReceived(const Packet& packet);
};

#endif // CONNECTIONTAB_H
