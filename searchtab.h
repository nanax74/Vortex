#ifndef SEARCHTAB_H
#define SEARCHTAB_H

#include <QWidget>

#include "packet.h"

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

private:
    Ui::SearchTab *ui;

    void FileEnableCase();
    void FileDisableCase();

signals:
    void sendData(const Packet& data); // ConnectionTab に渡す用
};

#endif // SEARCHTAB_H
