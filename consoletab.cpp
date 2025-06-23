#include "consoletab.h"
#include "ui_consoletab.h"

#include "utils.h"

ConsoleTab::ConsoleTab(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::ConsoleTab)
{
    ui->setupUi(this);

    connect(ui->pushButton_Send, &QPushButton::clicked, this, &ConsoleTab::onSendButtonClicked);

    // Enterキーでも送信できるように
    connect(ui->hexTextEdit_Send, &QTextEdit::textChanged, this, &ConsoleTab::onSendKeyPressed);
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

void ConsoleTab::onSendKeyPressed()
{
    QString text = ui->hexTextEdit_Send->toPlainText();
    if (text.endsWith('\n'))
    {
        onSendButtonClicked();  // Enterで送信
    }
}

void ConsoleTab::onDataReceived(const QString &data)
{
    // 受信欄にすでにあるデータを受信済みに移す
    QString previous = ui->hexTextEdit_Receive->toPlainText().trimmed();
    if (!previous.isEmpty())
    {
        ui->hexTextEdit_Received->append(previous);
    }

    // 新しいデータを受信欄に表示
    ui->hexTextEdit_Receive->setPlainText(data);
}
