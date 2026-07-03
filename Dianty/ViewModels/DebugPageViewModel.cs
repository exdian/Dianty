using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dianty.Services;
using Dianty.Utils.Messages;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;

namespace Dianty.ViewModels;

public partial class DebugPageViewModel : ObservableObject, IDisposable
{
    public DebugPageViewModel(IQueueService queueService)
    {
        _queueService = queueService;

        WeakReferenceMessenger.Default.Register<Log>(this, (r, l) =>
        {
            AppendLog(l);
        });
    }

    ~DebugPageViewModel()
    {
        Dispose();
    }

    private readonly IQueueService _queueService;

    public ObservableCollection<Log> Logs { get; } = [];

    public void AppendLog(string message)
    {
        AppendLog(new Log(message));
    }

    public void AppendLog(Log log)
    {
        _queueService.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            Logs.Add(log);
            if (Logs.Count > 5000)
            {
                for (int i = 0; i < 2000 && Logs.Count > 0; i++)
                {
                    Logs.RemoveAt(0);
                }
            }

            // 发送通知，便于视图滚动到最新项
            WeakReferenceMessenger.Default.Send(new LogAppendedMessage());
        });
    }

    [RelayCommand]
    private void ClearLog()
    {
        Logs.Clear();
    }

    public void Dispose()
    {
        WeakReferenceMessenger.Default.Unregister<Log>(this);
        GC.SuppressFinalize(this);
    }
}
