using Dianty.Models;
using Dianty.Resources;
using Dianty.Services;
using Dianty.ViewModels;
using Dianty.Views.Pages;
using GameMonitor;
using Microsoft.UI.Xaml;
using System;

namespace Dianty;
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? _window;
    private MainWindow? _mainWindow;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        var window = new MainWindow();
        ServiceLocator.Init(RegisterService);
        ResourceLoader.Init(MergedDictionaries);
        window.ViewModel = new MainViewModel(window);
        _mainWindow = window;
        _window = window;
        _window.Activate();
    }

    private void RegisterService()
    {
        if (_mainWindow is null)
            return;

        ITitleBarService titleBarService = _mainWindow;
        IWindowService windowService = _mainWindow;
        IQueueService queueService = _mainWindow;
        ITemplateContent templateContent = _mainWindow;
        ServiceLocator.Register(titleBarService);
        ServiceLocator.Register(windowService);
        ServiceLocator.Register(queueService);
        ServiceLocator.Register(templateContent);
        ServiceLocator.Register<IMemoryService>(static () => new MemoryService());

        ServiceLocator.RegisterViewModel(typeof(DebugPage), new DebugPageViewModel(queueService));
        ServiceLocator.RegisterViewModel(typeof(GamesPage), new GamesPage.RequiredParameter(
            new GamesPageViewModel(ServiceLocator.GetService<IMemoryService>(), queueService),
            queueService));
    }

    private void MergedDictionaries()
    {
        var newDictionary = new ResourceDictionary();
        LoadComponent(newDictionary, new Uri("ms-appx:///Resources/AppResourceDictionary.xaml", UriKind.Absolute));
        Resources.MergedDictionaries.Add(newDictionary);
    }
}
