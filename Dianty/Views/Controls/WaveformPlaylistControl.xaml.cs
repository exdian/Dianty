using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Windows.Input;

namespace Dianty.Views.Controls;

public sealed partial class WaveformPlaylistControl : UserControl
{
    public WaveformPlaylistControl()
    {
        InitializeComponent();
    }

    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty
        = DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(WaveformPlaylistControl),
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
            typeof(WaveformPlaylistControl),
            new PropertyMetadata(false));

    public object WaveformPlayingItemsSource
    {
        get => GetValue(WaveformPlayingItemsSourceProperty);
        set => SetValue(WaveformPlayingItemsSourceProperty, value);
    }

    public static readonly DependencyProperty WaveformPlayingItemsSourceProperty
        = DependencyProperty.Register(
            nameof(WaveformPlayingItemsSource),
            typeof(object),
            typeof(WaveformPlaylistControl),
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
            typeof(WaveformPlaylistControl),
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
            typeof(WaveformPlaylistControl),
            new PropertyMetadata(null));

    public object WaveformIntervalItemsSource
    {
        get => GetValue(WaveformIntervalItemsSourceProperty);
        set => SetValue(WaveformIntervalItemsSourceProperty, value);
    }

    public static readonly DependencyProperty WaveformIntervalItemsSourceProperty
        = DependencyProperty.Register(
            nameof(WaveformIntervalItemsSource),
            typeof(object),
            typeof(WaveformPlaylistControl),
            new PropertyMetadata(null));

    public object SelectedWaveformInterval
    {
        get => GetValue(SelectedWaveformIntervalProperty);
        set => SetValue(SelectedWaveformIntervalProperty, value);
    }

    public static readonly DependencyProperty SelectedWaveformIntervalProperty
        = DependencyProperty.Register(
            nameof(SelectedWaveformInterval),
            typeof(object),
            typeof(WaveformPlaylistControl),
            new PropertyMetadata(null));

    public ICommand PlayWaveformCommand
    {
        get => (ICommand)GetValue(PlayWaveformCommandProperty);
        set => SetValue(PlayWaveformCommandProperty, value);
    }

    public static readonly DependencyProperty PlayWaveformCommandProperty
        = DependencyProperty.Register(
            nameof(PlayWaveformCommand),
            typeof(ICommand),
            typeof(WaveformPlaylistControl),
            new PropertyMetadata(null));

    public ICommand RemoveWaveformCommand
    {
        get => (ICommand)GetValue(RemoveWaveformCommandProperty);
        set => SetValue(RemoveWaveformCommandProperty, value);
    }

    public static readonly DependencyProperty RemoveWaveformCommandProperty
        = DependencyProperty.Register(
            nameof(RemoveWaveformCommand),
            typeof(ICommand),
            typeof(WaveformPlaylistControl),
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
            typeof(WaveformPlaylistControl),
            new PropertyMetadata(null));

    private void OnPlayingButtonClick(object sender, RoutedEventArgs e)
    {
        var command = PlayWaveformCommand;
        if (command is not null && sender is ButtonBase button)
        {
            var commandParameter = button.CommandParameter;
            if (command.CanExecute(commandParameter))
                command.Execute(commandParameter);
        }
    }

    private void OnDeletionButtonClick(object sender, RoutedEventArgs e)
    {
        var command = RemoveWaveformCommand;
        if (command is not null && sender is ButtonBase button)
        {
            var commandParameter = button.CommandParameter;
            if (command.CanExecute(commandParameter))
                command.Execute(commandParameter);
        }
    }
}
