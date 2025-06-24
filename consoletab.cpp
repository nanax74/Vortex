#include "consoletab.h"
#include "ui_consoletab.h"

#include "utils.h"

ConsoleTab::ConsoleTab(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::ConsoleTab)
{
    ui->setupUi(this);

    connect(ui->pushButton_Send, &QPushButton::clicked, this, &ConsoleTab::onSendButtonClicked);
}

ConsoleTab::~ConsoleTab()
{
    delete ui;
}

void ConsoleTab::onSendButtonClicked()
{
    QString text = ui->hexTextEdit_Send->toPlainText().trimmed();
    if (!text.isEmpty())
    {
        QString hexString = filterHex(text);
        QByteArray data = QByteArray::fromHex(hexString.toUtf8());
        emit sendData(data);  // シグナルで ConnectionTab に渡す

        // 送信済みログに追加
        ui->hexTextEdit_Sent->append(text);

        // 送信欄をクリア
        ui->hexTextEdit_Send->clear();
    }
}

void ConsoleTab::onDataReceived(const QByteArray &data)
{
    // 受信欄にすでにあるデータを受信済みに移す
    QString previous = ui->hexTextEdit_Receive->toPlainText().trimmed();
    if (!previous.isEmpty())
    {
        ui->hexTextEdit_Received->append(previous);
    }

    // 新しいデータを受信欄に表示
    QString text = QString::fromUtf8(data.toHex());
    ui->hexTextEdit_Receive->setPlainText(text);
}
