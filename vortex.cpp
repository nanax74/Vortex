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
    connectionTabIndex = ui->tabWidget_Main->addTab(connectionTab, tr("接続"));

    ConsoleTab* consoleTab = new ConsoleTab(this);
    consoleTabIndex = ui->tabWidget_Main->addTab(consoleTab, tr("コンソール"));
    ui->tabWidget_Main->setTabEnabled(consoleTabIndex, false);

    SearchTab* searchTab = new SearchTab(this);
    searchTabIndex = ui->tabWidget_Main->addTab(searchTab, tr("サーチ"));
    ui->tabWidget_Main->setTabEnabled(searchTabIndex, false);

    connect(consoleTab, &ConsoleTab::sendData, connectionTab, &ConnectionTab::onSendDataRequested); // 送信要求
    connect(connectionTab, &ConnectionTab::dataReceived, consoleTab, &ConsoleTab::onDataReceived); // 受信検知
    connect(connectionTab, &ConnectionTab::dataSent, consoleTab, &ConsoleTab::onDataSent); // 送信

    connect(searchTab, &SearchTab::sendData, connectionTab, &ConnectionTab::onSendDataRequested);
    connect(connectionTab, &ConnectionTab::dataReceived, searchTab, &SearchTab::onDataReceived);

    connect(connectionTab, &ConnectionTab::connected, this, &Vortex::onConnected);
    connect(connectionTab, &ConnectionTab::disconnected, this, &Vortex::onDisconnected);

    connect(ui->tabWidget_Main, &QTabWidget::currentChanged, this, &Vortex::onTabChanged);
}

Vortex::~Vortex()
{
    delete ui;
}

void Vortex::onConnected()
{
    ui->tabWidget_Main->setTabEnabled(consoleTabIndex, true);
    ui->tabWidget_Main->setTabEnabled(searchTabIndex, true);
}

void Vortex::onDisconnected()
{
    ui->tabWidget_Main->setTabEnabled(consoleTabIndex, false);
    ui->tabWidget_Main->setTabEnabled(searchTabIndex, false);
}

void Vortex::onTabChanged(int index)
{
    if (index == searchTabIndex)
    {
        // サーチタブが選ばれたとき
        SearchTab* searchTab = qobject_cast<SearchTab*>(ui->tabWidget_Main->widget(searchTabIndex));
        if (searchTab)
        {
            searchTab->onTabOpened();
        }
    }
}
