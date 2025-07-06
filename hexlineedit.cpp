#include "hexlineedit.h"

#include <QKeyEvent>

HexLineEdit::HexLineEdit(QWidget* parent, int maxLength, const QString& defaultText)
    : QLineEdit(parent), maxLength_(maxLength)
{
    // 正規表現：[0-9A-Fa-f] のみ、最大 maxLength 桁
    QRegularExpression hexRegex(QString("[0-9A-Fa-f]{0,%1}").arg(maxLength_));
    hexValidator = new QRegularExpressionValidator(hexRegex, this);
    this->setValidator(hexValidator);

    // 入力最大長
    this->setMaxLength(maxLength_);

    QString text;
    if(!defaultText.isEmpty())
    {
        text = defaultText;
    }
    else
    {
        text = "0";
    }
    this->setText(text);

    // 小文字を自動で大文字に変換
    connect(this, &QLineEdit::textChanged, this, &HexLineEdit::onTextChanged);

    validationFunc = [](const QString&) { return true; };

    SetBorderColor(text);
}

void HexLineEdit::setValidationFunction(std::function<bool(const QString&)> func)
{
    validationFunc = std::move(func);
}

void HexLineEdit::onTextChanged(const QString& text)
{
    QString upper = text.toUpper();
    if (text != upper)
    {
        int cursorPos = this->cursorPosition();
        this->setText(upper);
        this->setCursorPosition(cursorPos); // カーソル位置を維持
        return;
    }

    SetBorderColor(upper);
}

void HexLineEdit::SetBorderColor(QString& text)
{
    // バリデーション関数に基づいて色を変える
    if (validationFunc(text))
    {
        this->setStyleSheet("QLineEdit { border: 2px solid rgb(0,255,0); }");
    }
    else
    {
        this->setStyleSheet("QLineEdit { border: 2px solid rgb(255,0,0); }");
    }
}

// フォーカスを失ったときに "0" を自動で入力する
void HexLineEdit::focusOutEvent(QFocusEvent* event)
{
    if (this->text().isEmpty())
    {
        this->setText("0");
    }

    // 親クラスの処理を忘れずに呼び出す
    QLineEdit::focusOutEvent(event);
}

// Enterキーでフォーカスを外す
void HexLineEdit::keyPressEvent(QKeyEvent* event)
{
    if (event->key() == Qt::Key_Return || event->key() == Qt::Key_Enter)
    {
        this->clearFocus();  // フォーカスを外す
        event->accept();     // イベント処理済みにする
        return;
    }
    // それ以外は親の実装に任せる
    QLineEdit::keyPressEvent(event);
}
