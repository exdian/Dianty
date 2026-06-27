using Microsoft.UI.Dispatching;

namespace Dianty.Services;

public interface IQueueService
{
    bool TryEnqueue(DispatcherQueueHandler callback);
}
