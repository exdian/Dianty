using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Windows.Input;

namespace Dianty.Views.Controls;

public sealed partial class WavePlayingQueueListView : UserControl
{
    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty
        = DependencyProperty.Register(
            nameof(Header),
            typeof(string),
            typeof(WavePlayingQueueListView),
            new PropertyMetadata(null));

    public bool IsChannelEnabled
    {
        get => (bool)GetValue(IsChannelEnabledProperty);
        set => SetValue(IsChannelEnabledProperty, value);
    }

    public static readonly DependencyProperty IsChannelEnabledProperty
        = DependencyProperty.Register(
            nameof(IsChannelEnabled),
            typeof(bool),
            typeof(WavePlayingQueueListView),
            new PropertyMetadata(false));

    public object WavePlayingItemsSource
    {
        get => GetValue(WavePlayingItemsSourceProperty);
        set => SetValue(WavePlayingItemsSourceProperty, value);
    }

    public static readonly DependencyProperty WavePlayingItemsSourceProperty
        = DependencyProperty.Register(
            nameof(WavePlayingItemsSource),
            typeof(object),
            typeof(WavePlayingQueueListView),
            new PropertyMetadata(null));

    public object PlayingModeItemsSource
    {
        get => GetValue(PlayingModeItemsSourceProperty);
        set => SetValue(PlayingModeItemsSourceProperty, value);
    }

    public static readonly DependencyProperty PlayingModeItemsSourceProperty
        = DependencyProperty.Register(
            nameof(PlayingModeItemsSource),
            typeof(object),
            typeof(WavePlayingQueueListView),
            new PropertyMetadata(null));

    public object SelectedPlayingMode
    {
        get => GetValue(SelectedPlayingModeProperty);
        set => SetValue(SelectedPlayingModeProperty, value);
    }

    public static readonly DependencyProperty SelectedPlayingModeProperty
        = DependencyProperty.Register(
            nameof(SelectedPlayingMode),
            typeof(object),
            typeof(WavePlayingQueueListView),
            new PropertyMetadata(null));

    public object WaveIntervalItemsSource
    {
        get => GetValue(WaveIntervalItemsSourceProperty);
        set => SetValue(WaveIntervalItemsSourceProperty, value);
    }

    public static readonly DependencyProperty WaveIntervalItemsSourceProperty
        = DependencyProperty.Register(
            nameof(WaveIntervalItemsSource),
            typeof(object),
            typeof(WavePlayingQueueListView),
            new PropertyMetadata(null));

    public object SelectedWaveInterval
    {
        get => GetValue(SelectedWaveIntervalProperty);
        set => SetValue(SelectedWaveIntervalProperty, value);
    }

    public static readonly DependencyProperty SelectedWaveIntervalProperty
        = DependencyProperty.Register(
            nameof(SelectedWaveInterval),
            typeof(object),
            typeof(WavePlayingQueueListView),
            new PropertyMetadata(null));

    public ICommand PlayWaveCommand
    {
        get => (ICommand)GetValue(PlayWaveCommandProperty);
        set => SetValue(PlayWaveCommandProperty, value);
    }

    public static readonly DependencyProperty PlayWaveCommandProperty
        = DependencyProperty.Register(
            nameof(PlayWaveCommand),
            typeof(ICommand),
            typeof(WavePlayingQueueListView),
            new PropertyMetadata(null));

    public ICommand RemoveWaveCommand
    {
        get => (ICommand)GetValue(RemoveWaveCommandProperty);
        set => SetValue(RemoveWaveCommandProperty, value);
    }

    public static readonly DependencyProperty RemoveWaveCommandProperty
        = DependencyProperty.Register(
            nameof(RemoveWaveCommand),
            typeof(ICommand),
            typeof(WavePlayingQueueListView),
            new PropertyMetadata(null));

    public WavePlayingQueueListView()
    {
        InitializeComponent();
    }

    private void OnPlayingButtonClick(object sender, RoutedEventArgs e)
    {
        var command = PlayWaveCommand;
        if (command is not null && sender is ButtonBase button)
        {
            var commandParameter = button.CommandParameter;
            if (command.CanExecute(commandParameter))
                command.Execute(commandParameter);
        }
    }

    private void OnDeletionButtonClick(object sender, RoutedEventArgs e)
    {
        var command = RemoveWaveCommand;
        if (command is not null && sender is ButtonBase button)
        {
            var commandParameter = button.CommandParameter;
            if (command.CanExecute(commandParameter))
                command.Execute(commandParameter);
        }
    }
}
