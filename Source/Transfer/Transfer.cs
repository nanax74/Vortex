using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Vortex
{
    public static class Transfer
    {
        public static async Task<int> SendAllAsync(Socket socket, byte[] data)
        {
            try
            {
                int size = data.Length;
                Debug.WriteLine("Sending: {0} bytes", size);

                // データをすべて送信するまでループ
                int sent = 0;
                while (sent < size)
                {
                    int bytes = await socket.SendAsync(new ArraySegment<byte>(data, sent, size - sent), SocketFlags.None);
                    if (bytes <= 0)
                    {
                        if(bytes == 0)
                        {
                            Debug.WriteLine("Disconnect from server");
                            return 0;
                        }
                        else
                        {
                            Debug.WriteLine("Failed to send data");
                            return -1;
                        }
                    }
                    sent += bytes;
                }
                Debug.WriteLine("Sent: {0} bytes", sent);
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
                Debug.WriteLine("Receiving: {0} bytes", size);

                // データをすべて受信するまでループ
                int received = 0;
                while (received < size)
                {
                    int bytes = await socket.ReceiveAsync(new ArraySegment<byte>(data, received, size - received), SocketFlags.None);
                    if (bytes <= 0)
                    {
                        if (bytes == 0)
                        {
                            Debug.WriteLine("Disconnect from server");
                            return 0;
                        }
                        else
                        {
                            Debug.WriteLine("Failed to send data");
                            return -1;
                        }
                    }
                    received += bytes;
                }
                Debug.WriteLine("Received: {0} bytes", received);
                return received;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to receive data: {ex.Message}");
                return -1;
            }
        }
    }
}
