#ifndef CONNECTIONTAB_H
#define CONNECTIONTAB_H

#include <QTcpSocket>
#include <QWidget>

constexpr quint16 STARDUST_PORT = 50000;

namespace Ui {
class ConnectionTab;
}

class ConnectionTab : public QWidget
{
    Q_OBJECT
public:
    explicit ConnectionTab(QWidget *parent = nullptr);
    ~ConnectionTab();

private slots:
    void onConnectButtonClicked();
    void onDisconnectButtonClicked();
    void onConnected();
    void onDisconnected();
    void onReadyRead();
    void onErrorOccurred();

private:
    Ui::ConnectionTab *ui;
    QTcpSocket *socket;

signals:
};

#endif // CONNECTIONTAB_H
