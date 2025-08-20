using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Vortex.Models;
using Vortex.Services;

namespace Vortex.ViewModels
{
    public class ConnectionViewModel : ObservableObject, IFeatureViewModel
    {
        public string DisplayName { get; } = "Connection";

        private readonly ConnectionModel _model;

        private readonly ServiceContext _context;

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (SetProperty(ref _isConnected, value))
                {
                    ConnectCommand.NotifyCanExecuteChanged();
                    DisconnectCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    ConnectCommand.NotifyCanExecuteChanged();
                    DisconnectCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public ConnectionViewModel(ServiceContext context)
        {
            _model = new Models.ConnectionModel();
            _context = context;

            ConnectCommand = new AsyncRelayCommand(ConnectAsync, CanConnect);
            DisconnectCommand = new RelayCommand(Disconnect, CanDisconnect);
        }

        public string IPAddress
        {
            get => _model.IPAddress;
            set
            {
                if(_model.IPAddress != value)
                {
                    _model.IPAddress = value;
                    OnPropertyChanged(nameof(IPAddress));
                }
            }
        }

        public IAsyncRelayCommand ConnectCommand { get; }
        public IRelayCommand DisconnectCommand { get; }

        private bool CanConnect() => !IsBusy && !IsConnected;
        private bool CanDisconnect() => !IsBusy && IsConnected;

        private async Task ConnectAsync()
        {
            IsBusy = true;
            try
            {
                _context.Logger.Log($"Connecting to {_model.IPAddress}:{_model.Port}...");
                await _context.Connection.ConnectAsync(_model.IPAddress, _model.Port);

                IsConnected = true;
                _context.Logger.Log($"Connected");
            }
            catch(Exception ex)
            {
                _context.Logger.Log($"Connection failed: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Disconnect()
        {
            IsBusy = true;
            try
            {
                _context.Connection.Disconnect();
                IsConnected = false;
                _context.Logger.Log($"Disconnected");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
