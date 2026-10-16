using Dianty.Localization;
using Dianty.Models;
using Dianty.Services;
using Dianty.Utils;
using Dianty.ViewModels;
using Dianty.Views.Controls;
using Dianty.Views.Pages;
using DungeonToolkit;
using DungeonToolkit.Coyote;
using GameMonitor;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Dianty;
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private MainWindow? _window;
    private Dictionary<Type, object>? _viewModelTable;
    private Action? _disposeAction;
    private bool _isUnhandledExceptionOccurred;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
        UnhandledException += Shutdown;
    }

    private async void Shutdown(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        if (_isUnhandledExceptionOccurred)
            return;
        _isUnhandledExceptionOccurred = true;

        var dialog = _window?.CreateContentDialog();
        if (dialog == null)
            return;
        var dialogText = Localizer.Instance?.AppText.MainWindowText.CrashDialogText;
        dialog.Title = dialogText?.Title ?? "Something went wrong";
        dialog.CloseButtonText = dialogText?.CloseButtonText ?? "Exit";
        dialog.IsPrimaryButtonEnabled = false;
        dialog.IsSecondaryButtonEnabled = false;
        dialog.DefaultButton = ContentDialogButton.Close;
        dialog.Content = e.Message;
        await dialog.ShowAsync();
        _window?.Close();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var window = new MainWindow();
        window.AppWindow.Closing += OnAppWindowClosing;
        _window = window;
        _window.Activate();
        Task.Run(InitializeAsync);
    }

    private void OnAppWindowClosing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        _disposeAction?.Invoke();
        _disposeAction = null;
    }

    private async Task InitializeAsync()
    {
        Debug.Assert(_window is not null);
        Debug.Assert(_window.DispatcherQueue is not null);
        Debug.Assert(!_window.DispatcherQueue.HasThreadAccess);
#if DEBUG
        var stopwatch = Stopwatch.StartNew();
#endif

        var delayTask = Task.Delay(300); // 最短加载时间
        ITitleBarService titleBarService = _window;
        IWindowInputService windowInputService = _window;
        IQueueService queueService = _window;
        ILocalizationService localizationService = Localizer.Init();
        var viewFactory = new ViewFactory(titleBarService, windowInputService, queueService, localizationService, GetViewModel, GetTopPageType);
        var mainViewModel = new MainViewModel(viewFactory);
        queueService.TryEnqueue(() => _window.ViewModel = mainViewModel);

        queueService.TryEnqueue(MergeDictionaries);
        queueService.TryEnqueue(SetWindowIcon);

        InitializeService();
        RegisterDisposableInstance(viewFactory);
        RegisterDisposableInstance(mainViewModel);

        // 确保总耗时不少于 300ms
        await delayTask.ConfigureAwait(false);
        queueService.TryEnqueue(DispatcherQueuePriority.Low, () => mainViewModel.IsLoaded = true);

#if DEBUG
        Debug.WriteLine($"总加载耗时: {stopwatch.ElapsedMilliseconds} ms");
#endif
    }

    private void RegisterDisposableInstance(object instance)
    {
        if (instance is IDisposable disposable)
            _disposeAction += disposable.Dispose;
    }

    private void RegisterViewModel(Type viewType, object viewModel)
    {
        Debug.Assert(viewType is not null);
        Debug.Assert(viewModel is not null);
        Debug.Assert(_viewModelTable is not null);
        _viewModelTable[viewType] = viewModel;
    }

    private object GetViewModel(Type viewType)
    {
        Debug.Assert(_viewModelTable is not null);
        return _viewModelTable[viewType];
    }

    private void InitializeService()
    {
        Debug.Assert(_window is not null);
        Debug.Assert(Localizer.Instance is not null);

        _viewModelTable = [];

        IWindowService windowService = _window;
        IQueueService queueService = _window;
        ILocalizationService localizationService = Localizer.Instance;

        var deviceRegistry = new DeviceRegistry();
        var waveformRegistry = new WaveformRegistry();
        var waveformDecoderRegistryA = new WaveformDecoderRegistry();
        var waveformDecoderRegistryB = new WaveformDecoderRegistry();
        var batteryLevelUpdateScheduler = new BatteryLevelUpdateScheduler(deviceRegistry, TimeSpan.FromSeconds(60));
        var deviceIdPool = new DeviceIdPool([], new DeviceId());
        var waveformIdPool = new WaveformIdPool([], new WaveformId());
        var waveformDecoderIdPoolA = new WaveformDecoderIdPool([], new WaveformDecoderId());
        var waveformDecoderIdPoolB = new WaveformDecoderIdPool([], new WaveformDecoderId());
        var waveformDecoderIdMemoryOrderStoreA = new MemoryOrderStore<WaveformDecoderId>();
        var waveformDecoderIdMemoryOrderStoreB = new MemoryOrderStore<WaveformDecoderId>();
        var coyoteBleDetector = new CoyoteBleDetector();
        var coyoteFunctionFactory = new Factory(coyoteBleDetector);
        var deviceManager = new DeviceManager(deviceRegistry, coyoteFunctionFactory, deviceIdPool, deviceRegistry);
        var waveformPlayerA = new WaveformPlayer(waveformDecoderRegistryA, waveformDecoderRegistryA, waveformDecoderIdMemoryOrderStoreA, waveformRegistry, coyoteFunctionFactory, waveformDecoderIdPoolA);
        var waveformPlayerB = new WaveformPlayer(waveformDecoderRegistryB, waveformDecoderRegistryB, waveformDecoderIdMemoryOrderStoreB, waveformRegistry, coyoteFunctionFactory, waveformDecoderIdPoolB);
        var waveformPusher = new WaveformPusher(deviceRegistry, waveformPlayerA, waveformPlayerB);
        var memoryService = new MemoryService();
        var gtaVcMonitor = new GtaVcMonitor(memoryService);
        var gtaVcGameIntegration = new GtaVcGameIntegration(gtaVcMonitor);
        var gameIntegrationManager = new GameIntegrationManager([gtaVcGameIntegration], waveformPusher);
        var waveformNameRegistry = new NameRegistry<WaveformId>();
        var viewModelItemFactory = new ViewModelItemFactory(deviceRegistry, deviceRegistry, deviceRegistry, deviceManager, deviceManager, queueService, localizationService, waveformNameRegistry, waveformPlayerA, waveformPlayerB, waveformDecoderRegistryA, waveformDecoderRegistryB, waveformPlayerA, waveformPlayerB);
        var coyoteCollection = new CoyoteCollection();

        var homePageViewModel = new HomePageViewModel(deviceRegistry, deviceManager, waveformPlayerA, waveformPlayerB, waveformNameRegistry, waveformPusher, gameIntegrationManager, queueService, localizationService);
        var devicesPageViewModel = new DevicesPageViewModel(coyoteCollection, deviceManager, deviceRegistry, deviceManager, deviceManager, viewModelItemFactory, queueService, localizationService);
        var waveformsPageViewModel = new WaveformsPageViewModel(coyoteFunctionFactory, waveformIdPool, waveformRegistry, viewModelItemFactory, waveformRegistry, waveformNameRegistry, waveformPlayerA, waveformPlayerB, waveformDecoderRegistryA, waveformDecoderRegistryB, waveformDecoderIdMemoryOrderStoreA, waveformDecoderIdMemoryOrderStoreB, windowService, queueService, localizationService);
        var gamesPageViewModel = new GamesPageViewModel(gameIntegrationManager, waveformPusher, waveformPusher, queueService, localizationService);
        var safetyPageViewModel = new SafetyPageViewModel(coyoteCollection, deviceRegistry, deviceRegistry);
        var debugPageViewModel = new DebugPageViewModel(queueService);
        var settingsPageViewModel = new SettingsPageViewModel(queueService, windowService, windowService, localizationService, VersionHelper.GetVersion());

        var gtaVcGameIntegrationViewModel = new GtaVcGameIntegrationViewModel(gtaVcGameIntegration, gtaVcMonitor, queueService, localizationService);
        RegisterViewModel(typeof(GtaVcRuleCards), gtaVcGameIntegrationViewModel);

        var devicesPageNavigationParameter = new DevicesPage.RequiredParameter(devicesPageViewModel, queueService, localizationService);
        var waveformsPageNavigationParameter = new WaveformsPage.RequiredParameter(waveformsPageViewModel, queueService, localizationService);
        var gamesPageNavigationParameter = new GamesPage.RequiredParameter(gamesPageViewModel, gtaVcGameIntegrationViewModel, GetPageHeaderSelector, GetPageContent, queueService, localizationService);

        RegisterViewModel(typeof(HomePage), homePageViewModel);
        RegisterViewModel(typeof(DevicesPage), devicesPageNavigationParameter);
        RegisterViewModel(typeof(WaveformsPage), waveformsPageNavigationParameter);
        RegisterViewModel(typeof(GamesPage), gamesPageNavigationParameter);
        RegisterViewModel(typeof(SafetyPage), safetyPageViewModel);
        RegisterViewModel(typeof(DebugPage), debugPageViewModel);
        RegisterViewModel(typeof(SettingsPage), settingsPageViewModel);

        RegisterDisposableInstance(deviceRegistry);
        RegisterDisposableInstance(waveformRegistry);
        RegisterDisposableInstance(waveformDecoderRegistryA);
        RegisterDisposableInstance(waveformDecoderRegistryB);
        RegisterDisposableInstance(batteryLevelUpdateScheduler);
        RegisterDisposableInstance(deviceIdPool);
        RegisterDisposableInstance(waveformIdPool);
        RegisterDisposableInstance(waveformDecoderIdPoolA);
        RegisterDisposableInstance(waveformDecoderIdPoolB);
        RegisterDisposableInstance(waveformDecoderIdMemoryOrderStoreA);
        RegisterDisposableInstance(waveformDecoderIdMemoryOrderStoreB);
        RegisterDisposableInstance(coyoteBleDetector);
        RegisterDisposableInstance(coyoteFunctionFactory);
        RegisterDisposableInstance(deviceManager);
        RegisterDisposableInstance(waveformPlayerA);
        RegisterDisposableInstance(waveformPlayerB);
        RegisterDisposableInstance(waveformPusher);
        RegisterDisposableInstance(memoryService);
        RegisterDisposableInstance(gtaVcMonitor);
        RegisterDisposableInstance(gtaVcGameIntegration);
        RegisterDisposableInstance(gameIntegrationManager);
        RegisterDisposableInstance(waveformNameRegistry);
        RegisterDisposableInstance(viewModelItemFactory);
        RegisterDisposableInstance(coyoteCollection);
        RegisterDisposableInstance(homePageViewModel);
        RegisterDisposableInstance(devicesPageViewModel);
        RegisterDisposableInstance(waveformsPageViewModel);
        RegisterDisposableInstance(gamesPageViewModel);
        RegisterDisposableInstance(safetyPageViewModel);
        RegisterDisposableInstance(debugPageViewModel);
        RegisterDisposableInstance(settingsPageViewModel);
        RegisterDisposableInstance(gtaVcGameIntegrationViewModel);
        RegisterDisposableInstance(devicesPageNavigationParameter);
        RegisterDisposableInstance(waveformsPageNavigationParameter);
        RegisterDisposableInstance(gamesPageNavigationParameter);
    }

    private void MergeDictionaries()
    {
        var newDictionary = new ResourceDictionary();
        LoadComponent(newDictionary, new Uri("ms-appx:///Resources/AppResourceDictionary.xaml", UriKind.Absolute));
        Resources.MergedDictionaries.Add(newDictionary);
    }

    private void SetWindowIcon()
    {
        if (_window == null)
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

    private static Type? GetTopPageType(string pageName)
    {
        return pageName switch
        {
            nameof(HomePage) => typeof(HomePage),
            nameof(DevicesPage) or nameof(CoyoteDetailPage) => typeof(DevicesPage),
            nameof(WaveformsPage) or nameof(WaveformPlaylistPage) or nameof(WaveformSettingsPage) => typeof(WaveformsPage),
            nameof(GamesPage) or nameof(GameIntegrationDetailPage) => typeof(GamesPage),
            nameof(SafetyPage) => typeof(SafetyPage),
            nameof(DebugPage) => typeof(DebugPage),
            nameof(SettingsPage) => typeof(SettingsPage),
            _ => null
        };
    }

    private static Func<ILocalizationService, string> GetPageHeaderSelector(string contentName)
    {
        return contentName switch
        {
            nameof(GtaVcRuleCards) => s => s.AppText.MainWindowText.MainViewText.GamesPageText.GtaVcGameIntegrationHeader,
            _ => _ => "null"
        };
    }

    private FrameworkElement? GetPageContent(string contentName)
    {
        return contentName switch
        {
            nameof(GtaVcRuleCards) => new GtaVcRuleCards((GtaVcGameIntegrationViewModel)GetViewModel(typeof(GtaVcRuleCards))),
            _ => null
        };
    }
}
