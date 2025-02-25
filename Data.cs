using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Vortex
{
    public static class Data
    {
        public static void SendData(Socket socket, byte[] data)
        {
            // データが空の場合中止
            if (data.Length == 0)
            {
                Debug.WriteLine("No data to send");
                return;
            }

            try
            {
                // 先にデータの長さを送信
                int dataLength = data.Length;
                byte[] dataLengthBytes = BitConverter.GetBytes(dataLength);
                int totalLengthSent = 0;
                while (totalLengthSent < dataLengthBytes.Length)
                {
                    int sent = socket.Send(dataLengthBytes, totalLengthSent, dataLengthBytes.Length - totalLengthSent, SocketFlags.None);
                    if (sent <= 0) // send関数の戻り値をチェック
                    {
                        if (sent == 0)
                        {
                            Debug.WriteLine("Connection closed by peer.");
                        }
                        else
                        {
                            Debug.WriteLine("Failed to send data length");
                        }
                        return;
                    }
                    totalLengthSent += sent;
                }

                int totalSent = 0;
                while (totalSent < data.Length)
                {
                    int sent = socket.Send(data, totalSent, Math.Min(4096, data.Length - totalSent), SocketFlags.None);
                    if (sent <= 0) // send関数の戻り値をチェック
                    {
                        if (sent == 0)
                        {
                            Debug.WriteLine("Connection closed by peer.");
                        }
                        else
                        {
                            Debug.WriteLine("Failed to send data");
                        }
                        return;
                    }
                    totalSent += sent;
                }
                Debug.WriteLine("Sent:{0}bytes", data.Length);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to send data: {ex.Message}");
            }
        }

        public static byte[] ReceiveData(Socket socket)
        {
            try
            {
                // 先にデータの長さを受信
                int dataLength = 0;
                byte[] dataLengthBytes = new byte[4];
                int totalLengthReceived = 0;
                while (totalLengthReceived < dataLengthBytes.Length)
                {
                    int received = socket.Receive(dataLengthBytes, totalLengthReceived, dataLengthBytes.Length - totalLengthReceived, SocketFlags.None);
                    if (received <= 0) // send関数の戻り値をチェック
                    {
                        if (received == 0)
                        {
                            Debug.WriteLine("Connection closed by peer.");
                        }
                        else
                        {
                            Debug.WriteLine("Failed to send data length");
                        }
                        return Array.Empty<byte>();
                    }
                    totalLengthReceived += received;
                }
                dataLength = BitConverter.ToInt32(dataLengthBytes.Reverse().ToArray(), 0);  // バイトオーダーを反転させつつint型に変換

                byte[] data = new byte[dataLength];
                int totalReceived = 0;
                while (totalReceived < dataLength)
                {
                    int received = socket.Receive(data, totalReceived, Math.Min(4096, dataLength - totalReceived), SocketFlags.None);
                    if (received <= 0) // send関数の戻り値をチェック
                    {
                        if (received == 0)
                        {
                            Debug.WriteLine("Connection closed by peer.");
                        }
                        else
                        {
                            Debug.WriteLine("Failed to send data");
                        }
                        return Array.Empty<byte>();
                    }
                    totalReceived += received;
                }
                Debug.WriteLine("Received:{0}bytes", data.Length);
                return data;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to receive data: {ex.Message}");
                return Array.Empty<byte>();
            }
        }

        public static async Task SendDataAsync(Socket socket, byte[] data)
        {
            // データが空の場合中止
            if (data.Length == 0)
            {
                Debug.WriteLine("No data to send");
                return;
            }

            try
            {
                // 先にデータの長さを送信
                int dataLength = data.Length;
                byte[] dataLengthBytes = BitConverter.GetBytes(dataLength);
                int totalLengthSent = 0;

                while (totalLengthSent < dataLengthBytes.Length)
                {
                    int sent = await socket.SendAsync(new ArraySegment<byte>(dataLengthBytes, totalLengthSent, dataLengthBytes.Length - totalLengthSent), SocketFlags.None);
                    if (sent <= 0) // send関数の戻り値をチェック
                    {
                        if (sent == 0)
                        {
                            Debug.WriteLine("Connection closed by peer.");
                        }
                        else
                        {
                            Debug.WriteLine("Failed to send data length");
                        }
                        return;
                    }
                    totalLengthSent += sent;
                }

                int totalSent = 0;
                while (totalSent < data.Length)
                {
                    int sent = await socket.SendAsync(new ArraySegment<byte>(data, totalSent, Math.Min(4096, data.Length - totalSent)), SocketFlags.None);
                    if (sent <= 0) // send関数の戻り値をチェック
                    {
                        if (sent == 0)
                        {
                            Debug.WriteLine("Connection closed by peer.");
                        }
                        else
                        {
                            Debug.WriteLine("Failed to send data");
                        }
                        return;
                    }
                    totalSent += sent;
                }
                Debug.WriteLine("Sent:{0}bytes", data.Length);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to send data: {ex.Message}");
            }
        }

        public static async Task<byte[]> ReceiveDataAsync(Socket socket)
        {
            try
            {
                // 先にデータの長さを受信
                int dataLength = 0;
                byte[] dataLengthBytes = new byte[4];
                int totalLengthReceived = 0;

                while (totalLengthReceived < dataLengthBytes.Length)
                {

                    int received = await socket.ReceiveAsync(new ArraySegment<byte>(dataLengthBytes, totalLengthReceived, dataLengthBytes.Length - totalLengthReceived), SocketFlags.None);
                    if (received <= 0) // receive関数の戻り値をチェック
                    {
                        if (received == 0)
                        {
                            Debug.WriteLine("Connection closed by peer.");
                        }
                        else
                        {
                            Debug.WriteLine("Failed to receive data length");
                        }
                        return Array.Empty<byte>();
                    }
                    totalLengthReceived += received;
                }
                dataLength = BitConverter.ToInt32(dataLengthBytes.Reverse().ToArray(), 0);  // バイトオーダーを反転させつつint型に変換

                byte[] data = new byte[dataLength];
                int totalReceived = 0;
                while (totalReceived < dataLength)
                {
                    int received = await socket.ReceiveAsync(new ArraySegment<byte>(data, totalReceived, Math.Min(4096, dataLength - totalReceived)), SocketFlags.None);
                    if (received <= 0) // receive関数の戻り値をチェック
                    {
                        if (received == 0)
                        {
                            Debug.WriteLine("Connection closed by peer.");
                        }
                        else
                        {
                            Debug.WriteLine("Failed to receive data");
                        }
                        return Array.Empty<byte>();
                    }
                    totalReceived += received;
                }
                Debug.WriteLine("Received:{0}bytes", data.Length);
                return data;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to receive data: {ex.Message}");
                return Array.Empty<byte>();
            }
        }
    }
}
