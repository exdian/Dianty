using Dianty.EventArguments;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Dianty.Services;

public interface IWindowService : IWindowInputService, IWindowThemeService, IWindowContentService;

public interface IWindowInputService
{
    event EventHandler<LetterKeyDownWithoutTextInputEventArgs>? LetterKeyDownWithoutTextInput;

    InputNonClientPointerSource? GetInputNonClientPointerSource();
    InputActivationListener? GetInputActivationListener();
}

public interface IWindowThemeService
{
    ElementTheme ColorTheme { get; set; }
}

public interface IWindowContentService
{
    ContentDialog? CreateContentDialog();
}
