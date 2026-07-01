using GameMonitor;
using SimpleMem;
using System.Text;

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

    public bool TryReadUtf16Le(nint address, int maxChars, out string text)
    {
        if (_mem is not null)
        {
            try
            {
                text = ReadUtf16Le(_mem, address, maxChars);
                return true;
            }
            catch { }
        }
        text = string.Empty;
        return false;
    }

    public bool TryReadUtf16Le(nint address, int[] offsets, int maxChars, out string text)
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
                text = ReadUtf16Le(_mem, address, maxChars);
                return true;
            }
            catch { }
        }
        text = string.Empty;
        return false;
    }

    private static string ReadUtf16Le(Memory mem, nint address, int maxChars)
    {
        var buffer = new StringBuilder(maxChars);
        int i = 0;
        while (i < maxChars)
        {
            int remaining = maxChars - i;
            if (remaining >= 2)
            {
                // 一次读取 4 字节，包含两个 UTF-16 字符
                uint value = mem.ReadMemory<uint>(address + i * 2);
                ushort c1 = (ushort)(value & 0xFFFF);
                ushort c2 = (ushort)(value >> 16);
                if (c1 == 0)
                    break;
                buffer.Append((char)c1);
                if (c2 == 0)
                    break;
                buffer.Append((char)c2);
                i = i + 2;
            }
            else
            {
                ushort c = mem.ReadMemory<ushort>(address + i * 2);
                if (c == 0)
                    break;
                buffer.Append((char)c);
                i++;
            }
        }
        return buffer.ToString();
    }
}
