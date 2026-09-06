namespace Dianty.Localization;

public class AppText
{
    public string Language { get; set; } = string.Empty;

    public MainWindowText MainWindowText { get; set; } = new();
}

public class MainWindowText
{
    public MainViewText MainViewText { get; set; } = new();
}

public class MainViewText
{
    public string HomeMenuItem { get; set; } = "主页";
    public string DevicesMenuItem { get; set; } = "设备";
    public string WavesMenuItem { get; set; } = "波形";
    public string GamesMenuItem { get; set; } = "玩法";
    public string SafetyMenuItem { get; set; } = "安全";
    public string DebugMenuItem { get; set; } = "调试";
    public string SettingsMenuItem { get; set; } = "设置";

    public HomePageText HomePageText { get; set; } = new();
    public DevicesPageText DevicesPageText { get; set; } = new();
    public WavesPageText WavesPageText { get; set; } = new();
    public GamesPageText GamesPageText { get; set; } = new();
    public SafetyPageText SafetyPageText { get; set; } = new();
    public DebugPageText DebugPageText { get; set; } = new();
    public SettingsPageText SettingsPageText { get; set; } = new();
}

public class HomePageText
{

}

public class DevicesPageText
{

}

public class WavesPageText
{

}

public class GamesPageText
{

}

public class SafetyPageText
{

}

public class DebugPageText
{

}

public class SettingsPageText
{

}
