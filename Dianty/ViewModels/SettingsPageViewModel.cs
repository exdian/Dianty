using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.Services;
using Dianty.Utils;
using Microsoft.UI.Xaml;
using System.Linq;
using System.Threading.Tasks;

namespace Dianty.ViewModels;

public partial class SettingsPageViewModel : ObservableObject
{
    public SettingsPageViewModel(IQueueService queueService, IWindowService windowService, ILocalizationService localizationService)
    {
        _queueService = queueService;
        _windowService = windowService;
        _localizationService = localizationService;

        Version = VersionHelper.Version ?? string.Empty;
        AppThemeSelectionItems =
            [new AppThemeSelectionItem(ElementTheme.Light, "浅色"),
            new AppThemeSelectionItem(ElementTheme.Dark, "深色"),
            new AppThemeSelectionItem(ElementTheme.Default, "跟随系统"),];
        _queueService.TryEnqueue(() =>
            SelectedAppTheme = AppThemeSelectionItems.FirstOrDefault(t => t.Theme == _windowService.ColorTheme));
        _ = InitializeLanguageSelectionItems();
    }

    private readonly IQueueService _queueService;
    private readonly IWindowService _windowService;
    private readonly ILocalizationService _localizationService;

    public string Version { get; }

    public AppThemeSelectionItem[] AppThemeSelectionItems { get; }

    [ObservableProperty]
    public partial AppThemeSelectionItem SelectedAppTheme { get; set; }

    [ObservableProperty]
    public partial LanguageSelectionItem[] LanguageSelectionItems { get; private set; } = [];

    [ObservableProperty]
    public partial LanguageSelectionItem SelectedLanguage { get; set; }

    private async Task InitializeLanguageSelectionItems()
    {
        var languages = await _localizationService.GetAvailableLanguagesAsync().ConfigureAwait(false);
        var languageSelectionItems = languages.Select(l => new LanguageSelectionItem(l.Key, l.Value)).ToArray();
        var selectedLanguage = languageSelectionItems.FirstOrDefault(l => l.FileName == _localizationService.CurrentLanguageFileName);
        _queueService.TryEnqueue(() =>
        {
            LanguageSelectionItems = languageSelectionItems;
            SelectedLanguage = selectedLanguage;
        });
    }

    partial void OnSelectedAppThemeChanged(AppThemeSelectionItem value)
    {
        _queueService.TryEnqueue(() => _windowService.ColorTheme = value.Theme);
    }

    partial void OnSelectedLanguageChanged(LanguageSelectionItem value)
    {
        if (LanguageSelectionItems.Length > 0)
            _queueService.TryEnqueue(() => _localizationService.SetLanguageAsync(value.FileName));
    }
}

public readonly record struct AppThemeSelectionItem(ElementTheme Theme, string Description);

public readonly record struct LanguageSelectionItem(string FileName, string Description);
