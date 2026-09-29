using CommunityToolkit.Mvvm.Messaging;
using Dianty.Services;
using Dianty.Utils.Messages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static Dianty.Services.ILocalizationService;

namespace Dianty.Views.Controls;

public sealed partial class PathBar : UserControl
{
    public RequiredParameter Parameter
    {
        get => (RequiredParameter)GetValue(ParameterProperty);
        set => SetValue(ParameterProperty, value);
    }

    public static readonly DependencyProperty ParameterProperty
        = DependencyProperty.Register(
            nameof(Parameter),
            typeof(RequiredParameter),
            typeof(PathBar),
            new PropertyMetadata(null, OnParameterChanged));

    private static void OnParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (PathBar)d;
        var newValue = (RequiredParameter)e.NewValue;
        var oldValue = (RequiredParameter)e.OldValue;

        oldValue?.LocalizationService.CurrentLanguageFileNameChanged -= control.OnCurrentLanguageFileNameChanged;
        newValue?.LocalizationService.CurrentLanguageFileNameChanged += control.OnCurrentLanguageFileNameChanged;
        control.UpdatePaths(newValue);
    }

    public PathBar()
    {
        InitializeComponent();
        Loaded += OnPathBarLoaded;
        Unloaded += OnPathBarUnloaded;
    }

    private ObservableCollection<string> Paths { get; } = [];

    private void UpdatePaths(RequiredParameter? parameter)
    {
        if (parameter is null)
            return;

        var paths = parameter.PathGetters.Select(f => f.Invoke(parameter.LocalizationService));
        parameter.QueueService.TryEnqueue(() =>
        {
            Paths.Clear();
            foreach (string path in paths)
            {
                Paths.Add(path);
            }
        });
    }

    private void OnPathBarLoaded(object sender, RoutedEventArgs e)
    {
        var localizationService = Parameter?.LocalizationService;
        if (localizationService is not null)
        {
            localizationService.CurrentLanguageFileNameChanged -= OnCurrentLanguageFileNameChanged;
            localizationService.CurrentLanguageFileNameChanged += OnCurrentLanguageFileNameChanged;
        }
    }

    private void OnPathBarUnloaded(object sender, RoutedEventArgs e)
    {
        Parameter?.LocalizationService.CurrentLanguageFileNameChanged -= OnCurrentLanguageFileNameChanged;
    }

    private void OnCurrentLanguageFileNameChanged(object? sender, CurrentLanguageFileNameChangedEventArgs e)
    {
        UpdatePaths(Parameter);
    }

    private void OnBreadcrumbBarItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        var options = new FrameNavigationOptions
        {
            IsNavigationStackEnabled = true,
            TransitionInfoOverride = new SlideNavigationTransitionInfo
            {
                Effect = SlideNavigationTransitionEffect.FromLeft
            }
        };
        var navigationRequest = new NavigationRequest(Paths.Count - 1 - args.Index, options);
        for (int i = Paths.Count - 1; i > args.Index; i--)
        {
            Paths.RemoveAt(i);
        }

        WeakReferenceMessenger.Default.Send(navigationRequest);
    }

    public record class RequiredParameter(
        IQueueService QueueService, ILocalizationService LocalizationService,
        IReadOnlyList<Func<ILocalizationService, string>> PathGetters)
    {
        public RequiredParameter Concat(IEnumerable<Func<ILocalizationService, string>> tail)
        {
            return this with { PathGetters = [.. PathGetters, .. tail] };
        }
    }
}
