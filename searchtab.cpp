#include "searchtab.h"
#include "ui_searchtab.h"

#include <winsock2.h>

#include "command.h"
#include "packet.h"
#include "searchtablemodel.h"

SearchTab::SearchTab(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::SearchTab)
{
    ui->setupUi(this);
    connect(ui->pushButton_Search, &QPushButton::clicked, this, &SearchTab::onSearchButtonClicked);
    connect(ui->pushButton_Reset, &QPushButton::clicked, this, &SearchTab::onResetButtonClicked);

    ui->comboBox_Type->addItem("特定値", static_cast<quint32>(Command::ID::SpecificSearch));
    ui->comboBox_Type->addItem("不定値", static_cast<quint32>(Command::ID::UnknownSearch));

    ui->comboBox_ComparisonOperator->addItem("=", static_cast<quint32>(Command::Comparison::Equal));
    ui->comboBox_ComparisonOperator->addItem("!=", static_cast<quint32>(Command::Comparison::NotEqual));
    ui->comboBox_ComparisonOperator->addItem("<", static_cast<quint32>(Command::Comparison::LessThan));
    ui->comboBox_ComparisonOperator->addItem("<=", static_cast<quint32>(Command::Comparison::LessOrEqual));
    ui->comboBox_ComparisonOperator->addItem(">", static_cast<quint32>(Command::Comparison::GreaterThan));
    ui->comboBox_ComparisonOperator->addItem(">=", static_cast<quint32>(Command::Comparison::GreaterOrEqual));

    ui->comboBox_ValueType->addItem("int8", static_cast<quint32>(Command::ValueType::Int8));
    ui->comboBox_ValueType->addItem("uint8", static_cast<quint32>(Command::ValueType::UInt8));
    ui->comboBox_ValueType->addItem("int16", static_cast<quint32>(Command::ValueType::Int16));
    ui->comboBox_ValueType->addItem("uint16", static_cast<quint32>(Command::ValueType::UInt16));
    ui->comboBox_ValueType->addItem("int32", static_cast<quint32>(Command::ValueType::Int32));
    ui->comboBox_ValueType->addItem("uint32", static_cast<quint32>(Command::ValueType::UInt32));
    ui->comboBox_ValueType->addItem("int64", static_cast<quint32>(Command::ValueType::Int64));
    ui->comboBox_ValueType->addItem("uint64", static_cast<quint32>(Command::ValueType::UInt64));
    ui->comboBox_ValueType->addItem("float", static_cast<quint32>(Command::ValueType::Float));
    ui->comboBox_ValueType->addItem("double", static_cast<quint32>(Command::ValueType::Double));

    ui->progressBar->setValue(0.0f);

    ui->lineEdit_StartAddress->setText("10000000");
    ui->lineEdit_EndAddress->setText("20000000");

    ui->comboBox_ValueType->setCurrentText("float");

    tableModel = new SearchTableModel(this);
    ui->tableView_Search->setModel(tableModel);
    ui->tableView_Search->horizontalHeader()->setSectionResizeMode(QHeaderView::Stretch);

    nowValueUpdateTimer = new QTimer(this);
    nowValueUpdateTimer->setInterval(1000); // 1秒毎に更新

    connect(nowValueUpdateTimer, &QTimer::timeout, this, &SearchTab::onRequestNowValues);
}

SearchTab::~SearchTab()
{
    delete ui;
}

void SearchTab::onSearchButtonClicked()
{
    startStopwatch();
    ui->pushButton_Search->setEnabled(false);

    const quint32 startAddress = ui->lineEdit_StartAddress->text().toUInt(nullptr, 16);
    const quint32 endAddress = ui->lineEdit_EndAddress->text().toUInt(nullptr, 16);

    if(!isDumped)
    {
        const quint32 cmdId = static_cast<quint32>(Command::ID::DumpForSearch);
        const int valueType = ui->comboBox_ValueType->currentData().toUInt();

        Packet cmd;
        cmd.Write(cmdId);
        cmd.Write(startAddress);
        cmd.Write(endAddress);
        cmd.Write(valueType);

        emit sendData(cmd);
    }
    else
    {
        const quint32 cmdId = ui->comboBox_Type->currentData().toUInt();
        const int comparison = ui->comboBox_ComparisonOperator->currentData().toUInt();

        Packet cmd;
        cmd.Write(cmdId);
        cmd.Write(comparison);

        emit sendData(cmd);
    }

    tableModel->clear();
    nowValueUpdateTimer->stop();
}

