using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.Services;
using DungeonToolkit.Coyote;
using System.Collections.ObjectModel;

namespace Dianty.ViewModels;

public partial class DevicesPageViewModel : ObservableObject
{
    public DevicesPageViewModel(IQueueService queueService, ICoyoteBLEDetector coyoteBleDetector)
    {
        _queueService = queueService;
        _coyoteBleDetector = coyoteBleDetector;

#if DEBUG
        CoyoteItems.Add(new CoyoteBleItem(new CoyoteBLE(), _queueService, _coyoteBleDetector));
        CoyoteItems.Add(new CoyoteWsItem(new CoyoteWS(), _queueService));
#endif
    }

    private readonly IQueueService _queueService;
    private readonly ICoyoteBLEDetector _coyoteBleDetector;

    public ObservableCollection<CoyoteItem> CoyoteItems { get; } = [];
}
