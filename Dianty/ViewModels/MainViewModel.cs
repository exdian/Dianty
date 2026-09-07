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
        InitializeData();
    }

    private readonly IQueueService _queueService;

    [ObservableProperty]
    public partial bool IsLoaded { get; private set; }

    private void InitializeData()
    {
        _queueService.TryEnqueue(DispatcherQueuePriority.Low, async () =>
        {
            var stopwatch = Stopwatch.StartNew();
            var minLoadTask = Task.Delay(300); // 最短加载时间
            var planTask = Task.CompletedTask;

            var plan = ToDoList.Plan;
            ToDoList.Plan = null;
            if (plan is not null)
                planTask = Task.Run(plan);

            var uiPlan = ToDoList.UiPlan;
            ToDoList.UiPlan = null;
            uiPlan?.Invoke();

            await planTask;
            Debug.WriteLine($"后台加载耗时: {stopwatch.ElapsedMilliseconds} ms");

            // 确保总耗时不少于 300ms
            await minLoadTask;

            IsLoaded = true;
            Debug.WriteLine($"总加载耗时: {stopwatch.ElapsedMilliseconds} ms");
        });
    }
}
