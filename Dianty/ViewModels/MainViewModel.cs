using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.Models;
using GameMonitor;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;

namespace Dianty.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public MainViewModel(DispatcherQueue dispatcher)
    {
        _dispatcher = dispatcher;
        _monitor = new GtaVcMonitor(new MemoryService());
        _monitor.Start();
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();
        LoadDataAsync();
    }

    private void Timer_Tick(object? sender, object e)
    {
        var pid = _monitor.CurrentPid;
        if (pid == null)
        {
            CurrentPid = "PID：未知";
            CurrentHealth = "未知";
            CurrentArmor = "未知";
        }
        else
        {
            CurrentPid = $"PID：{pid}";
            CurrentHealth = _monitor.CurrentHealth.ToString("F2");
            CurrentArmor = _monitor.CurrentArmor.ToString("F2");
        }
    }

    private readonly DispatcherQueue _dispatcher;
    private readonly GtaVcMonitor _monitor;
    private readonly DispatcherTimer _timer;

    [ObservableProperty]
    public partial object SubViewModel { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string CurrentPid { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string CurrentHealth { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string CurrentArmor { get; set; } = string.Empty;

    private async void LoadDataAsync()
    {
        // 模拟后台耗时操作
        await Task.Delay(2000);

        _dispatcher.TryEnqueue(() =>
        {
            SubViewModel = this;
        });
#if DEBUG
        for (int i = 0; i < 5; i++)
        {
            await Task.Delay(2000);
            _dispatcher.TryEnqueue(() =>
            {
                SubViewModel = string.Empty;
            });
            await Task.Delay(2000);
            _dispatcher.TryEnqueue(() =>
            {
                SubViewModel = this;
            });
        }
#endif
    }
}
