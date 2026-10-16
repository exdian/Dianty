using Dianty.EventArguments;
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

    event EventHandler<CurrentLanguageChangedEventArgs>? CurrentLanguageChanged;

    Task<IEnumerable<KeyValuePair<string, string>>> GetAvailableLanguagesAsync();
    Task SetLanguageAsync(string fileName);
}
