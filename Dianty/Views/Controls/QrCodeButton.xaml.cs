using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Threading.Tasks;

namespace Dianty.Views.Controls;

public sealed partial class QrCodeButton : UserControl
{
    public bool IsDisplayed
    {
        get => (bool)GetValue(IsDisplayedProperty);
        set => SetValue(IsDisplayedProperty, value);
    }

    public static readonly DependencyProperty IsDisplayedProperty
        = DependencyProperty.Register(
            nameof(IsDisplayed),
            typeof(bool),
            typeof(QrCodeButton),
            new PropertyMetadata(null, OnIsDisplayedChanged));

    public Geometry QrCodeGeometry
    {
        get => (Geometry)GetValue(QrCodeGeometryProperty);
        set => SetValue(QrCodeGeometryProperty, value);
    }

    public static readonly DependencyProperty QrCodeGeometryProperty
        = DependencyProperty.Register(
            nameof(QrCodeGeometry),
            typeof(Geometry),
            typeof(QrCodeButton),
            new PropertyMetadata(null));

    private static void OnIsDisplayedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is QrCodeButton button)
            button.SetQrCodeDisplay();
    }

    public QrCodeButton()
    {
        InitializeComponent();

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        SetQrCodeDisplay();
    }

    private async void SetQrCodeDisplay()
    {
        // TeachingTip 在打开和关闭动画途中不能响应开关
        int i = 0;
        while (i < 10 && IsLoaded && _qrCodeTip.IsOpen != IsDisplayed)
        {
            _qrCodeTip.IsOpen = IsDisplayed;
            await Task.Delay(100);
            i++;
        }
    }

    private void OnDisplayButtonClick(object sender, RoutedEventArgs e)
    {
        _qrCodeTip.IsOpen = !_qrCodeTip.IsOpen;
    }
}
