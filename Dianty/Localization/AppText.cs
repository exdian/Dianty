namespace Dianty.Localization;

public class AppText
{
    public string LanguageLabel { get; set; } = string.Empty;

    public MainWindowText MainWindowText { get; set; } = new();
}

public class MainWindowText
{
    public CrashDialogText CrashDialogText { get; set; } = new();
    public MainViewText MainViewText { get; set; } = new();
}

public class CrashDialogText
{
    public string Title { get; set; } = "出错了";
    public string CloseButtonText { get; set; } = "结束运行";
}

public class MainViewText
{
    public string MenuHome { get; set; } = "主页";
    public string MenuDevices { get; set; } = "设备";
    public string MenuWaves { get; set; } = "波形";
    public string MenuGames { get; set; } = "玩法";
    public string MenuSafety { get; set; } = "安全";
    public string MenuDebug { get; set; } = "调试";
    public string MenuSettings { get; set; } = "设置";

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
    public string DeviceManagementCardHeader { get; set; } = "设备管理";
    public string ConnectedCountFormat { get; set; } = "已连接 {0}";
    public string AddedCountFormat { get; set; } = "已添加 {0}";
    public string NowPlayingCardHeader { get; set; } = "正在播放";
    public string ChannelAFormat { get; set; } = "A通道 {0}";
    public string ChannelBFormat { get; set; } = "B通道 {0}";
    public string OutputStrengthCardHeader { get; set; } = "输出强度";
    public string CurrentStrengthFormat { get; set; } = "当前强度 {0}";
    public string NoneLabel { get; set; } = "无";
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
