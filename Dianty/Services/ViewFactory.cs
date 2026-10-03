using Dianty.Views;
using System;

namespace Dianty.Services;

public sealed class ViewFactory : ITemplateContent
{
    private readonly ITitleBarService _titleBarService;
    private readonly IWindowInputService _windowInputService;
    private readonly IQueueService _queueService;
    private readonly ILocalizationService _localizationService;
    private readonly Func<Type, object> _viewModelLocator;
    private readonly Func<string, Type?> _topPageTypeMatcher;

    public ViewFactory(
        ITitleBarService titleBarService,
        IWindowInputService windowInputService,
        IQueueService queueService,
        ILocalizationService localizationService,
        Func<Type, object> viewModelLocator,
        Func<string, Type?> topPageTypeMatcher)
    {
        _titleBarService = titleBarService;
        _windowInputService = windowInputService;
        _queueService = queueService;
        _localizationService = localizationService;
        _viewModelLocator = viewModelLocator;
        _topPageTypeMatcher = topPageTypeMatcher;
    }

    object? ITemplateContent.CreateContent(object? item)
    {
        if (item is bool value && value)
        {
            return new MainView(_titleBarService, _windowInputService, _queueService, _localizationService, _viewModelLocator, _topPageTypeMatcher);
        }
        else
        {
            return new LoadView();
        }
    }
}
