#include "hextextedit.h"

#include <QKeyEvent>
#include <QRegularExpression>

HexTextEdit::HexTextEdit(QWidget* parent)
    : QTextEdit(parent)
{
    connect(this, &QTextEdit::textChanged, this, &HexTextEdit::onTextChanged);
}

void HexTextEdit::onTextChanged()
{
    // 入力を取得して、大文字化＋無効文字除去
    QString currentText = this->toPlainText().toUpper();

    // 正規表現で 0-9A-F のみ抽出
    QString filtered;
    for (QChar& c : currentText)
    {
        if (c.isDigit() || (c >= 'A' && c <= 'F'))
            filtered.append(c);
    }

    if (this->toPlainText() != filtered)
    {
        this->blockSignals(true);  // 無限ループ防止
        this->setPlainText(filtered);
        this->moveCursor(QTextCursor::End); // カーソルを末尾に
        this->blockSignals(false);
    }
}

void HexTextEdit::keyPressEvent(QKeyEvent* event)
{
    // 改行キーを無視する（必要であれば）
    if (event->key() == Qt::Key_Return || event->key() == Qt::Key_Enter)
    {
        event->ignore(); // 改行禁止
        return;
    }

    QTextEdit::keyPressEvent(event); // 通常処理
}
