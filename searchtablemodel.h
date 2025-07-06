#ifndef SEARCHTABLEMODEL_H
#define SEARCHTABLEMODEL_H

#include <QAbstractTableModel>
#include <QPair>
#include <QList>

class SearchTableModel : public QAbstractTableModel
{
    Q_OBJECT

public:
    SearchTableModel(QObject *parent = nullptr);

    void setDataList(const QList<QPair<quint32, float>>& list);
    void addPair(quint32 address, float value);
    void clear();
    void updateNowValue(quint32 address, float newValue);

    int rowCount(const QModelIndex &parent = QModelIndex()) const override;
    int columnCount(const QModelIndex &parent = QModelIndex()) const override;
    QVariant data(const QModelIndex &index, int role = Qt::DisplayRole) const override;
    QVariant headerData(int section, Qt::Orientation orientation, int role) const override;

private:
    QList<QPair<quint32, float>> m_data;

    QMap<quint32, float> m_nowValueCache;  // <アドレス, 現在値> のキャッシュ
};

#endif // SEARCHTABLEMODEL_H
