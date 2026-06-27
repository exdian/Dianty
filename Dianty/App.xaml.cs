using Dianty.Services;
using Dianty.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

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
        var serviceLocator = ServiceLocator.Instance;
        if (serviceLocator is null)
            return;

        if (_mainWindow is not null)
        {
            serviceLocator.Register<ITitleBarService>(_mainWindow);
            serviceLocator.Register<IWindowService>(_mainWindow);
            serviceLocator.Register<IQueueService>(_mainWindow);
            serviceLocator.Register<ITemplateContent>(_mainWindow);
        }
    }
}
