using Microsoft.UI.Input;

namespace Dianty.Services;

public interface IWindowService
{
    InputNonClientPointerSource? GetInputNonClientPointerSource();
    InputActivationListener? GetInputActivationListener();
}
