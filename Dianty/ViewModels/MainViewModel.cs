using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.Services;
using System;
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
        var task = Task.Delay(300);

        // 模拟后台耗时操作
        await Task.Delay(200);
        ServiceLocator.RegisterViewModels();

        // 至少加载 300 毫秒
        await task;
        _queueService.TryEnqueue(() =>
        {
            IsLoaded = true;
        });
    }
}
