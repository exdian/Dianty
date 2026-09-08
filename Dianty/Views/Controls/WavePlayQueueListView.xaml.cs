using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Windows.Input;

namespace Dianty.Views.Controls;

public sealed partial class WavePlayQueueListView : UserControl
{
    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty
        = DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(WavePlayQueueListView),
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
            typeof(WavePlayQueueListView),
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
            typeof(WavePlayQueueListView),
            new PropertyMetadata(null));

    public object PlaybackModeItemsSource
    {
        get => GetValue(PlaybackModeItemsSourceProperty);
        set => SetValue(PlaybackModeItemsSourceProperty, value);
    }

    public static readonly DependencyProperty PlaybackModeItemsSourceProperty
        = DependencyProperty.Register(
            nameof(PlaybackModeItemsSource),
            typeof(object),
            typeof(WavePlayQueueListView),
            new PropertyMetadata(null));

    public object SelectedPlaybackMode
    {
        get => GetValue(SelectedPlaybackModeProperty);
        set => SetValue(SelectedPlaybackModeProperty, value);
    }

    public static readonly DependencyProperty SelectedPlaybackModeProperty
        = DependencyProperty.Register(
            nameof(SelectedPlaybackMode),
            typeof(object),
            typeof(WavePlayQueueListView),
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
            typeof(WavePlayQueueListView),
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
            typeof(WavePlayQueueListView),
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
            typeof(WavePlayQueueListView),
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
            typeof(WavePlayQueueListView),
            new PropertyMetadata(null));

    public ICommand ClearPlayingItemsCommand
    {
        get => (ICommand)GetValue(ClearPlayingItemsCommandProperty);
        set => SetValue(ClearPlayingItemsCommandProperty, value);
    }

    public static readonly DependencyProperty ClearPlayingItemsCommandProperty
        = DependencyProperty.Register(
            nameof(ClearPlayingItemsCommand),
            typeof(ICommand),
            typeof(WavePlayQueueListView),
            new PropertyMetadata(null));

    public WavePlayQueueListView()
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
