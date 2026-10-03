using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DungeonToolkit;
using DungeonToolkit.Coyote;
using System;
using System.Threading;

namespace Dianty.ViewModels;

public sealed partial class SafetyPageViewModel : ObservableObject
{
    private readonly IDataSource<CoyoteItemBase> _coyoteDataSource;
    private readonly IDataSource<IWaveformOutputDevice> _waveformOutputDeviceDataSource;
    private readonly IDataSource<IConnectableDevice> _connectableDeviceDataSource;

    public SafetyPageViewModel(
        IDataSource<CoyoteItemBase> coyoteDataSource,
        IDataSource<IWaveformOutputDevice> WaveformOutputDeviceDataSource,
        IDataSource<IConnectableDevice> connectableDeviceDataSource)
    {
        _coyoteDataSource = coyoteDataSource;
        _waveformOutputDeviceDataSource = WaveformOutputDeviceDataSource;
        _connectableDeviceDataSource = connectableDeviceDataSource;
    }

    [RelayCommand]
    private void Disable()
    {
        var devices = _coyoteDataSource.GetSnapshot();
        var outputDevices = _waveformOutputDeviceDataSource.GetSnapshot();

        var count = Math.Min(devices.Count, outputDevices.Count);
        for (int i = 0; i < count; i++)
        {
            devices[i].IsEnabled = false;
            outputDevices[i].CancelOutputAsync();
        }

        for (int i = count; i < devices.Count; i++)
        {
            devices[i].IsEnabled = false;
        }

        for (int i = count; i < outputDevices.Count; i++)
        {
            outputDevices[i].CancelOutputAsync();
        }
    }

    [RelayCommand]
    private void Disconnect()
    {
        var connectableDevices = _connectableDeviceDataSource.GetSnapshot();

        foreach (var item in connectableDevices)
        {
            item.DisconnectAsync(CancellationToken.None);
        }
    }
}
