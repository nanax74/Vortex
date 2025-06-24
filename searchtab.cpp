#include "searchtab.h"
#include "ui_searchtab.h"

SearchTab::SearchTab(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::SearchTab)
{
    ui->setupUi(this);
}

SearchTab::~SearchTab()
{
    delete ui;
}
