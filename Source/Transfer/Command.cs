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
        static public async Task<byte[]> ReadBytes(Socket socket, uint address, int size)
        {
            string command = string.Format(string.Format("00{0:X8}{1:X8}", address, size));
            byte[] dataInput = ConvertEx.ToBytes(command);
            Packet sendPacket = new Packet(dataInput, Packet.DATA_TYPE_DATA);
            await sendPacket.SendPacketAsync(socket);
            Packet receivePacket = new Packet();
            await receivePacket.ReceivePacketAsync(socket);
            return receivePacket.Data;
        }

        static public async Task<byte[]> ReadBytesBigEndian(Socket socket, uint address, int size)
        {
            return ConvertEx.ReverseEndian(await ReadBytes(socket, address, size), 4);
        }

        static public async Task<uint[]> ReadUInt32s(Socket socket, uint address, int size)
        {
            return ConvertEx.ToUInt32Array(await ReadBytes(socket, address, size));
        }
    }
}
