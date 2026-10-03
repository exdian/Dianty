using System.Diagnostics;

namespace Dianty.Utils;

public static class VersionHelper
{
    public static string GetVersion()
    {
        var process = Process.GetCurrentProcess();
        var fileVersionInfo = process.MainModule?.FileVersionInfo;
        if (fileVersionInfo is not null)
            return $"{fileVersionInfo.FileMajorPart}.{fileVersionInfo.FileMinorPart}.{fileVersionInfo.FileBuildPart}.{fileVersionInfo.FilePrivatePart}";
        else
            return "0.0.0.0";
    }
}
