using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.Services;
using Microsoft.UI.Xaml;
using System.Linq;

namespace Dianty.ViewModels;

public partial class SettingsPageViewModel : ObservableObject
{
    public SettingsPageViewModel(IQueueService queueService, IWindowService windowService)
    {
        _queueService = queueService;
        _windowService = windowService;

        AppThemeSelectionItems =
            [new AppThemeSelectionItem(ElementTheme.Light, "浅色"),
            new AppThemeSelectionItem(ElementTheme.Dark, "深色"),
            new AppThemeSelectionItem(ElementTheme.Default, "跟随系统"),];
        _queueService.TryEnqueue(() =>
            SelectedAppTheme = AppThemeSelectionItems.FirstOrDefault(t => t.Theme == _windowService.ColorTheme));
    }

    private readonly IQueueService _queueService;
    private readonly IWindowService _windowService;

    public AppThemeSelectionItem[] AppThemeSelectionItems { get; }

    [ObservableProperty]
    public partial AppThemeSelectionItem SelectedAppTheme { get; set; }

    partial void OnSelectedAppThemeChanged(AppThemeSelectionItem value)
    {
        _queueService.TryEnqueue(() => _windowService.ColorTheme = value.Theme);
    }
}

public readonly record struct AppThemeSelectionItem(ElementTheme Theme, string Description);
