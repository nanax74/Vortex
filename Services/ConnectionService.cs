using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vortex.Services
{
    public class ConnectionService
    {
        private Socket? socket;

        public async Task ConnectAsync(string ipAddress, int port, int timeoutMs = 3000)
        {
            if (socket != null && socket.Connected)
            {
                socket.Close();
                socket.Dispose();
            }

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            using var cts = new CancellationTokenSource(timeoutMs);

            try
            {
                await socket.ConnectAsync(ipAddress, port, cts.Token);
            }
            catch
            {
                if (socket != null)
                {
                    socket.Close();
                    socket.Dispose();
                    socket = null;
                }
                throw;
            }
        }

        public void Disconnect()
        {
            if (socket != null && socket.Connected)
            {
                socket.Close();
                socket.Dispose();
            }
            socket = null;
        }
    }
}
