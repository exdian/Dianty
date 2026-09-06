using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.System.UserProfile;
using static Dianty.Services.ILocalizationService;

namespace Dianty.Localization;

public partial class Localizer : ObservableObject, ILocalizationService
{
    public static Localizer Instance { get; set; } = null!;

    [ObservableProperty]
    public partial AppText AppText { get; private set; }

    public string TextDirectory { get; set; } = Path.Combine(AppContext.BaseDirectory, "Localization/Text");

    public string CurrentLanguageFileName
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                OnCurrentLanguageFileNameChanged(value);
            }
        }
    } = string.Empty;

    public event EventHandler<CurrentLanguageFileNameChangedEventArgs>? CurrentLanguageFileNameChanged;

    public static void Init()
    {
        const string simplifiedChinese = "zh-Hans";
        const string english = "en-US";

        var systemLang = GlobalizationPreferences.Languages.FirstOrDefault(english);
        if (systemLang.Contains(simplifiedChinese))
            systemLang = simplifiedChinese;

        var localizer = new Localizer();
        var fileName = $"{systemLang}.json";
        var filePath = Path.Combine(localizer.TextDirectory, fileName);

        AppText? appText = null;

        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            appText = JsonSerializer.Deserialize(json, AppTextJsonContext.Unsafe.AppText);
        }

        if (appText is null)
        {
            // 无匹配系统的语言则尝试退回英文
            filePath = Path.Combine(localizer.TextDirectory, english + ".json");
            if (systemLang != simplifiedChinese && File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                appText = JsonSerializer.Deserialize(json, AppTextJsonContext.Unsafe.AppText);
            }
            else
            {
                // 系统语言是中文或者英文文件不存在则生成中文
                fileName = simplifiedChinese + ".json";
                appText = new AppText { Language = "简体中文" };
                Directory.CreateDirectory(localizer.TextDirectory);
                var json = JsonSerializer.Serialize(appText, AppTextJsonContext.Unsafe.AppText);
                File.WriteAllText(Path.Combine(localizer.TextDirectory, fileName), json);
            }
        }

        Debug.Assert(appText is not null);
        localizer.AppText = appText;
        localizer.CurrentLanguageFileName = fileName;
        Instance = localizer;
    }

    public async Task<IEnumerable<KeyValuePair<string, string>>> GetAvailableLanguagesAsync()
    {
        var result = new ConcurrentDictionary<string, string>();

        if (!Directory.Exists(TextDirectory))
            return result;

        var files = Directory.GetFiles(TextDirectory);
        var tasks = files.Select(async filePath =>
        {
            try
            {
                const long maxFileSize = 8L * 1024 * 1024; // 8MB
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > maxFileSize)
                    return;

                var json = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
                var appText = JsonSerializer.Deserialize(json, AppTextJsonContext.Unsafe.AppText);

                if (appText is not null)
                {
                    var fileName = Path.GetFileName(filePath);
                    var language = string.IsNullOrEmpty(appText.Language) ? fileName : appText.Language;
                    result.TryAdd(fileName, language);
                }
            }
            catch { }
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);
        return result;
    }

    public async Task SetLanguageAsync(string fileName)
    {
        var filePath = Path.Combine(TextDirectory, fileName);
        var json = await File.ReadAllTextAsync(filePath);
        var appText = JsonSerializer.Deserialize(json, AppTextJsonContext.Unsafe.AppText);
        if (appText is not null)
        {
            AppText = appText;
            CurrentLanguageFileName = fileName;
        }
        else
        {
            throw new Exception("Json Exception");
        }
    }

    private void OnCurrentLanguageFileNameChanged(string currentLanguageFileName)
    {
        var args = new CurrentLanguageFileNameChangedEventArgs(currentLanguageFileName);
        CurrentLanguageFileNameChanged?.Invoke(this, args);
    }
}
