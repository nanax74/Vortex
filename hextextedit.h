#ifndef HEXTEXTEDIT_H
#define HEXTEXTEDIT_H

#include <QTextEdit>

class HexTextEdit : public QTextEdit
{
    Q_OBJECT

public:
    explicit HexTextEdit(QWidget* parent = nullptr);

protected:
    void keyPressEvent(QKeyEvent* event) override;

private slots:
    void onTextChanged();
};

#endif // HEXTEXTEDIT_H
