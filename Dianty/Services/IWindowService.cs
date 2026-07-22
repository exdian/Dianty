using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dianty.Services;

public interface IWindowService
{
    ElementTheme ColorTheme { get; set; }

    InputNonClientPointerSource? GetInputNonClientPointerSource();
    InputActivationListener? GetInputActivationListener();
    ContentDialog? CreateContentDialog();
}
