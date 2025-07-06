#ifndef CONSOLETAB_H
#define CONSOLETAB_H

#include <QWidget>

#include "packet.h"

namespace Ui {
class ConsoleTab;
}

class ConsoleTab : public QWidget
{
    Q_OBJECT

public:
    explicit ConsoleTab(QWidget *parent = nullptr);
    ~ConsoleTab();

public slots:
    void onDataReceived(const Packet& data); // ConnectionTabから受信
    void onDataSent(const Packet& data); // ConnectTabから受信

private slots:
    void onSendButtonClicked();

private:
    Ui::ConsoleTab *ui;

signals:
    void sendData(const Packet& data); // ConnectionTab に渡す用
};

#endif // CONSOLETAB_H
