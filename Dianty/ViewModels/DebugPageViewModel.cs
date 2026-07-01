using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dianty.Services;
using Dianty.Utils;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;

namespace Dianty.ViewModels;

public partial class DebugPageViewModel : ObservableObject
{
    public DebugPageViewModel(IQueueService queueService)
    {
        _queueService = queueService;

        WeakReferenceMessenger.Default.Register<LogMessage>(this, (r, m) =>
        {
            AppendLog(m.Content);
        });
    }

    private readonly IQueueService _queueService;

    public ObservableCollection<LogEntry> Logs { get; } = [];

    public void AppendLog(string message)
    {
        _queueService.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            Logs.Add(new LogEntry(message));
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
}
