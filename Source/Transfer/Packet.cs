using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Vortex
{
    public class Packet
    {
        public const int DATA_TYPE_DATA = 0x00; // コマンドや応答
        public const int DATA_TYPE_PROGRESS = 0x01; // 進捗
        public const int DATA_TYPE_ERROR = 0xFF; // 空のデータ

        private const int MaxPacketSize = 1024 * 1024;
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public int Type { get; set; } = DATA_TYPE_ERROR;

        public Packet() { }

        public Packet(byte[] data, int type)
        {
            Data = data;
            Type = type;
        }

        public async Task<int> SendPacketAsync(Socket socket)
        {
            int size = Data.Length;

            if (size == 0)
            {
                Debug.WriteLine("Data to send is empty");
                return -1;
            }
            else if (size > MaxPacketSize)
            {
                Debug.WriteLine("Data to send is too large: {0}", size);
                return -1;
            }

            byte[] header = new byte[8];
            BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(0, 4), size);
            BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4, 4), Type);

            int result = await Transfer.SendAllAsync(socket, header, header.Length);
            if (result <= 0) return result;

            return await Transfer.SendAllAsync(socket, Data, Data.Length);
        }

        public async Task<int> ReceivePacketAsync(Socket socket)
        {
            byte[] header = new byte[8];
            int result = await Transfer.ReceiveAllAsync(socket, header, header.Length);
            if (result <= 0) return result;

            int size = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(0, 4));
            Type = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(4, 4));

            if (size > MaxPacketSize)
            {
                Debug.WriteLine("Data to receive is too large: {0}", size);
                return -1;
            }

            if (size == 0)
            {
                Debug.WriteLine("Data to receive is empty");
                return -1;
            }

            Data = new byte[size];
            return await Transfer.ReceiveAllAsync(socket, Data, size);
        }
    }
}
