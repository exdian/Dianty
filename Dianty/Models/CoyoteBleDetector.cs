using DungeonToolkit.Coyote;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;

namespace Dianty.Models;

internal sealed class CoyoteBleDetector : ICoyoteBleDetector
{
    public async Task<ulong?> ScanAsync(Guid uuid, string name, CancellationToken cancellationToken)
    {
        var watcher = GetWatcher();
        var tcs = new TaskCompletionSource();
        ulong? result = null;
        watcher.Received += Watcher_Received;

        void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            if (tcs.Task.IsCompleted)
                return;

            var localName = args.Advertisement.LocalName;
            var serviceUuids = args.Advertisement.ServiceUuids;
            if (serviceUuids.Contains(uuid) && localName == name)
            {
                result = args.BluetoothAddress;
                tcs.TrySetResult();
            }
        }

        try
        {
            watcher.Start();
        }
        catch
        {
            watcher.Received -= Watcher_Received;
            return null;
        }

        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(30), cancellationToken)).ConfigureAwait(false);
        watcher.Stop();
        watcher.Received -= Watcher_Received;
        if (cancellationToken.IsCancellationRequested)
        {
            tcs.TrySetResult();
            await completedTask.ConfigureAwait(false);
        }
        return result;
    }

    public async Task<bool> ScanAsync(ulong address, CancellationToken cancellationToken)
    {
        var watcher = GetWatcher();
        var tcs = new TaskCompletionSource();
        watcher.Received += Watcher_Received;

        void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            if (tcs.Task.IsCompleted)
                return;

            if (args.BluetoothAddress == address)
            {
                tcs.TrySetResult();
            }
        }

        try
        {
            watcher.Start();
        }
        catch
        {
            watcher.Received -= Watcher_Received;
            return false;
        }

        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(30), cancellationToken)).ConfigureAwait(false);
        watcher.Stop();
        watcher.Received -= Watcher_Received;
        if (cancellationToken.IsCancellationRequested)
        {
            tcs.TrySetResult();
            await completedTask.ConfigureAwait(false);
        }
        return completedTask == tcs.Task;
    }

    public async Task<ICoyoteBleService?> ConnectAsync(ulong address, GattServiceInfo gattServiceInfo, CancellationToken cancellationToken)
    {
        try
        {
            return await CoyoteBleService.CreateCoyoteBleServiceAsync(address, gattServiceInfo, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    private static BluetoothLEAdvertisementWatcher GetWatcher()
    {
        return new BluetoothLEAdvertisementWatcher
        {
            ScanningMode = BluetoothLEScanningMode.Active,
            SignalStrengthFilter =
            {
                OutOfRangeTimeout = TimeSpan.FromMilliseconds(5000),
                InRangeThresholdInDBm = -75,
                OutOfRangeThresholdInDBm = -90
            }
        };
    }
}
