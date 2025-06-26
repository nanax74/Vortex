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

    ui->comboBox_ValueType->addItem("int8", static_cast<int>(Command::ValueType::Int8));
    ui->comboBox_ValueType->addItem("uint8", static_cast<int>(Command::ValueType::UInt8));
    ui->comboBox_ValueType->addItem("int16", static_cast<int>(Command::ValueType::Int16));
    ui->comboBox_ValueType->addItem("uint16", static_cast<int>(Command::ValueType::UInt16));
    ui->comboBox_ValueType->addItem("int32", static_cast<int>(Command::ValueType::Int32));
    ui->comboBox_ValueType->addItem("uint32", static_cast<int>(Command::ValueType::UInt32));
    ui->comboBox_ValueType->addItem("int64", static_cast<int>(Command::ValueType::Int64));
    ui->comboBox_ValueType->addItem("uint64", static_cast<int>(Command::ValueType::UInt64));
    ui->comboBox_ValueType->addItem("float", static_cast<int>(Command::ValueType::Float));
    ui->comboBox_ValueType->addItem("double", static_cast<int>(Command::ValueType::Double));

    ui->progressBar->setValue(0.0f);
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
    const int valueType = ui->comboBox_ValueType->currentData().toUInt();

    Packet cmd;
    cmd.Write(searchId);
    cmd.Write(startAddress);
    cmd.Write(endAddress);
    cmd.Write(valueType);

    emit sendData(cmd);
}

void SearchTab::onResetButtonClicked()
{
    Packet cmd;

    quint32 clearId = static_cast<quint32>(Command::ID::ClearSearchFile);
    cmd.Write(clearId);

    emit sendData(cmd);  // シグナルで ConnectionTab に渡す
}

void SearchTab::onDataReceived(const Packet& packet)
{
    Packet data = packet;
    quint32 id = 0;
    data.Read(id);
    Command::ID cmdId = static_cast<Command::ID>(id);
    if(cmdId == Command::ID::CheckSearchFile)
    {
        quint32 enable = 0;
        data.Read(enable);
        if(enable != 0)
        {
            FileEnableCase();
        }
        else
        {
            FileDisableCase();
        }
    }
    else if(cmdId == Command::ID::DumpForSearch)
    {
        quint32 responseTypeId = 0;
        data.Read(responseTypeId);
        Command::Response responseType = static_cast<Command::Response>(responseTypeId);
        if(responseType == Command::Response::Progress)
        {
            float progress = 0.0f;
            data.Read(progress);
            ui->progressBar->setValue(progress * 100.0f);
        }
    }
}

void SearchTab::onTabOpened()
{
    // サーチファイルの有無を確認するためのコマンド
    Packet cmd;

    quint32 checkSearchFileId = static_cast<quint32>(Command::ID::CheckSearchFile);
    cmd.Write(checkSearchFileId);

    emit sendData(cmd);
}

void SearchTab::FileEnableCase()
{
    ui->comboBox_Type->setEnabled(true);
    ui->comboBox_ComparisonOperator->setEnabled(true);
    ui->lineEdit_Value->setEnabled(true);
}

void SearchTab::FileDisableCase()
{
    ui->comboBox_Type->setEnabled(false);
    ui->comboBox_ComparisonOperator->setEnabled(false);
    ui->lineEdit_Value->setEnabled(false);
}
