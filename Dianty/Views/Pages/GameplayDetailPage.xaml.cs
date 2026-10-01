using Dianty.Services;
using Dianty.Utils;
using Dianty.Views.Controls;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Dianty.Views.Pages;

public sealed partial class GameplayDetailPage : Page
{
    public GameplayDetailPage()
    {
        InitializeComponent();
    }

    private string? _contentName;
    private IQueueService? _queueService;

    private PathBar.RequiredParameter? PathBarParameter { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is RequiredParameter parameter)
        {
            PathBarParameter = parameter.PathBarParameter;
            _contentName = parameter.ContentName;
            _queueService = parameter.QueueService;
        }
    }

    private void OnRootGridLoaded(object sender, RoutedEventArgs e)
    {
        if (_queueService is null || _contentName is null)
            return;

        _queueService.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            var content = PageHelper.GetPageContent(_contentName);
            if (content != null)
            {
                Grid.SetRow(content, 1);
                _rootGrid.Children.Add(content);
            }
        });
    }

    public record class RequiredParameter(PathBar.RequiredParameter PathBarParameter,
        string ContentName, IQueueService QueueService);
}
