#ifndef HEXLINEEDIT_H
#define HEXLINEEDIT_H

#include <QLineEdit>
#include <QRegularExpressionValidator>

class HexLineEdit : public QLineEdit
{
    Q_OBJECT

public:
    explicit HexLineEdit(QWidget* parent = nullptr, int maxLength = 8);

private slots:
    void onTextChanged(const QString& text);

private:
    QRegularExpressionValidator* hexValidator;
    int maxLength_;
};

#endif // HEXLINEEDIT_H
