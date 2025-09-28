using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Vortex.Services;
using Vortex.ViewModels;
using WinUIEx;

namespace Vortex;

public partial class App : Application
{
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    private WindowEx? _window;

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host
        .CreateDefaultBuilder()
        .UseContentRoot(AppContext.BaseDirectory)
        .ConfigureServices((context, services) =>
        {
            services.AddSingleton<TcpClientService>();

            services.AddSingleton<ConnectionViewModel>();
            services.AddSingleton<StreamViewModel>();
        }).Build();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}
