using DungeonToolkit.Coyote;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;

namespace Dianty.Models;

internal class CoyoteBleDetector : ICoyoteBleDetector
{
    public async Task<ulong?> ScanAsync(Guid uuid, string name, CancellationToken token)
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

        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(30), token));
        watcher.Stop();
        watcher.Received -= Watcher_Received;
        if (token.IsCancellationRequested)
        {
            tcs.TrySetCanceled(token);
            await completedTask;
        }
        return result;
    }

    public async Task<bool> ScanAsync(ulong address, CancellationToken token)
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

        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(30), token));
        watcher.Stop();
        watcher.Received -= Watcher_Received;
        if (token.IsCancellationRequested)
        {
            tcs.TrySetCanceled(token);
            await completedTask;
        }
        return completedTask == tcs.Task;
    }

    public async Task<ICoyoteBleService?> ConnectAsync(ulong address, GattServiceInfo gattServiceInfo, CancellationToken token)
    {
        try
        {
            return await CoyoteBleService.CreateCoyoteBleServiceAsync(address, gattServiceInfo, token);
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
