#include "vortex.h"
#include "ui_vortex.h"

#include "connectiontab.h"

Vortex::Vortex(QWidget *parent)
    : QMainWindow(parent)
    , ui(new Ui::Vortex)
{
    ui->setupUi(this);

    ConnectionTab* connectionTab = new ConnectionTab(this);
    ui->tabWidget_Main->addTab(connectionTab, tr("接続"));
}

Vortex::~Vortex()
{
    delete ui;
}
