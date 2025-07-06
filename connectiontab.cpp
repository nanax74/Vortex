#include "connectiontab.h"
#include "ui_connectiontab.h"

#include <QTimer>

#include <winsock2.h>

#include "command.h"
#include "packet.h"

ConnectionTab::ConnectionTab(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::ConnectionTab)
    , socket(new QTcpSocket(this))
{
    ui->setupUi(this);

    // ボタン押下で接続開始
    connect(ui->pushButton_Connect, &QPushButton::clicked, this, &ConnectionTab::onConnectButtonClicked);
    connect(ui->pushButton_Disconnect, &QPushButton::clicked, this, &ConnectionTab::onDisconnectButtonClicked);

    // ソケットのシグナル接続
    connect(socket, &QTcpSocket::connected, this, &ConnectionTab::onConnected);
    connect(socket, &QTcpSocket::disconnected, this, &ConnectionTab::onDisconnected);
    connect(socket, &QTcpSocket::readyRead, this, &ConnectionTab::onReadyRead);
    connect(socket, &QTcpSocket::errorOccurred, this, &ConnectionTab::onErrorOccurred);

    ui->pushButton_Disconnect->setEnabled(false);

    heartbeatTimer = new QTimer(this);
    connect(heartbeatTimer, &QTimer::timeout, this, &ConnectionTab::onHeartBeat);
    connect(this, &ConnectionTab::dataReceived, this, &ConnectionTab::onHeartbeatResponseReceived);
}

ConnectionTab::~ConnectionTab()
{
    delete ui;
}

void ConnectionTab::onConnectButtonClicked()
{
    QString host = ui->lineEdit_Host->text();

    if(socket->state() == QAbstractSocket::UnconnectedState)
    {
        socket->connectToHost(host, STARDUST_PORT);
    }
}

void ConnectionTab::onDisconnectButtonClicked()
{
    if(socket->state() == QAbstractSocket::ConnectedState)
    {
        socket->disconnectFromHost();
    }
}

void ConnectionTab::onConnected()
{
    // 低遅延設定
    socket->socketOption(QAbstractSocket::SocketOption::LowDelayOption);

    ui->textEdit_Log->append("<span style='color: green;'>[Info] 接続しました");
    ui->textEdit_Log->moveCursor(QTextCursor::End);

    ui->pushButton_Connect->setEnabled(false);
    ui->pushButton_Disconnect->setEnabled(true);

    lastResponseTimer.start();
    heartbeatTimer->start(1000);

    emit connected();
}

void ConnectionTab::onDisconnected()
{
    ui->textEdit_Log->append("<span style='color: green;'>[Info] 切断しました");
    ui->textEdit_Log->moveCursor(QTextCursor::End);

    ui->pushButton_Connect->setEnabled(true);
    ui->pushButton_Disconnect->setEnabled(false);

    heartbeatTimer->stop();
    lastResponseTimer.invalidate();

    emit disconnected();
}

void ConnectionTab::onReadyRead()
{
    recvBuffer.append(socket->readAll());

    while (true)
    {
        if (recvBuffer.size() < sizeof(quint32))
        {
            // ヘッダーがまだ全部来ていない（パケット長のため4バイト）
            break;
        }

        // 先頭4バイトはパケット長（ビッグエンディアン）
        quint32 packetSize = qFromBigEndian<quint32>(reinterpret_cast<const uchar*>(recvBuffer.constData()));

        if (recvBuffer.size() < packetSize + sizeof(quint32))
        {
            // パケット全体がまだ揃っていない
            break;
        }

        // パケット本体を切り出し
        QByteArray data = recvBuffer.mid(sizeof(quint32), packetSize);

        // バッファから取り除く
        recvBuffer.remove(0, packetSize + sizeof(quint32));

        Packet packet(data);

        emit dataReceived(packet);
    }
}

void ConnectionTab::onErrorOccurred()
{
    ui->textEdit_Log->append("<span style='color: red;'>[Error] " + socket->errorString());
    ui->textEdit_Log->moveCursor(QTextCursor::End);
}

void ConnectionTab::onSendDataRequested(const Packet &packet)
{
    if (socket && socket->state() == QAbstractSocket::ConnectedState)
    {
        emit dataSent(packet);

        Packet packetSend;

        QByteArray data = packet.GetData();
        quint32 size = data.size();
        packetSend.Write(size);
        packetSend.Append(data);

        socket->write(packetSend.GetData());
    }
    else
    {
        ui->textEdit_Log->append("<span style='color: red;'>[Error] ソケットが未接続のため送信できません");
    }
}

void ConnectionTab::onHeartBeat()
{
    // 5秒以上返答がない場合は切断
    if (lastResponseTimer.isValid() && lastResponseTimer.elapsed() > 5000)
    {
        ui->textEdit_Log->append("<span style='color: red;'>[Error] 応答なしのため切断します");
        socket->disconnectFromHost();
        return;
    }

    quint32 cmd = static_cast<quint32>(Command::ID::Respond);
    Packet packet;
    packet.Write(cmd);

    onSendDataRequested(packet);
}

void ConnectionTab::onHeartbeatResponseReceived(const Packet& packet)
{
    if (packet.GetData().size() >= 4)
    {
        quint32 cmd = 0;
        Packet copy = packet;
        copy.Read(cmd);

        if (cmd == static_cast<quint32>(Command::ID::Respond))
        {
            lastResponseTimer.restart();
        }
    }
}
