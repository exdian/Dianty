using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.EventArguments;
using Dianty.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dianty.ViewModels;

public sealed partial class SettingsPageViewModel : ObservableObject, IDisposable
{
    private readonly IQueueService _queueService;
    private readonly IWindowThemeService _windowThemeService;
    private readonly IWindowContentService _windowContentService;
    private readonly ILocalizationService _localizationService;
    private bool _isDisposed;

    public SettingsPageViewModel(
        IQueueService queueService,
        IWindowThemeService windowThemeService,
        IWindowContentService windowContentService,
        ILocalizationService localizationService,
        string appVersion)
    {
        _queueService = queueService;
        _windowThemeService = windowThemeService;
        _windowContentService = windowContentService;
        _localizationService = localizationService;
        Version = appVersion;

        AppThemeSelectionItems = AppThemeSelectionItem.GetSelectionItems(_localizationService);
        _queueService.TryEnqueue(() =>
            SelectedAppTheme = AppThemeSelectionItems.FirstOrDefault(t => t.Theme == _windowThemeService.ColorTheme));
        _ = InitializeLanguageSelectionItems();

        _localizationService.CurrentLanguageChanged += OnCurrentLanguageChanged;
    }

    public string Version { get; }

    [ObservableProperty]
    public partial AppThemeSelectionItem[] AppThemeSelectionItems { get; private set; }

    [ObservableProperty]
    public partial AppThemeSelectionItem? SelectedAppTheme { get; set; }

    [ObservableProperty]
    public partial LanguageSelectionItem[] LanguageSelectionItems { get; private set; } = [];

    [ObservableProperty]
    public partial LanguageSelectionItem? SelectedLanguage { get; set; }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            _localizationService.CurrentLanguageChanged -= OnCurrentLanguageChanged;
        }

        _isDisposed = true;
    }

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

    private void OnCurrentLanguageChanged(object? sender, CurrentLanguageChangedEventArgs e)
    {
        var appThemeSelectionItems = AppThemeSelectionItem.GetSelectionItems(_localizationService);
        var selectedAppTheme = SelectedAppTheme;
        if (selectedAppTheme is not null)
        {
            var theme = selectedAppTheme.Theme;
            selectedAppTheme = appThemeSelectionItems.FirstOrDefault(i => i.Theme == theme);
        }
        _queueService.TryEnqueue(() =>
        {
            AppThemeSelectionItems = appThemeSelectionItems;
            SelectedAppTheme = selectedAppTheme;
        });
    }

    partial void OnSelectedAppThemeChanged(AppThemeSelectionItem? value)
    {
        if (value is null)
            return;
        _queueService.TryEnqueue(() => _windowThemeService.ColorTheme = value.Theme);
    }

    partial void OnSelectedLanguageChanged(LanguageSelectionItem? value)
    {
        if (value is null)
            return;

        _queueService.TryEnqueue(async () =>
        {
            bool isExceptionOccurred = false;
            string message = string.Empty;
            try
            {
                await _localizationService.SetLanguageAsync(value.FileName);
            }
            catch (Exception ex)
            {
                isExceptionOccurred = true;
                message = ex.Message;
            }
            if (isExceptionOccurred)
            {
                var dialog = _windowContentService.CreateContentDialog();
                if (dialog == null)
                    return;
                var dialogText = _localizationService.AppText.MainWindowText.MainViewText.SettingsPageText.LanguageSwitchFailedDialogText;
                dialog.Title = dialogText.Title;
                dialog.CloseButtonText = dialogText.CloseButtonText;
                dialog.IsPrimaryButtonEnabled = false;
                dialog.IsSecondaryButtonEnabled = false;
                dialog.DefaultButton = ContentDialogButton.Close;
                dialog.Content = message;
                await dialog.ShowAsync();
            }
        });
    }
}

public sealed record class AppThemeSelectionItem(ElementTheme Theme, string Description)
{
    private static AppThemeSelectionItem[]? s_SelectionItems;

    public static AppThemeSelectionItem[] GetSelectionItems(ILocalizationService service)
    {
        var text = service.AppText.MainWindowText.MainViewText.SettingsPageText;
        if (s_SelectionItems is null
            || s_SelectionItems.Length != 3
            || !ReferenceEquals(s_SelectionItems[0].Description, text.LightThemeItemText)
            || !ReferenceEquals(s_SelectionItems[1].Description, text.DarkThemeItemText)
            || !ReferenceEquals(s_SelectionItems[2].Description, text.DefaultThemeItemText))
        {
            s_SelectionItems =
                [new AppThemeSelectionItem(ElementTheme.Light, text.LightThemeItemText),
                new AppThemeSelectionItem(ElementTheme.Dark, text.DarkThemeItemText),
                new AppThemeSelectionItem(ElementTheme.Default, text.DefaultThemeItemText)];
        }
        return s_SelectionItems;
    }
}

public sealed record class LanguageSelectionItem(string FileName, string Description);
