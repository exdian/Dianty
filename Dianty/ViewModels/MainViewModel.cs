using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;

namespace Dianty.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public MainViewModel(DispatcherQueue dispatcher)
    {
        _dispatcher = dispatcher;
        LoadDataAsync();
    }

    private readonly DispatcherQueue _dispatcher;

    [ObservableProperty]
    public partial bool IsLoaded { get; private set; }

    private async void LoadDataAsync()
    {
        var task = Task.Delay(300);

        // 模拟后台耗时操作
        await Task.Delay(200);

        // 至少加载 300 毫秒
        await task;
        _dispatcher.TryEnqueue(() =>
        {
            IsLoaded = true;
        });
    }
}
