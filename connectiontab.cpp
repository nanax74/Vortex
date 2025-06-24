#include "connectiontab.h"
#include "ui_connectiontab.h"

#include <winsock2.h>

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
    // TCP_NODELAY を設定
    int fd = static_cast<int>(socket->socketDescriptor());
    if (fd != -1)
    {
        BOOL flag = TRUE;
        int result = setsockopt(fd, IPPROTO_TCP, TCP_NODELAY, (const char*)&flag, sizeof(flag));
        if (result == 0)
        {
            ui->textEdit_Log->append("<span style='color: green;'>[Info] TCP_NODELAY を有効化しました");
        }
        else
        {
            ui->textEdit_Log->append("<span style='color: orange;'>[Warn] TCP_NODELAY 設定失敗");
        }
    }

    ui->textEdit_Log->append("<span style='color: green;'>[Info] 接続しました");
    ui->textEdit_Log->moveCursor(QTextCursor::End);

    ui->pushButton_Connect->setEnabled(false);
    ui->pushButton_Disconnect->setEnabled(true);
}

void ConnectionTab::onDisconnected()
{
    ui->textEdit_Log->append("<span style='color: green;'>[Info] 切断しました");
    ui->textEdit_Log->moveCursor(QTextCursor::End);

    ui->pushButton_Connect->setEnabled(true);
    ui->pushButton_Disconnect->setEnabled(false);
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
        QByteArray packet = recvBuffer.mid(sizeof(quint32), packetSize);

        // バッファから取り除く
        recvBuffer.remove(0, packetSize + sizeof(quint32));

        emit dataReceived(packet);

        QString text = QString::fromUtf8(packet.toHex());
        ui->textEdit_Log->append("<span style='color: blue;'>[Recv] " + text);
        ui->textEdit_Log->moveCursor(QTextCursor::End);
    }
}

void ConnectionTab::onErrorOccurred()
{
    ui->textEdit_Log->append("<span style='color: red;'>[Error] " + socket->errorString());
    ui->textEdit_Log->moveCursor(QTextCursor::End);
}

void ConnectionTab::onSendDataRequested(const QByteArray &data)
{
    if (socket && socket->state() == QAbstractSocket::ConnectedState)
    {
        QByteArray packet;

        quint32 size = data.size();
        quint32 size_be = htonl(size);  // ネットワークバイトオーダーに変換

        packet.append(reinterpret_cast<const char*>(&size_be), sizeof(size_be));
        packet.append(data);

        socket->write(packet);
    }
    else
    {
        ui->textEdit_Log->append("<span style='color: red;'>[Error] ソケットが未接続のため送信できません");
    }
}
