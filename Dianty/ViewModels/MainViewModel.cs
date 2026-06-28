using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Dianty.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public MainViewModel(IQueueService queueService)
    {
        _queueService = queueService;
        LoadDataAsync();
    }

    private readonly IQueueService _queueService;

    [ObservableProperty]
    public partial bool IsLoaded { get; private set; }

    private async void LoadDataAsync()
    {
        Task task = Task.CompletedTask;
        var stopwatch = Stopwatch.StartNew();

        // 后台耗时操作
        await Task.Run(() =>
        {
            task = Task.Delay(300);
            ServiceLocator.RegisterDefault();
        });
        Debug.WriteLine($"后台加载耗时: {stopwatch.ElapsedMilliseconds} ms");

        // 至少加载 300 毫秒
        await task;
        _queueService.TryEnqueue(() =>
        {
            IsLoaded = true;
            Debug.WriteLine($"实际加载完成耗时: {stopwatch.ElapsedMilliseconds} ms");
        });
    }
}
