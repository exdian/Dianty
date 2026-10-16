using System;

namespace Dianty.Services.Messages;

public sealed class Log
{
    public string Timestamp { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public Log() { }

    public Log(string message)
    {
        Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        Message = message;
    }

    public override string ToString() => $"[{Timestamp}] {Message}";
}

public sealed class LogAppendedMessage { }
