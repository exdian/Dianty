using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.Services;
using Dianty.Utils;
using Microsoft.UI.Dispatching;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Dianty.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public MainViewModel(IQueueService queueService)
    {
        _queueService = queueService;
        _queueService.TryEnqueue(DispatcherQueuePriority.Low, LoadData);
    }

    private readonly IQueueService _queueService;

    [ObservableProperty]
    public partial bool IsLoaded { get; private set; }

    private async void LoadData()
    {
        var stopwatch = Stopwatch.StartNew();
        var task = Task.Delay(300);

        if (ToDoList.Plan is not null)
            await Task.Run(ToDoList.Plan);

        if (ToDoList.UiPlan is not null)
            _queueService.TryEnqueue(ToDoList.UiPlan.Invoke);

        Debug.WriteLine($"后台加载耗时: {stopwatch.ElapsedMilliseconds} ms");

        // 至少加载 300 毫秒
        await task;
        _queueService.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            IsLoaded = true;
            Debug.WriteLine($"总加载耗时: {stopwatch.ElapsedMilliseconds} ms");
        });
    }
}
