#include "vortex.h"

#include <QApplication>
#include <QLocale>
#include <QTranslator>

int main(int argc, char *argv[])
{
    QApplication a(argc, argv);

    QTranslator translator;
    const QStringList uiLanguages = QLocale::system().uiLanguages();
    for (const QString &locale : uiLanguages) {
        const QString baseName = "Vortex_" + QLocale(locale).name();
        if (translator.load(":/i18n/" + baseName)) {
            a.installTranslator(&translator);
            break;
        }
    }

    QApplication::setStyle("windowsvista");

    QPalette pal = qApp->palette();

    // 選択中の背景とテキスト色を設定
    pal.setColor(QPalette::Highlight, QColor(51, 153, 255));
    pal.setColor(QPalette::HighlightedText, QColor(255, 255, 255));

    qApp->setPalette(pal);

    Vortex w;
    w.show();
    return a.exec();
}
