#include "transmittab.h"
#include "ui_transmittab.h"

TransmitTab::TransmitTab(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::TransmitTab)
{
    ui->setupUi(this);
}

TransmitTab::~TransmitTab()
{
    delete ui;
}
