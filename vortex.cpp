#include "vortex.h"
#include "ui_vortex.h"

#include "connectiontab.h"
#include "consoletab.h"
#include "searchtab.h"

Vortex::Vortex(QWidget *parent)
    : QMainWindow(parent)
    , ui(new Ui::Vortex)
{
    ui->setupUi(this);

    ConnectionTab* connectionTab = new ConnectionTab(this);
    ui->tabWidget_Main->addTab(connectionTab, tr("接続"));

    ConsoleTab* consoleTab = new ConsoleTab(this);
    ui->tabWidget_Main->addTab(consoleTab, tr("コンソール"));

    SearchTab* searchTab = new SearchTab(this);
    ui->tabWidget_Main->addTab(searchTab, tr("サーチ"));

    connect(consoleTab, &ConsoleTab::sendData, connectionTab, &ConnectionTab::onSendDataRequested); // 送信要求
    connect(connectionTab, &ConnectionTab::dataReceived, consoleTab, &ConsoleTab::onDataReceived); // 受信検知

    connect(searchTab, &SearchTab::sendData, connectionTab, &ConnectionTab::onSendDataRequested);
    connect(connectionTab, &ConnectionTab::dataReceived, searchTab, &SearchTab::onDataReceived);
}

Vortex::~Vortex()
{
    delete ui;
}
