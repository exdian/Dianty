using Dianty.Services;
using Dianty.ViewModels;
using Dianty.Views.Pages;
using Microsoft.UI.Xaml;

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
        window.ViewModel = new MainViewModel(window);
        _mainWindow = window;
        _window = window;
        _window.Activate();
    }

    private void RegisterService()
    {
        if (_mainWindow is not null)
        {
            ServiceLocator.Register<ITitleBarService>(_mainWindow);
            ServiceLocator.Register<IWindowService>(_mainWindow);
            ServiceLocator.Register<IQueueService>(_mainWindow);
            ServiceLocator.Register<ITemplateContent>(_mainWindow);
        }

        ServiceLocator.RegisterViewModel(typeof(GamesPage), new GamesPage.RequiredParameter(
            new GamesViewModel(),
            ServiceLocator.GetService<IQueueService>()));
    }
}
