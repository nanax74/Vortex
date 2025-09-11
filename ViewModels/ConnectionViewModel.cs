using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VortexLib;

namespace Vortex.ViewModels;

public partial class ConnectionViewModel : ObservableObject
{
    private readonly TCPClient _client = new();

    private string _ipAddress = "0.0.0.0";

    public string IpAddress
    {
        get => _ipAddress;
        set => SetProperty(ref _ipAddress, value);
    }

    private const int _port = 50000;

    [RelayCommand]
    private async Task Connect()
    {
        if (IpAddress == string.Empty) return;

        try
        {
            await _client.ConnectAsync(IpAddress, _port, TimeSpan.FromSeconds(3.0));
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Failed to connect to server. {ex}");
        }
    }

    [RelayCommand]
    private void Disconnect()
    {
        try
        {
            _client.Disconnect();
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Failed to disconnect to server. {ex}");
        }
    }
}