using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.Services;
using DungeonToolkit.Coyote;
using System.Collections.ObjectModel;

namespace Dianty.ViewModels;

public partial class DevicesPageViewModel : ObservableObject
{
    public DevicesPageViewModel(IQueueService queueService, ICoyoteBleDetector coyoteBleDetector)
    {
        _queueService = queueService;
        _coyoteBleDetector = coyoteBleDetector;

#if DEBUG
        CoyoteItems.Add(new CoyoteBleItem(new CoyoteBLE { DeviceName = "调试" }, _queueService, _coyoteBleDetector));
        CoyoteItems.Add(new CoyoteWsItem(new CoyoteWS { DeviceName = "调试0" }, _queueService));
        CoyoteItems.Add(new CoyoteWsItem(new CoyoteWS { DeviceName = "调试1" }, _queueService));
#endif
    }

    private readonly IQueueService _queueService;
    private readonly ICoyoteBleDetector _coyoteBleDetector;

    public ObservableCollection<CoyoteItem> CoyoteItems { get; } = [];
}
