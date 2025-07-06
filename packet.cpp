#include "packet.h"

Packet::Packet() : cursor(0) {}
Packet::Packet(const QByteArray& data) : data(data), cursor(0) {}

const QByteArray& Packet::GetData() const
{
    return data;
}

void Packet::ResetCursor()
{
    cursor = 0;
}

void Packet::Append(const QByteArray& data)
{
    this->data.append(data);
}
