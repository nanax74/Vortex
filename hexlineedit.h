#ifndef HEXLINEEDIT_H
#define HEXLINEEDIT_H

#include <QLineEdit>
#include <QRegularExpressionValidator>

#include <functional>

class HexLineEdit : public QLineEdit
{
    Q_OBJECT

public:
    explicit HexLineEdit(QWidget* parent = nullptr, int maxLength = 8, const QString& defaultText = QString());

    // 入力の正しさを判定する関数をセット
    void setValidationFunction(std::function<bool(const QString&)> func);

private slots:
    void onTextChanged(const QString& text);
    void focusOutEvent(QFocusEvent* event) override;
    void keyPressEvent(QKeyEvent* event) override;

private:
    QRegularExpressionValidator* hexValidator;
    int maxLength_;
    std::function<bool(const QString&)> validationFunc;
    void SetBorderColor(QString& text);
};

#endif // HEXLINEEDIT_H
