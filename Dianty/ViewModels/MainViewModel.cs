using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.Services;
using Dianty.Utils;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Dianty.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public MainViewModel(IQueueService queueService, IWindowService windowService)
    {
        _queueService = queueService;
        _windowService = windowService;

        InitializeData();
    }

    private readonly IQueueService _queueService;
    private readonly IWindowService _windowService;

    [ObservableProperty]
    public partial bool IsLoaded { get; private set; }

    private void InitializeData()
    {
        _queueService.TryEnqueue(DispatcherQueuePriority.Low, async () =>
        {
            bool isExceptionOccurred = false;
            string message = string.Empty;
            try
            {
                await InitializeDataAsync();
            }
            catch (Exception ex)
            {
                isExceptionOccurred = true;
                message = ex.Message;
            }
            if (isExceptionOccurred)
            {
                var dialog = _windowService.CreateContentDialog();
                if (dialog is null)
                    return;
                dialog.Title = "Error";
                dialog.CloseButtonText = "Close";
                dialog.IsPrimaryButtonEnabled = false;
                dialog.IsSecondaryButtonEnabled = false;
                dialog.DefaultButton = ContentDialogButton.Close;
                dialog.Content = message;
                await dialog.ShowAsync();
                _queueService.Crash(message);
            }
        });
    }

    private async Task InitializeDataAsync()
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
        if (uiPlan is not null)
            _queueService.TryEnqueue(uiPlan.Invoke);

        await planTask.ConfigureAwait(false);
        Debug.WriteLine($"后台加载耗时: {stopwatch.ElapsedMilliseconds} ms");

        // 确保总耗时不少于 300ms
        await minLoadTask.ConfigureAwait(false);

        _queueService.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            IsLoaded = true;
            Debug.WriteLine($"总加载耗时: {stopwatch.ElapsedMilliseconds} ms");
        });
    }
}
