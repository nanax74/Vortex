#include "vortex.h"
#include "ui_vortex.h"

#include "connectiontab.h"
#include "consoletab.h"

Vortex::Vortex(QWidget *parent)
    : QMainWindow(parent)
    , ui(new Ui::Vortex)
{
    ui->setupUi(this);

    ConnectionTab* connectionTab = new ConnectionTab(this);
    ui->tabWidget_Main->addTab(connectionTab, tr("接続"));

    ConsoleTab* consoleTab = new ConsoleTab(this);
    ui->tabWidget_Main->addTab(consoleTab, tr("コンソール"));

    connect(consoleTab, &ConsoleTab::sendData, connectionTab, &ConnectionTab::onSendDataRequested); // 送信要求
    connect(connectionTab, &ConnectionTab::dataReceived, consoleTab, &ConsoleTab::onDataReceived); // 受信検知
}

Vortex::~Vortex()
{
    delete ui;
}
