using CommunityToolkit.Mvvm.Messaging;
using Dianty.Localization;
using Dianty.Services.Messages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Windows.Input;

namespace Dianty.Views.Controls;

public sealed partial class CoyoteDeletionCard : UserControl
{
    public DeviceDeletionConfirmationDialogText LocalizedText
    {
        get => (DeviceDeletionConfirmationDialogText)GetValue(LocalizedTextProperty);
        set => SetValue(LocalizedTextProperty, value);
    }

    public static readonly DependencyProperty LocalizedTextProperty
        = DependencyProperty.Register(
            nameof(LocalizedText),
            typeof(DeviceDeletionConfirmationDialogText),
            typeof(CoyoteDeletionCard),
            new PropertyMetadata(null));

    public string TargetName
    {
        get => (string)GetValue(TargetNameProperty);
        set => SetValue(TargetNameProperty, value);
    }

    public static readonly DependencyProperty TargetNameProperty
        = DependencyProperty.Register(
            nameof(TargetName),
            typeof(string),
            typeof(CoyoteDeletionCard),
            new PropertyMetadata(null));

    public ICommand DeleteCommand
    {
        get => (ICommand)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    public static readonly DependencyProperty DeleteCommandProperty
        = DependencyProperty.Register(
            nameof(DeleteCommand),
            typeof(ICommand),
            typeof(CoyoteDeletionCard),
            new PropertyMetadata(null));

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty
        = DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(CoyoteDeletionCard),
            new PropertyMetadata(null));

    public CoyoteDeletionCard()
    {
        InitializeComponent();

        _deletionSlider.AddHandler(PointerReleasedEvent, new PointerEventHandler(OnDeletionSliderPointerReleased), true);
    }

    private int _deletionSliderOldValue;
    private int _deletionSliderNewValue;

    private void OnDeletionSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        int newValue = (int)e.NewValue;
        if (newValue % 10 == 0)
        {
            _deletionSliderOldValue = (int)e.OldValue;
            _deletionSliderNewValue = newValue;
        }
        else
        {
            _deletionSlider.Value = newValue - newValue % 10;
        }
    }

    private async void OnDeletionSliderPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_deletionSliderNewValue >= 90 && _deletionSliderOldValue > 0 && DeleteCommand is not null)
        {
            _deletionSlider.Value = 100;

            var dialogText = LocalizedText;
            var dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = dialogText?.Title ?? "Delete?",
                PrimaryButtonText = dialogText?.PrimaryButtonText ?? "Confirm Delete",
                CloseButtonText = dialogText?.CloseButtonText ?? "Cancel",
                IsSecondaryButtonEnabled = false,
                DefaultButton = ContentDialogButton.None,
                Content = string.Format(dialogText?.ContentFormat ?? "Remove \"{0}\" from the list", TargetName),
                RequestedTheme = ActualTheme
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                var command = DeleteCommand;
                var parameter = CommandParameter;
                if (command is not null && command.CanExecute(parameter))
                {
                    command.Execute(parameter);

                    var navigationOptions = new FrameNavigationOptions
                    {
                        IsNavigationStackEnabled = false,
                        TransitionInfoOverride = new SlideNavigationTransitionInfo
                        {
                            Effect = SlideNavigationTransitionEffect.FromLeft
                        }
                    };
                    var navigationRequest = new NavigationRequest(1, navigationOptions);
                    WeakReferenceMessenger.Default.Send(navigationRequest);
                    return;
                }
            }
        }
        _deletionSlider.Value = 0;
    }
}
