using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Vortex
{
    public static class Data
    {
        const int MAX_DATA_SIZE = 0x10000; // データ本体の最大サイズ

        public static int SendAll(Socket socket, byte[] data)
        {
            try
            {
                int size = data.Length;
                Debug.WriteLine("Sending:{0}bytes", size);
                int sent = 0;

                while (sent < size)
                {
                    int bytes = socket.Send(data, sent, size - sent, SocketFlags.None);
                    if (bytes <= 0) // send関数の戻り値をチェック
                    {
                        if (bytes == 0)
                        {
                            Debug.WriteLine("Disconnect from server");
                            return 0;
                        }
                        else
                        {
                            Debug.WriteLine("Failed to send data");
                        }
                        return -1;
                    }
                    sent += bytes;
                }
                Debug.WriteLine("Sent:{0}bytes", data.Length);
                return sent;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to send data: {ex.Message}");
                return -1;
            }
        }

        public static int ReceiveAll(Socket socket, byte[] data, int size)
        {
            try
            {
                Debug.WriteLine("Receiving:{0}bytes", size);
                int received = 0;

                while (received < size)
                {
                    int bytes = socket.Receive(data, received, size - received, SocketFlags.None);
                    if (bytes <= 0) // send関数の戻り値をチェック
                    {
                        if (bytes == 0)
                        {
                            Debug.WriteLine("Disconnect from server");
                            return 0;
                        }
                        else
                        {
                            Debug.WriteLine("Failed to send data");
                        }
                        return -1;
                    }
                    received += bytes;
                }
                Debug.WriteLine("Received:{0}bytes", data.Length);
                return received;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to receive data: {ex.Message}");
                return -1;
            }
        }

        public static async Task<int> SendAllAsync(Socket socket, byte[] data)
        {
            try
            {
                int size = data.Length;
                Debug.WriteLine("Sending:{0}bytes", size);
                int sent = 0;

                while (sent < size)
                {
                    int bytes = await socket.SendAsync(new ArraySegment<byte>(data, sent, size - sent), SocketFlags.None);
                    if (bytes <= 0) // send関数の戻り値をチェック
                    {
                        if (bytes == 0)
                        {
                            Debug.WriteLine("Disconnect from server");
                            return 0;
                        }
                        else
                        {
                            Debug.WriteLine("Failed to send data");
                        }
                        return -1;
                    }
                    sent += bytes;
                }
                Debug.WriteLine("Sent:{0}bytes", data.Length);
                return sent;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to send data: {ex.Message}");
                return -1;
            }
        }

        public static async Task<int> ReceiveAllAsync(Socket socket, byte[] data, int size)
        {
            try
            {
                Debug.WriteLine("Receiving:{0}bytes", size);
                int received = 0;

                while (received < size)
                {
                    int bytes = await socket.ReceiveAsync(new ArraySegment<byte>(data, received, size - received), SocketFlags.None);
                    if (bytes <= 0) // send関数の戻り値をチェック
                    {
                        if (bytes == 0)
                        {
                            Debug.WriteLine("Disconnect from server");
                            return 0;
                        }
                        else
                        {
                            Debug.WriteLine("Failed to receive data");
                        }
                        return -1;
                    }
                    received += bytes;
                }
                Debug.WriteLine("Received:{0}bytes", data.Length);
                return received;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to receive data: {ex.Message}");
                return -1;
            }
        }

        public static async Task<int> SendPacketAsync(Socket socket, byte[] data, int type)
        {
            int size = data.Length;

            if (size == 0)
            {
                Debug.WriteLine("Data to send is empty");
                return -1;
            }
            else if (size > MAX_DATA_SIZE)
            {
                Debug.WriteLine("Data to send is too large:{0}", size);
                return -1;
            }

            byte[] header = new byte[8]; // 4バイトの type + 4バイトの size
            BitConverter.GetBytes(type).Reverse().ToArray().CopyTo(header, 0);
            BitConverter.GetBytes(data.Length).Reverse().ToArray().CopyTo(header, 4);

            // ヘッダを送信
            int result = await SendAllAsync(socket, header);
            if (result <= 0) return result;

            return await SendAllAsync(socket, data);
        }

        public static async Task<(int result, byte[] data, int type)> ReceivePacketAsync(Socket socket)
        {
            int headerSize = 8;
            byte[] header = new byte[headerSize];

            int result;

            result = await ReceiveAllAsync(socket, header, headerSize);
            if (result <= 0) return (result, new byte[0], 0);

            int type = BitConverter.ToInt32(header, 0);
            int size = BitConverter.ToInt32(header, 4);

            type = BinaryPrimitives.ReverseEndianness(type);
            size = BinaryPrimitives.ReverseEndianness(size);

            if(size > MAX_DATA_SIZE)
            {
                Debug.WriteLine("Data to receive is too large:{0}", size);
                return (-1, new byte[0], 0);
            }

            if(size == 0) return (-1, new byte[0], 0);

            byte[] data = new byte[size];
            result = await ReceiveAllAsync(socket, data, size);
            return (result, data, type);
        }
    }
}
