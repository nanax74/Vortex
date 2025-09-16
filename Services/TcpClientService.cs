using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using VortexLib;
using Windows.Services.Maps;

namespace Vortex.Services;

public class TcpClientService
{
    private readonly MessageClient _client = new();

    private string _host = "0.0.0.0";
    public IPAddress IPAddress
    {
        get
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_host)) return IPAddress.None;
                IPAddress ipAddress = IPAddress.Parse(_host);
                if (ipAddress == null) return IPAddress.None;
                return ipAddress;
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"[TcpClientService] {ex}");
                return IPAddress.None;
            }
        }

        set => _host = value.ToString();
    }

    public string Host
    {
        get => _host;
        set => _host = value;
    }

    public int Port { get; set; } = 50000;

    public int TimeoutMs { get; set; } = 1000 * 3;

    public async Task ConnectAsync()
    {
        if (IPAddress == IPAddress.None) return;

        try
        {
            await _client.ConnectAsync(IPAddress, Port, TimeSpan.FromMilliseconds(TimeoutMs));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to connect to server. {ex}");
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            await _client.DisconnectAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to disconnect to server. {ex}");
        }
    }
}