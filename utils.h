#ifndef UTILS_H
#define UTILS_H

#include <QWidget>
#include <QLineEdit>
#include <QRegularExpression>

inline QString filterHex(const QString& input)
{
    QString filtered = input;
    static QRegularExpression hex("[^0-9A-Fa-f]");
    filtered.remove(hex);
    return filtered;
}

#endif // UTILS_H
