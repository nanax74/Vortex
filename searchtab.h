#ifndef SEARCHTAB_H
#define SEARCHTAB_H

#include <QWidget>
#include <QElapsedTimer>
#include <QTimer>

#include "packet.h"
#include "searchtablemodel.h"

namespace Ui {
class SearchTab;
}

class SearchTab : public QWidget
{
    Q_OBJECT

public:
    explicit SearchTab(QWidget *parent = nullptr);
    ~SearchTab();

public slots:
    void onDataReceived(const Packet& packet); // ConnectionTab から受信
    void onTabOpened();

private slots:
    void onSearchButtonClicked();
    void onResetButtonClicked();
    void onRequestNowValues();

private:
    Ui::SearchTab *ui;
    bool isDumped = false;
    QElapsedTimer stopwatch;
    QTimer *displayTimer;
    SearchTableModel* tableModel;
    QTimer* nowValueUpdateTimer = nullptr;

    void FileEnableCase();
    void FileDisableCase();
    void startStopwatch();
    void stopStopwatch();

signals:
    void sendData(const Packet& data); // ConnectionTab に渡す用
};

#endif // SEARCHTAB_H
