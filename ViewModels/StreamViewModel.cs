using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vortex.Services;

namespace Vortex.ViewModels;

public partial class StreamViewModel : ObservableObject
{
    private readonly TcpClientService _tcpClientService = App.GetService<TcpClientService>();

    public StreamViewModel()
    {
        Func<bool> clearSentCanExecute = () => { return DataSent.Count > 0; };
        ClearSentCommand = new RelayCommand(ClearSent, clearSentCanExecute);
        DataSent.CollectionChanged += (_, __) => ClearSentCommand.NotifyCanExecuteChanged();

        Func<bool> clearReceivedCanExecute = () => { return DataReceived.Count > 0; };
        ClearReceivedCommand = new RelayCommand(ClearReceived, clearReceivedCanExecute);
        DataReceived.CollectionChanged += (_, __) => ClearReceivedCommand.NotifyCanExecuteChanged();

        Func<bool> sendDataCanExecute = () => { return DataBuffer?.Length > 0; };
        SendDataCommand = new RelayCommand(SendData, sendDataCanExecute);
    }

    private byte[]? _dataBuffer;
    public byte[]? DataBuffer
    {
        get => _dataBuffer;
        set
        {
            if (SetProperty(ref _dataBuffer, value))
            {
                SendDataCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public ObservableCollection<string> DataSent { get; } = new();
    public ObservableCollection<string> DataReceived { get; } = new();

    public IRelayCommand ClearSentCommand { get; }
    public IRelayCommand ClearReceivedCommand { get; }
    public IRelayCommand SendDataCommand { get; }

    private void ClearSent() => DataSent.Clear();

    private void ClearReceived() => DataReceived.Clear();

    private void SendData()
    {
        if (DataBuffer is { Length: > 0 })
            _tcpClientService.Send(DataBuffer);
    }
}
