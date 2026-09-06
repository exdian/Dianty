using Microsoft.UI.Dispatching;

namespace Dianty.Services;

public interface IQueueService
{
    void TryEnqueue(DispatcherQueueHandler callback);
    void TryEnqueue(DispatcherQueuePriority priority, DispatcherQueueHandler callback);
    void Crash(string message);
}
