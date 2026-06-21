using GameMonitor;
using SimpleMem;

namespace Dianty.Models;

internal class MemoryService : IMemoryService
{
    private Memory? _mem;

    public bool HasAccessed
    {
        get
        {
            try
            {
                if (_mem is not null && !_mem.Process.HasExited)
                    return true;
                else
                    _mem = null;
            }
            catch { }
            return false;
        }
    }

    public nint BaseAddress { get; private protected set; }

    public bool TryAccessProcess(string processName, string moduleName, out int pid)
    {
        try
        {
            _mem = new Memory(processName, moduleName);
            pid = _mem.Process.Id;
        }
        catch
        {
            _mem = null;
            pid = default;
            return false;
        }
        BaseAddress = _mem.ModuleBaseAddress;
        return true;
    }

    public bool TryReadMemory<T>(nint address, out T value) where T : struct
    {
        if (_mem is not null)
        {
            try
            {
                value = _mem.ReadMemory<T>(address);
                return true;
            }
            catch { }
        }
        value = default;
        return false;
    }

    public bool TryReadMemory<T>(nint address, int[] offsets, out T value) where T : struct
    {
        if (_mem is not null)
        {
            try
            {
                for (var i = 0; i < offsets.Length; i++)
                {
                    var ptr = _mem.ReadMemory<nint>(address);
                    address = ptr + offsets[i];
                }
                value = _mem.ReadMemory<T>(address);
                return true;
            }
            catch { }
        }
        value = default;
        return false;
    }
}
