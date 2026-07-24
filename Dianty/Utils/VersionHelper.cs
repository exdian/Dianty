using System.Diagnostics;

namespace Dianty.Utils;

public class VersionHelper
{
    public static string? Version { get; private set; }

    public static void Init()
    {
        var process = Process.GetCurrentProcess();
        var fileVersionInfo = process.MainModule?.FileVersionInfo;
        if (fileVersionInfo is not null)
        {
            Version = $"{fileVersionInfo.FileMajorPart}.{fileVersionInfo.FileMinorPart}.{fileVersionInfo.FileBuildPart}.{fileVersionInfo.FilePrivatePart}";
        }
    }
}
