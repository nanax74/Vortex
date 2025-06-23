#include "hexlineedit.h"

HexLineEdit::HexLineEdit(QWidget* parent, int maxLength)
    : QLineEdit(parent), maxLength_(maxLength)
{
    // 正規表現：[0-9A-Fa-f] のみ、最大 maxLength 桁
    QRegularExpression hexRegex(QString("[0-9A-Fa-f]{0,%1}").arg(maxLength_));
    hexValidator = new QRegularExpressionValidator(hexRegex, this);
    this->setValidator(hexValidator);

    // 入力最大長
    this->setMaxLength(maxLength_);

    // 小文字を自動で大文字に変換
    connect(this, &QLineEdit::textChanged, this, &HexLineEdit::onTextChanged);
}

void HexLineEdit::onTextChanged(const QString& text)
{
    QString upper = text.toUpper();
    if (text != upper)
    {
        int cursorPos = this->cursorPosition();
        this->setText(upper);
        this->setCursorPosition(cursorPos); // カーソル位置を維持
    }
}
