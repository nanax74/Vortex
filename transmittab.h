#ifndef TRANSMITTAB_H
#define TRANSMITTAB_H

#include <QWidget>

namespace Ui {
class TransmitTab;
}

class TransmitTab : public QWidget
{
    Q_OBJECT

public:
    explicit TransmitTab(QWidget *parent = nullptr);
    ~TransmitTab();

private:
    Ui::TransmitTab *ui;
};

#endif // TRANSMITTAB_H
