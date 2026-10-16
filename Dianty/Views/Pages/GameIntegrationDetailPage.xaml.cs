using Dianty.Services;
using Dianty.Views.Controls;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace Dianty.Views.Pages;

public sealed partial class GameIntegrationDetailPage : Page
{
    public GameIntegrationDetailPage()
    {
        InitializeComponent();
    }

    private string? _contentName;
    private Func<string, FrameworkElement?>? _pageContentFactory;
    private IQueueService? _queueService;

    private PathBar.RequiredParameter? PathBarParameter { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is RequiredParameter parameter)
        {
            PathBarParameter = parameter.PathBarParameter;
            _contentName = parameter.ContentName;
            _pageContentFactory = parameter.PageContentFactory;
            _queueService = parameter.QueueService;
        }
    }

    private void OnRootGridLoaded(object sender, RoutedEventArgs e)
    {
        if (_queueService is null || _contentName is null || _pageContentFactory is null)
            return;

        _queueService.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            var content = _pageContentFactory.Invoke(_contentName);
            if (content != null)
            {
                Grid.SetRow(content, 1);
                _rootGrid.Children.Add(content);
            }
        });
    }

    public sealed record class RequiredParameter(PathBar.RequiredParameter PathBarParameter,
        string ContentName, Func<string, FrameworkElement?> PageContentFactory, IQueueService QueueService);
}
