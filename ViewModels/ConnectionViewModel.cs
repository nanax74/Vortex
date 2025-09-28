using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vortex.Services;

namespace Vortex.ViewModels;

public partial class ConnectionViewModel : ObservableObject
{
    private readonly TcpClientService _tcpClientService = App.GetService<TcpClientService>();

    public ConnectionViewModel()
    {
        Func<bool> connectCanExecute = () => !_tcpClientService.IsConnected;
        ConnectCommand = new AsyncRelayCommand(Connect, connectCanExecute);

        Func<bool> disconnectCanExecute = () => _tcpClientService.IsConnected;
        DisconnectCommand = new AsyncRelayCommand(Disconnect, disconnectCanExecute);
    }

    public string IPAddress
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
            OnPropertyChanged(nameof(IPAddress));
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
            OnPropertyChanged(nameof(Port));
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
            OnPropertyChanged(nameof(TimeoutMs));
        }
    }

    public IAsyncRelayCommand ConnectCommand { get; }
    public IAsyncRelayCommand DisconnectCommand { get; }

    private async Task Connect()
    {
        await _tcpClientService.ConnectAsync();
        ConnectCommand.NotifyCanExecuteChanged();
        DisconnectCommand.NotifyCanExecuteChanged();
    }

    private async Task Disconnect()
    {
        await _tcpClientService.DisconnectAsync();
        ConnectCommand.NotifyCanExecuteChanged();
        DisconnectCommand.NotifyCanExecuteChanged();
    }
}