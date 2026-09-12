using Dianty.Localization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dianty.Services;

public interface ILocalizationService
{
    AppText AppText { get; }
    string TextDirectory { get; set; }
    string CurrentLanguageFileName { get; }

    event EventHandler<CurrentLanguageFileNameChangedEventArgs>? CurrentLanguageFileNameChanged;

    Task<IEnumerable<KeyValuePair<string, string>>> GetAvailableLanguagesAsync();
    Task SetLanguageAsync(string fileName);

    public class CurrentLanguageFileNameChangedEventArgs(string currentLanguageFileName) : EventArgs
    {
        public string CurrentLanguageFileName { get; } = currentLanguageFileName;
    }
}
