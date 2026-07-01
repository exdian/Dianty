using System;

namespace Dianty.Utils;

public class LogEntry
{
    public string Timestamp { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public LogEntry() { }

    public LogEntry(string message)
    {
        Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        Message = message;
    }

    public override string ToString() => $"[{Timestamp}] {Message}";
}
