using CommunityToolkit.WinUI.Controls;
using Dianty.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dianty.Views.Controls;

public sealed partial class ObjectIconSettingsCard : SettingsCard
{
    private Viewbox? _headerIconPresenterHolder;
    private ContentPresenter? _headerIconPresenter;

    public object ObjectIcon
    {
        get => GetValue(ObjectIconProperty);
        set => SetValue(ObjectIconProperty, value);
    }

    public static readonly DependencyProperty ObjectIconProperty
        = DependencyProperty.Register(
            nameof(ObjectIcon),
            typeof(object),
            typeof(ObjectIconSettingsCard),
            new PropertyMetadata(null, OnObjectIconChanged));

    private static void OnObjectIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ObjectIconSettingsCard)d).UpdateHeaderIcon();
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (VisualTreeHelperExtension.FindChildByName(this, "PART_HeaderIconPresenterHolder") is Viewbox viewbox)
        {
            _headerIconPresenterHolder = viewbox;
            _headerIconPresenter = VisualTreeHelperExtension.FindChildByName(viewbox, "PART_HeaderIconPresenter") as ContentPresenter;
            _headerIconPresenterHolder.Visibility = ObjectIcon == null ? Visibility.Collapsed : Visibility.Visible;
        }
        UpdateHeaderIcon();
    }

    private void UpdateHeaderIcon()
    {
        _headerIconPresenter?.Content = ObjectIcon;
    }
}
