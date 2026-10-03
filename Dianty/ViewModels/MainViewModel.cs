using CommunityToolkit.Mvvm.ComponentModel;
using Dianty.Services;

namespace Dianty.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    public MainViewModel(ITemplateContent templateContent)
    {
        TemplateContent = templateContent;
    }

    public ITemplateContent TemplateContent { get; }

    [ObservableProperty]
    public partial bool IsLoaded { get; set; }
}
