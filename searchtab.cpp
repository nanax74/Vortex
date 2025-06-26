#include "searchtab.h"
#include "ui_searchtab.h"

#include <winsock2.h>

#include "command.h"
#include "packet.h"

SearchTab::SearchTab(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::SearchTab)
{
    ui->setupUi(this);
    connect(ui->pushButton_Search, &QPushButton::clicked, this, &SearchTab::onSearchButtonClicked);
    connect(ui->pushButton_Reset, &QPushButton::clicked, this, &SearchTab::onResetButtonClicked);
}

SearchTab::~SearchTab()
{
    delete ui;
}

void SearchTab::onSearchButtonClicked()
{
    const quint32 searchId = static_cast<quint32>(Command::ID::DumpForSearch);
    const quint32 startAddress = ui->lineEdit_StartAddress->text().toUInt(nullptr, 16);
    const quint32 endAddress = ui->lineEdit_EndAddress->text().toUInt(nullptr, 16);
}

void SearchTab::onResetButtonClicked()
{
    Packet data;

    quint32 clearId = static_cast<quint32>(Command::ID::ClearSearchFile);
    data.Write(clearId);

    emit sendData(data);  // シグナルで ConnectionTab に渡す
}

void SearchTab::onDataReceived(const Packet& data)
{

}
