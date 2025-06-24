#ifndef CONSOLETAB_H
#define CONSOLETAB_H

#include <QWidget>

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
    void onDataReceived(const QByteArray &data); // ConnectionTab から受信
private slots:
    void onSendButtonClicked();

private:
    Ui::ConsoleTab *ui;

signals:
    void sendData(const QByteArray &data); // ConnectionTab に渡す用
};

#endif // CONSOLETAB_H
