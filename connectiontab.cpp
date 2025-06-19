#include "connectiontab.h"
#include "ui_connectiontab.h"

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
    ui->textEdit_Log->append("<span style='color: green;'>[Info] 接続しました");

    ui->pushButton_Connect->setEnabled(false);
    ui->pushButton_Disconnect->setEnabled(true);
}

void ConnectionTab::onDisconnected()
{
    ui->textEdit_Log->append("<span style='color: green;'>[Info] 切断しました");

    ui->pushButton_Connect->setEnabled(true);
    ui->pushButton_Disconnect->setEnabled(false);
}

void ConnectionTab::onReadyRead()
{
    QByteArray data = socket->readAll();
    ui->textEdit_Log->append("<span style='color: blue;'>[Recv] " + QString::fromUtf8(data));
}

void ConnectionTab::onErrorOccurred()
{
    ui->textEdit_Log->append("<span style='color: red;'>[Error] " + socket->errorString());
}
