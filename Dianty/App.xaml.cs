using Dianty.Models;
using Dianty.Services;
using Dianty.Utils;
using Dianty.ViewModels;
using Dianty.Views;
using Dianty.Views.Pages;
using DungeonToolkit.Coyote;
using GameMonitor;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Security.Cryptography;

namespace Dianty;
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private MainWindow? _window;

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
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var window = new MainWindow();
        window.AppWindow.Closing += OnAppWindowClosing;
        ToDoList.Plan += VersionHelper.Init;
        ToDoList.Plan += RegisterService;
        ToDoList.UiPlan += MergeDictionaries;
        ToDoList.UiPlan += SetWindowIcon;
        TopPageLocator.Init(GetTopPage);
        window.ViewModel = new MainViewModel(window);
        _window = window;
        _window.Activate();
    }

    private void OnAppWindowClosing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        ServiceLocator.Dispose();
    }

    private void RegisterService()
    {
        if (_window is null)
            return;

        ITitleBarService titleBarService = _window;
        IWindowService windowService = _window;
        IQueueService queueService = _window;
        ITemplateContent templateContent = _window;
        ServiceLocator.Register(titleBarService);
        ServiceLocator.Register(windowService);
        ServiceLocator.Register(queueService);
        ServiceLocator.Register(templateContent);
        ICoyoteBleDetector coyoteBleDetector = new CoyoteBleDetector();
        ServiceLocator.Register(coyoteBleDetector);
        ServiceLocator.Register<IMemoryService>(static () => new MemoryService());

        var coyoteManager = new CoyoteManager();
        var coyoteItems = new CoyoteCollection(coyoteManager);
        ServiceLocator.Register<ICoyoteListService>(coyoteItems);
        var gameManager = new GameManager(coyoteManager)
        {
            GtaVcGameRule = new GtaVcGameRule(ServiceLocator.GetService<IMemoryService>())
        };
        var gamesPageViewModel = new GamesPageViewModel(gameManager, queueService);
        ServiceLocator.Register(gamesPageViewModel);

        ServiceLocator.RegisterViewModel(typeof(HomePage), new HomePageViewModel(coyoteItems, gameManager, queueService));
        ServiceLocator.RegisterViewModel(typeof(DevicesPage), new DevicesPage.RequiredParameter(
            new DevicesPageViewModel(coyoteItems, queueService, coyoteBleDetector), queueService));
        ServiceLocator.RegisterViewModel(typeof(WavesPage), new WavesPageViewModel(coyoteManager, queueService, windowService));
        ServiceLocator.RegisterViewModel(typeof(GamesPage), new GamesPage.RequiredParameter(
            gamesPageViewModel, queueService));
        ServiceLocator.RegisterViewModel(typeof(SafetyPage), new SafetyPageViewModel(coyoteItems));
        ServiceLocator.RegisterViewModel(typeof(DebugPage), new DebugPageViewModel(queueService));
        ServiceLocator.RegisterViewModel(typeof(SettingsPage), new SettingsPageViewModel(queueService, windowService));
    }

    private void MergeDictionaries()
    {
        var newDictionary = new ResourceDictionary();
        LoadComponent(newDictionary, new Uri("ms-appx:///Resources/AppResourceDictionary.xaml", UriKind.Absolute));
        Resources.MergedDictionaries.Add(newDictionary);
    }

    private void SetWindowIcon()
    {
        if (_window is null)
            return;

        var file = "Assets/AppIcon.ico";
        try
        {
            var fileInfo = new FileInfo(file);
            if (fileInfo.Length > 64 * 1024)
                return;
            var fileData = File.ReadAllBytes(file);
            var sha256Value = SHA256.HashData(fileData);
            var expectedSha256Value = new byte[32] { 183, 38, 20, 203, 130, 196, 34, 183, 215, 152, 11, 232, 21, 199, 207, 241, 57, 6, 224, 120, 254, 193, 99, 100, 198, 42, 21, 178, 181, 150, 170, 1 };
            if (!sha256Value.SequenceEqual(expectedSha256Value))
                return;
            var sha1Value = SHA1.HashData(fileData);
            var expectedSha1Value = new byte[20] { 216, 127, 31, 62, 14, 60, 105, 190, 115, 186, 152, 104, 221, 230, 65, 198, 1, 208, 1, 155 };
            if (!sha1Value.SequenceEqual(expectedSha1Value))
                return;
        }
        catch
        {
            return;
        }
        _window.AppWindow.SetIcon(file);
    }

    private static Type? GetTopPage(string pageName)
    {
        return pageName switch
        {
            nameof(HomePage) => typeof(HomePage),
            nameof(DevicesPage) or nameof(CoyoteDetailPage) => typeof(DevicesPage),
            nameof(WavesPage) or nameof(WavePlayingQueuePage) or nameof(WaveSettingsPage) => typeof(WavesPage),
            nameof(GamesPage) => typeof(GamesPage),
            nameof(SafetyPage) => typeof(SafetyPage),
            nameof(DebugPage) => typeof(DebugPage),
            nameof(SettingsPage) => typeof(SettingsPage),
            _ => null
        };
    }
}
