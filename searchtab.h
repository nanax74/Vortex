#ifndef SEARCHTAB_H
#define SEARCHTAB_H

#include <QWidget>

namespace Ui {
class SearchTab;
}

class SearchTab : public QWidget
{
    Q_OBJECT

public:
    explicit SearchTab(QWidget *parent = nullptr);
    ~SearchTab();

private:
    Ui::SearchTab *ui;
};

#endif // SEARCHTAB_H
