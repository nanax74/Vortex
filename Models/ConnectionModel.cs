using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Vortex.Models
{
    public class ConnectionModel
    {
        public string IPAddress { get; set; } = "192.168.68.104"; // WiiUのIPアドレス

        public int Port { get; set; } = 50000; // Stardustのポート
    }
}
