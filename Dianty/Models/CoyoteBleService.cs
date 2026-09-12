using DungeonToolkit.Coyote;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Dianty.Models;

internal partial class CoyoteBleService : ICoyoteBleService
{
    private CoyoteBleService(
        BluetoothLEDevice bleDevice,
        GattCharacteristic commandCharacteristic, GattCharacteristic messageCharacteristic,
        GattCharacteristic? batteryCharacteristic)
    {
        _bleDevice = bleDevice;
        _commandCharacteristic = commandCharacteristic;
        _messageCharacteristic = messageCharacteristic;
        _batteryCharacteristic = batteryCharacteristic;

        _bleDevice.ConnectionStatusChanged += BleDevice_ConnectionStatusChanged;
        _messageCharacteristic.ValueChanged += MessageCharacteristic_ValueChanged;
    }

    private bool _isDisposed;
    private readonly BluetoothLEDevice _bleDevice;
    private readonly GattCharacteristic _commandCharacteristic;
    private readonly GattCharacteristic _messageCharacteristic;
    private readonly GattCharacteristic? _batteryCharacteristic;

    public event EventHandler<ICoyoteBleService.MessageReceivedEventArgs>? MessageReceived;
    public event EventHandler? Disconnected;

    public static async Task<CoyoteBleService> CreateCoyoteBleServiceAsync(
        ulong address, GattServiceInfo gattServiceInfo, CancellationToken token)
    {
        BluetoothLEDevice? bleDevice = null;
        CoyoteBleService? result = null;
        try
        {
            bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(address).AsTask(token).ConfigureAwait(false);
            bleDevice ??= await BluetoothLEDevice.FromBluetoothAddressAsync(address).AsTask(token).ConfigureAwait(false)
                ?? throw new Exception("设备连接失败");

            // 获取 GATT 服务
            var servicesResult = await bleDevice.GetGattServicesAsync().AsTask(token).ConfigureAwait(false);
            if (servicesResult.Status != GattCommunicationStatus.Success)
                servicesResult = await bleDevice.GetGattServicesAsync().AsTask(token).ConfigureAwait(false);
            if (servicesResult.Status != GattCommunicationStatus.Success)
                throw new Exception($"服务获取失败: {servicesResult.Status}");
            var commandService = servicesResult.Services.FirstOrDefault(s => s.Uuid == gattServiceInfo.CommandServiceUuid)
                ?? throw new Exception("未找到命令服务");
            var batteryService = servicesResult.Services.FirstOrDefault(s => s.Uuid == gattServiceInfo.BatteryServiceUuid);

            // 获取命令特征和消息特征
            var characteristicsResult = await commandService.GetCharacteristicsAsync().AsTask(token).ConfigureAwait(false);
            if (characteristicsResult.Status != GattCommunicationStatus.Success)
                characteristicsResult = await commandService.GetCharacteristicsAsync().AsTask(token).ConfigureAwait(false);
            if (characteristicsResult.Status != GattCommunicationStatus.Success)
                throw new Exception($"特征获取失败: {characteristicsResult.Status}");
            var commandCharacteristic = characteristicsResult.Characteristics.FirstOrDefault(c => c.Uuid == gattServiceInfo.CommandCharacteristicUuid)
                ?? throw new Exception("未找到命令特征");
            var messageCharacteristic = characteristicsResult.Characteristics.FirstOrDefault(c => c.Uuid == gattServiceInfo.MessageCharacteristicUuid)
                ?? throw new Exception("未找到消息特征");
            if (!commandCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.WriteWithoutResponse))
                throw new Exception("命令特征权限错误");

            // 获取电池特征
            GattCharacteristic? batteryCharacteristic = null;
            if (batteryService is not null)
            {
                var batteryCharacteristicsResult = await batteryService.GetCharacteristicsAsync().AsTask(token).ConfigureAwait(false);
                batteryCharacteristic = batteryCharacteristicsResult.Characteristics.FirstOrDefault(c => c.Uuid == gattServiceInfo.BatteryCharacteristicUuid);
            }

            // 订阅消息通知
            result = new CoyoteBleService(bleDevice, commandCharacteristic, messageCharacteristic, batteryCharacteristic);
            var operationResult = await messageCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                GattClientCharacteristicConfigurationDescriptorValue.Notify).AsTask(token).ConfigureAwait(false);
            if (operationResult != GattCommunicationStatus.Success)
            {
                operationResult = await messageCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.Notify).AsTask(token).ConfigureAwait(false);
            }
            if (operationResult != GattCommunicationStatus.Success)
            {
                result.Dispose();
                throw new Exception($"消息特征描述符写入失败: {operationResult}");
            }

            return result;
        }
        catch
        {
            if (result is not null)
            {
                result.Dispose();
            }
            else if (bleDevice is not null)
            {
                try
                {
                    foreach (var service in bleDevice.GattServices.Reverse())
                    {
                        service.Dispose();
                    }
                }
                catch { }
                bleDevice.Dispose();
            }
            throw;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        if (disposing)
        {
            _bleDevice.ConnectionStatusChanged -= BleDevice_ConnectionStatusChanged;
            _messageCharacteristic.ValueChanged -= MessageCharacteristic_ValueChanged;
            try
            {
                foreach (var service in _bleDevice.GattServices.Reverse())
                {
                    service.Dispose();
                }
            }
            catch { }
            _bleDevice.Dispose();
        }
    }

    public async Task<bool> SendCommandAsync(byte[] command)
    {
        if (_isDisposed)
            return false;

        var result = await _commandCharacteristic.WriteValueAsync(command.AsBuffer()).AsTask().ConfigureAwait(false);
        return result == GattCommunicationStatus.Success;
    }

    public async Task<byte?> ReadBatteryLevelAsync()
    {
        if (_isDisposed || _batteryCharacteristic == null)
            return null;

        var operationResult = await _batteryCharacteristic.ReadValueAsync().AsTask().ConfigureAwait(false);
        if (operationResult.Status != GattCommunicationStatus.Success)
            return null;
        var result = operationResult.Value.ToArray();
        if (result.Length < 1)
            return null;
        return result[0];
    }

    private void OnDisconnected()
    {
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    private void OnMessageReceived(byte[] message)
    {
        var args = new ICoyoteBleService.MessageReceivedEventArgs(message);
        MessageReceived?.Invoke(this, args);
    }

    private void BleDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
    {
        if (sender.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
        {
            Dispose();
            OnDisconnected();
        }
    }

    private void MessageCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
    {
        var message = args.CharacteristicValue.ToArray();
        OnMessageReceived(message);
    }
}
