using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vortex.Services;
using VortexLib;

namespace Vortex.ViewModels;

public partial class ConnectionViewModel : ObservableObject
{
    private readonly TcpClientService _tcpClientService = App.GetService<TcpClientService>();

    public string IpAddress
    {
        get
        {
            if (_tcpClientService == null) return "255.255.255.255";
            return _tcpClientService.Host;
        }

        set
        {
            if(_tcpClientService == null) return;
            _tcpClientService.Host = value;
        }
    }

    public int Port
    {
        get
        {
            if (_tcpClientService == null) return 0;
            return _tcpClientService.Port;
        }

        set
        {
            if (_tcpClientService == null) return;
            _tcpClientService.Port = value;
        }
    }

    public int TimeoutMs
    {
        get
        {
            if (_tcpClientService == null) return 0;
            return _tcpClientService.TimeoutMs;
        }

        set
        {
            if (_tcpClientService == null) return;
            _tcpClientService.TimeoutMs = value;
        }
    }

    [RelayCommand]
    private async Task Connect()
    {
        await _tcpClientService.ConnectAsync();
    }

    [RelayCommand]
    private async Task Disconnect()
    {
        await _tcpClientService.DisconnectAsync();
    }
}