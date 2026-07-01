using Dianty.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dianty.Views.Controls;

internal partial class TemplateContentControl : ContentControl
{
    public ITemplateContent TemplateContent
    {
        get => (ITemplateContent)GetValue(TemplateContentProperty);
        set => SetValue(TemplateContentProperty, value);
    }

    public static readonly DependencyProperty TemplateContentProperty
        = DependencyProperty.Register(
            nameof(TemplateContent),
            typeof(ITemplateContent),
            typeof(TemplateContentControl),
            new PropertyMetadata(null, OnTemplateContentChanged));

    private static void OnTemplateContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (TemplateContentControl)d;
        var templateContent = (ITemplateContent)e.NewValue;

        if (templateContent is not null)
        {
            control.Content = templateContent.CreateContent(control.SelectedTemplate);
        }
        //else
        //{
        //    control.Content = null;
        //}
    }

    public object SelectedTemplate
    {
        get => GetValue(SelectedTemplateProperty);
        set => SetValue(SelectedTemplateProperty, value);
    }

    public static readonly DependencyProperty SelectedTemplateProperty
        = DependencyProperty.Register(
            nameof(SelectedTemplate),
            typeof(object),
            typeof(TemplateContentControl),
            new PropertyMetadata(null, OnSelectedTemplateChanged));

    private static void OnSelectedTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (TemplateContentControl)d;
        var templateContent = control.TemplateContent;

        if (templateContent is not null)
        {
            control.Content = templateContent.CreateContent(e.NewValue);
        }
        //else
        //{
        //    control.Content = null;
        //}
    }
}
