using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Dianty.Utils;

internal class SettingsCardHelper
{
    public static void StretchContent(SettingsCard settingsCard)
    {
        // 使内容拉伸铺满
        if (VisualTreeHelperExtension.FindChildByName(settingsCard, "PART_RootGrid") is not Grid grid)
            return;
        if (VisualTreeHelperExtension.FindChildByName(grid, "PART_ContentPresenter") is not ContentPresenter contentPresenter)
            return;
        settingsCard.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        grid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Auto);
        grid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
        contentPresenter.HorizontalAlignment = HorizontalAlignment.Stretch;
        Grid.SetColumnSpan(contentPresenter, 2);
        var visualStateGroups = VisualStateManager.GetVisualStateGroups(grid);
        foreach (var visualStateGroup in visualStateGroups)
        {
            if (visualStateGroup.Name == "ContentAlignmentStates")
            {
                foreach (var state in visualStateGroup.States)
                {
                    if (state.Name == "RightWrapped")
                    {
                        state.Setters.RemoveAt(3);
                    }
                    else if (state.Name == "RightWrappedNoIcon")
                    {
                        state.Setters.RemoveAt(4);
                    }
                }
                var currentState = visualStateGroup.CurrentState;
                if (currentState is null)
                {
                    VisualStateManager.GoToState(settingsCard, "Right", false);
                }
                else
                {
                    foreach (var state in visualStateGroup.States)
                    {
                        if (state != currentState)
                        {
                            VisualStateManager.GoToState(settingsCard, state.Name, false);
                            VisualStateManager.GoToState(settingsCard, currentState.Name, false);
                            break;
                        }
                    }
                }
                break;
            }
        }
    }
}