void SearchTab::onResetButtonClicked()
{
    Packet cmd;

    quint32 clearId = static_cast<quint32>(Command::ID::ClearSearchFile);
    cmd.Write(clearId);

    emit sendData(cmd);  // シグナルで ConnectionTab に渡す

    FileDisableCase();
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
            if(progress < 1.0f)
            {
                ui->progressBar->setValue(progress * 100.0f);
            }
            else
            {
                QApplication::beep();
                ui->progressBar->setValue(0.0f);
                FileEnableCase();

                ui->pushButton_Search->setEnabled(true);
                stopStopwatch();
            }
        }
    }
    else if(cmdId == Command::ID::UnknownSearch)
    {
        quint32 responseTypeId = 0;
        data.Read(responseTypeId);
        Command::Response responseType = static_cast<Command::Response>(responseTypeId);
        if(responseType == Command::Response::Progress)
        {
            handleSearchProgress(data);
            nowValueUpdateTimer->start();
        }
        else if(responseType == Command::Response::Data)
        {
            quint32 address = 0;
            data.Read(address);
            float value = 0.0f; // Assuming float for now, need to handle different value types
            data.Read(value);

            tableModel->addPair(address, value);
        }
    }
    else if(cmdId == Command::ID::SpecificSearch) // Add this block for SpecificSearch
    {
        quint32 responseTypeId = 0;
        data.Read(responseTypeId);
        Command::Response responseType = static_cast<Command::Response>(responseTypeId);
        if(responseType == Command::Response::Progress)
        {
            handleSearchProgress(data);
            // For SpecificSearch, we might not need to start nowValueUpdateTimer here,
            // as it's for updating current values of found addresses.
            // The timer is usually started after a search is complete and results are displayed.
        }
        else if(responseType == Command::Response::Data)
        {
            quint32 address = 0;
            data.Read(address);
            float value = 0.0f; // Assuming float for now, need to handle different value types
            data.Read(value);

            tableModel->addPair(address, value);
        }
        else if(responseType == Command::Response::Success) // Handle success response
        {
            // Search completed successfully
            ui->pushButton_Search->setEnabled(true);
            stopStopwatch();
            // Optionally, update UI to indicate search completion
        }
    }
    else if (cmdId == Command::ID::ReadMemory)
    {
        quint32 address = 0;
        data.Read(address);
        float value = 0;
        data.Read(value);

        tableModel->updateNowValue(address, value);
    }
    else if (cmdId == Command::ID::ReadMultipleMemory)
    {
        quint32 responseTypeId = 0;
        data.Read(responseTypeId);
        Command::Response responseType = static_cast<Command::Response>(responseTypeId);
        if (responseType == Command::Response::Data)
        {
            quint32 numRegions = data.Read<quint32>();
            for (quint32 i = 0; i < numRegions; ++i)
            {
                quint32 address = data.Read<quint32>();
                quint32 size = data.Read<quint32>();
                // Assuming float for now, need to handle different value types
                if (size == sizeof(float))
                {
                    float value = data.Read<float>();
                    tableModel->updateNowValue(address, value);
                }
                else
                {
                    // Skip bytes for other types
                    for (quint32 j = 0; j < size; ++j)
                    {
                        data.Read<quint8>();
                    }
                }
            }
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

void SearchTab::onRequestNowValues()
{
    int totalRows = tableModel->rowCount();
    if (totalRows == 0 || totalRows > 100)
        return;  // 0件または100件超なら更新しない

    for (int i = 0; i < totalRows; ++i)
    {
        QModelIndex index = tableModel->index(i, 0); // アドレス列
        QString addrStr = tableModel->data(index, Qt::DisplayRole).toString();
        bool ok = false;
        quint32 address = addrStr.toUInt(&ok, 16);
        if (ok)
        {
            Packet cmd;
            cmd.Write(static_cast<quint32>(Command::ID::ReadMemory));
            cmd.Write(address);
            quint32 valueType = ui->comboBox_ValueType->currentData().toUInt(); // Get the selected ValueType
            cmd.Write(valueType); // Send the value type

            emit sendData(cmd);
        }
    }
}


void SearchTab::FileEnableCase()
{
    ui->comboBox_Type->setEnabled(true);
    ui->comboBox_ComparisonOperator->setEnabled(true);
    ui->lineEdit_Value->setEnabled(true);
    ui->pushButton_Search->setText("サーチ");
    isDumped = true;
}

void SearchTab::FileDisableCase()
{
    ui->comboBox_Type->setEnabled(false);
    ui->comboBox_ComparisonOperator->setEnabled(false);
    ui->lineEdit_Value->setEnabled(false);
    ui->pushButton_Search->setText("ダンプ");
    isDumped = false;
}

void SearchTab::startStopwatch()
{
    stopwatch.start(); // 計測開始

    if (!displayTimer)
    {
        displayTimer = new QTimer(this);
        connect(displayTimer, &QTimer::timeout, this, [=]() {
            double seconds = stopwatch.elapsed() / 1000.0;
            ui->label_Time->setText(QString::asprintf("(%.2f秒)", seconds));
        });
    }

    displayTimer->start(100);  // 表示更新を開始（0.1秒ごと）
}

void SearchTab::stopStopwatch()
{
    if (displayTimer)
    {
        displayTimer->stop();  // 表示更新を停止
    }

    double seconds = stopwatch.elapsed() / 1000.0;
    ui->label_Time->setText(QString::asprintf("(%.2f秒)", seconds)); // 最終値を表示
}
