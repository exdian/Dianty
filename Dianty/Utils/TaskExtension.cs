using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dianty.Utils;

public static class TaskExtension
{
    extension<T>(Task<T> task)
    {
        public async Task<T> WithCancellation(CancellationToken token)
        {
            // 如果任务已完成，直接返回结果，避免不必要的开销
            if (task.IsCompleted)
            {
                return await task.ConfigureAwait(false);
            }

            // 创建一个永远无法由代码主动完成的 TaskCompletionSource
            // 它只会在取消令牌被触发时由回调完成
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            // 注册取消回调：当 CancellationToken 被取消时，将 tcs 的任务标记为完成
            using var registration = token.Register(
                state => ((TaskCompletionSource)state!).TrySetResult(), tcs);

            // 等待两个任务中的任何一个先完成：原始任务 或 取消信号任务
            Task completedTask = await Task.WhenAny(task, tcs.Task).ConfigureAwait(false);

            // 如果先完成的是取消信号任务，则抛出 OperationCanceledException
            if (completedTask != task)
            {
                throw new OperationCanceledException(token);
            }
            tcs.TrySetResult();

            // 否则，原始任务已完成，返回其结果
            return await task.ConfigureAwait(false);
        }
    }
}
