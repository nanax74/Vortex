#ifndef PACKET_H
#define PACKET_H

#include <QWidget>

class Packet
{
public:
    Packet();
    Packet(const QByteArray& data);

    template<typename T>
    void Write(const T& value);

    template<typename T>
    bool Read(T& value);

    const QByteArray& GetData() const;
    void ResetCursor();
    void Append(const QByteArray& packet);

private:
    QByteArray data;
    quint32 cursor = 0;
};

template<typename T>
void Packet::Write(const T& value)
{
    T be_value = qToBigEndian(value);
    const char* ptr = reinterpret_cast<const char*>(&be_value);
    data.append(ptr, sizeof(T));
}

template<typename T>
bool Packet::Read(T& value)
{
    if (cursor + sizeof(T) > static_cast<quint32>(data.size()))
        return false;

    const char* ptr = data.constData() + cursor;
    value = qFromBigEndian<T>(reinterpret_cast<const uchar*>(ptr));
    cursor += sizeof(T);
    return true;
}

#endif // PACKET_H
