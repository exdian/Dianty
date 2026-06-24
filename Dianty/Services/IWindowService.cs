using Microsoft.UI.Input;

namespace Dianty.Services;

internal interface IWindowService
{
    InputNonClientPointerSource GetInputNonClientPointerSource();
}
