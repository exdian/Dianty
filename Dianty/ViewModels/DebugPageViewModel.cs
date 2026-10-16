using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dianty.Services;
using Dianty.Services.Messages;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dianty.ViewModels;

public sealed partial class DebugPageViewModel : ObservableObject, IDisposable
{
    public DebugPageViewModel(IQueueService queueService)
    {
        _queueService = queueService;

        WeakReferenceMessenger.Default.Register<Log>(this, (r, l) =>
        {
            AppendLog(l);
        });
    }

    private readonly IQueueService _queueService;
    private bool _isDisposed;

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
                List<Log> reserve = new(Logs.Count - 2000);
                for (int i = 2000; i < Logs.Count; i++)
                {
                    reserve.Add(Logs[i]);
                }

                Logs.Clear();

                for (int i = 0; i < reserve.Count; i++)
                {
                    Logs.Insert(i, reserve[i]);
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
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            WeakReferenceMessenger.Default.Unregister<Log>(this);
        }

        _isDisposed = true;
    }
}
