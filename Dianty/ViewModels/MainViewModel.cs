using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.Resources;
using Dianty.Services;
using Microsoft.UI.Dispatching;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Dianty.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public MainViewModel(IQueueService queueService)
    {
        _queueService = queueService;
        _queueService.TryEnqueue(DispatcherQueuePriority.Low, LoadDataAsync);
    }

    private readonly IQueueService _queueService;

    [ObservableProperty]
    public partial bool IsLoaded { get; private set; }

    private async void LoadDataAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        var task = Task.Delay(300);

        // 后台耗时操作
        await Task.Run(ServiceLocator.RegisterDefault);
        Debug.WriteLine($"后台加载耗时: {stopwatch.ElapsedMilliseconds} ms");

        // 必须在 UI 线程的操作
        _queueService.TryEnqueue(ResourceLoader.Load);

        // 至少加载 300 毫秒
        await task;
        _queueService.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            IsLoaded = true;
            Debug.WriteLine($"总加载耗时: {stopwatch.ElapsedMilliseconds} ms");
        });
    }
}
