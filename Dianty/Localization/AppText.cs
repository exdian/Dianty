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
    public ToggleSwitchText ToggleSwitchText { get; set; } = new();
    public QrCodeButtonText QrCodeButtonText { get; set; } = new();
    public DeviceDeletionCardText DeviceDeletionCardText { get; set; } = new();
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
    public string AddDeviceButtonText { get; set; } = "添加设备";
    public string BluetoothLabel { get; set; } = "蓝牙";
    public string SocketLabel { get; set; } = "Socket";
    public string CancelButtonText { get; set; } = "取消";
    public string SortButtonText { get; set; } = "排序";
    public string WaitingBluetoothConnectionStateText { get; set; } = "正在搜索并连接";
    public string WaitingSocketServerStateText { get; set; } = "正在连接服务器获取二维码";
    public string WaitingScanQrCodeStateText { get; set; } = "已获取二维码";
    public string ConnectionFailedStateText { get; set; } = "连接失败";
    public string ConnectionSuccessfulStateText { get; set; } = "连接成功";
    public string ConnectionCanceledStateText { get; set; } = "已取消连接";
    public string DefaultDeviceNameLabel { get; set; } = "新设备";
    public string DetailsMenuPath { get; set; } = "详细信息";
    public string DeviceNameCardHeader { get; set; } = "设备名称";
    public string ConnectionStatusCardHeader { get; set; } = "连接状态";
    public string ConnectionStatusCardBluetoothDescription { get; set; } = "此设备通过蓝牙连接";
    public string ConnectionStatusCardSocketDescription { get; set; } = "此设备通过WebSocket连接";
    public string ConnectingStateText { get; set; } = "正在连接";
    public string ConnectedStateText { get; set; } = "已连接";
    public string DisconnectedStateText { get; set; } = "已断开连接";
    public string OutputSwitchCardHeader { get; set; } = "是否允许输出";
    public string StrengthLimitCardHeader { get; set; } = "强度上限";
    public string OutputParametersCardHeader { get; set; } = "输出参数";
    public string OutputParametersCardDescription { get; set; } = "轻柔模式、频率与强度平衡参数";
    public string GentleModeCardHeader { get; set; } = "轻柔模式";
    public string ChannelAFrequencyBalanceSliderHeader { get; set; } = "A通道频率平衡参数";
    public string ChannelAStrengthBalanceSliderHeader { get; set; } = "A通道强度平衡参数";
    public string ChannelBFrequencyBalanceSliderHeader { get; set; } = "B通道频率平衡参数";
    public string ChannelBStrengthBalanceSliderHeader { get; set; } = "B通道强度平衡参数";
    public string MoreInfoCardHeader { get; set; } = "更多";
    public string CurrentChannelAStrengthCardHeader { get; set; } = "当前A通道强度";
    public string CurrentChannelBStrengthCardHeader { get; set; } = "当前B通道强度";
    public string BatteryLevelCardHeader { get; set; } = "电池电量";
    public string ChannelAStrengthLimitCardHeader { get; set; } = "A通道强度上限";
    public string ChannelBStrengthLimitCardHeader { get; set; } = "B通道强度上限";
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

public class ToggleSwitchText
{
    public string OffText { get; set; } = "关";
    public string OnText { get; set; } = "开";
}

public class QrCodeButtonText
{
    public string ToolTip { get; set; } = "查看二维码";
    public string CloseButtonText { get; set; } = "隐藏";
}

public class DeviceDeletionCardText
{
    public string Header { get; set; } = "删除此设备";
    public string Description { get; set; } = "滑动到右侧可从列表中移除此设备";

    public DeviceDeletionConfirmationDialogText ConfirmationDialogText { get; set; } = new();
}

public class DeviceDeletionConfirmationDialogText
{
    public string Title { get; set; } = "是否删除？";
    public string PrimaryButtonText { get; set; } = "确认删除";
    public string CloseButtonText { get; set; } = "取消";
    public string ContentFormat { get; set; } = "从列表中移除“{0}”";
}
