using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Vortex
{
    public static class Command
    {
        static public async Task<uint[]> ReadUInt32(Socket socket, uint address, int size)
        {
            string command = string.Format(string.Format("00{0:X8}{1:X8}", address, size));
            byte[] dataInput = ConvertEx.ToBytes(command);
            Packet sendPacket = new Packet(dataInput, Packet.DATA_TYPE_DATA);
            await sendPacket.SendPacketAsync(socket);
            Packet receivePacket = new Packet();
            await receivePacket.ReceivePacketAsync(socket);
            return ConvertEx.ToUInt32Array(receivePacket.Data);
        }
    }
}
