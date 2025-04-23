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
            string command = string.Format(string.Format($"00{address:X8}{size:X8}"));
            byte[] dataInput = ConvertEx.ToBytes(command);
            Packet sendPacket = new Packet(dataInput, Packet.DATA_TYPE_DATA);
            await sendPacket.SendPacketAsync(socket);
            Packet receivePacket = new Packet();
            await receivePacket.ReceivePacketAsync(socket);
            if(receivePacket.Type == Packet.DATA_TYPE_DATA)
            {
                return receivePacket.Data;
            }
            return Array.Empty<byte>();
        }

        static public async Task<byte[]> ReadBytesBigEndian(Socket socket, uint address, int size)
        {
            return ConvertEx.ReverseEndianPerBlock(await ReadBytes(socket, address, size), 4);
        }

        static public async Task<uint[]> ReadUInt32s(Socket socket, uint address, int size)
        {
            return ConvertEx.ToUInt32Array(await ReadBytes(socket, address, size));
        }

        static public async Task WriteKernel(Socket socket, uint address, uint[] value)
        {
            string valueString = string.Concat(value.Select(x => x.ToString("X8")));
            string command = string.Format(string.Format($"05{address:X8}{valueString}"));
            byte[] dataInput = ConvertEx.ToBytes(command);
            Packet sendPacket = new Packet(dataInput, Packet.DATA_TYPE_DATA);
            await sendPacket.SendPacketAsync(socket);
            Packet receivePacket = new Packet();
            await receivePacket.ReceivePacketAsync(socket);
            return;
        }
    }
}
