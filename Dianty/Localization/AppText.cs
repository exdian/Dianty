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
    public string ChannelAQueueSwitchHeader { get; set; } = "A通道队列";
    public string ChannelBQueueSwitchHeader { get; set; } = "B通道队列";
    public string ImportWaveButtonText { get; set; } = "导入波形";
    public string DeleteButtonText { get; set; } = "删除";
    public string SortButtonText { get; set; } = "排序";
    public string QueueButtonText { get; set; } = "播放队列";
    public string WaveFilePickerTitle { get; set; } = "打开";
    public string WaveFilesLabel { get; set; } = "波形文件";
    public string AllFilesLabel { get; set; } = "所有文件";
    public string DefaultWaveNameLabel { get; set; } = "新波形";
    public string ChannelALabel { get; set; } = "A通道";
    public string ChannelBLabel { get; set; } = "B通道";
    public string QueueMenuPath { get; set; } = "播放队列";
    public string ClearButtonText { get; set; } = "清空";
    public string QueueModePlaceholderText { get; set; } = "播放模式";
    public string QueueIntervalPlaceholderText { get; set; } = "切换间隔";
    public string QueueIntervalUnitText { get; set; } = "秒";
    public string DetailsMenuPath { get; set; } = "详细信息";
    public string WaveNameCardHeader { get; set; } = "名称";
    public string WaveDescriptionCardHeader { get; set; } = "备注";
    public string WaveQueueSwitchCardHeader { get; set; } = "将波形加入到播放队列";
    public string ChannelAQueueCardHeader { get; set; } = "A通道播放队列";
    public string ChannelBQueueCardHeader { get; set; } = "B通道播放队列";

    public ErrorProcessingFileDialogText ErrorProcessingFileDialogText { get; set; } = new();
    public ClassicWavesNameLabel ClassicWavesNameLabel { get; set; } = new();
}

public class ErrorProcessingFileDialogText
{
    public string AllFailedTitle { get; set; } = "发生错误";
    public string PartiallyFailedTitle { get; set; } = "处理部分文件时发生错误";
    public string CloseButtonText { get; set; } = "关闭";
    public string ContentFirstLineText { get; set; } = "处理文件时出错：";
    public string FailureCountFormat { get; set; } = "({0}个) ：";
}

public class ClassicWavesNameLabel
{
    public string Breathing { get; set; } = "呼吸";
    public string Tide { get; set; } = "潮汐";
    public string Pulsating { get; set; } = "连击";
    public string QuickRub { get; set; } = "快速按捏";
    public string GradualRub { get; set; } = "按捏渐强";
    public string Heartbeat { get; set; } = "心跳节奏";
    public string Compress { get; set; } = "压缩";
    public string Rhythmic { get; set; } = "节奏步伐";
    public string Grainy { get; set; } = "颗粒摩擦";
    public string Bouncy { get; set; } = "渐变弹跳";
    public string Ripple { get; set; } = "波浪涟漪";
    public string Rainfall { get; set; } = "雨水冲刷";
    public string TempoTap { get; set; } = "变速敲击";
    public string Signal { get; set; } = "信号灯";
    public string Tease1 { get; set; } = "挑逗1";
    public string Tease2 { get; set; } = "挑逗2";
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
