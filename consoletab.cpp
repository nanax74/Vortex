#include "consoletab.h"
#include "ui_consoletab.h"

ConsoleTab::ConsoleTab(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::ConsoleTab)
{
    ui->setupUi(this);

    connect(ui->pushButton_Send, &QPushButton::clicked, this, &ConsoleTab::onSendButtonClicked);

    // Enterキーでも送信できるように
    connect(ui->textEdit_Send, &QTextEdit::textChanged, this, &ConsoleTab::onSendKeyPressed);
}

ConsoleTab::~ConsoleTab()
{
    delete ui;
}

void ConsoleTab::onSendButtonClicked()
{
    QString data = ui->textEdit_Send->toPlainText().trimmed();
    if (!data.isEmpty()) {
        emit sendData(data);  // シグナルで ConnectionTab に渡す

        // 送信済みログに追加
        ui->textEdit_Sent->append(data);

        // 送信欄をクリア
        ui->textEdit_Send->clear();
    }
}

void ConsoleTab::onSendKeyPressed()
{
    QString text = ui->textEdit_Send->toPlainText();
    if (text.endsWith('\n'))
    {
        onSendButtonClicked();  // Enterで送信
    }
}

void ConsoleTab::onDataReceived(const QString &data)
{
    // 受信欄にすでにあるデータを受信済みに移す
    QString previous = ui->textEdit_Receive->toPlainText().trimmed();
    if (!previous.isEmpty())
    {
        ui->textEdit_Received->append(previous);
    }

    // 新しいデータを受信欄に表示
    ui->textEdit_Receive->setPlainText(data);
}
