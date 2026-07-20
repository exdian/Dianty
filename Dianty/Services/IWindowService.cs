using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;

namespace Dianty.Services;

public interface IWindowService
{
    InputNonClientPointerSource? GetInputNonClientPointerSource();
    InputActivationListener? GetInputActivationListener();
    ContentDialog? CreateContentDialog();
}
