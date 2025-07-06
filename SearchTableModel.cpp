#include "searchtablemodel.h"

SearchTableModel::SearchTableModel(QObject *parent)
    : QAbstractTableModel(parent)
{
}

void SearchTableModel::setDataList(const QList<QPair<quint32, float>>& list)
{
    beginResetModel();
    m_data = list;
    endResetModel();
}

void SearchTableModel::addPair(quint32 address, float value)
{
    beginInsertRows(QModelIndex(), m_data.size(), m_data.size());
    m_data.append(qMakePair(address, value));
    endInsertRows();
}

void SearchTableModel::clear()
{
    beginResetModel();
    m_data.clear();
    endResetModel();
}

void SearchTableModel::updateNowValue(quint32 address, float newValue)
{
    m_nowValueCache[address] = newValue;

    for (int row = 0; row < m_data.size(); ++row)
    {
        if (m_data[row].first == address)
        {
            QModelIndex index = this->index(row, 2);
            emit dataChanged(index, index);
            break;
        }
    }
}


int SearchTableModel::rowCount(const QModelIndex &) const
{
    return m_data.size();
}

int SearchTableModel::columnCount(const QModelIndex &) const
{
    return 3; // Address, Value, Now Value
}

QVariant SearchTableModel::data(const QModelIndex &index, int role) const
{
    if (!index.isValid() || role != Qt::DisplayRole)
        return QVariant();

    const auto& pair = m_data[index.row()];
    if (index.column() == 0)
        return QString("%1").arg(pair.first, 8, 16, QChar('0')).toUpper();   // quint32
    else if (index.column() == 1)
        return QString::number(pair.second, 'f', 2); // float, 小数点以下2桁表示
    else if (index.column() == 2)
    {
        const quint32 address = pair.first;
        if (m_nowValueCache.contains(address))
            return QString::number(m_nowValueCache[address], 'f', 2);
        else
            return "...";  // 未取得時のプレースホルダー
    }
    return QVariant();
}

QVariant SearchTableModel::headerData(int section, Qt::Orientation orientation, int role) const
{
    if (role != Qt::DisplayRole)
        return QVariant();

    if (orientation == Qt::Horizontal)
    {
        if (section == 0) return "Address";
        if (section == 1) return "Value";
        if (section == 2) return "Now Value";
    }

    return QVariant();
}
