using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using static Dianty.Models.GameManager;

namespace Dianty.ViewModels;

public partial class GamesViewModel : ObservableObject
{
    public GamesViewModel()
    {
        StrengthModes = new Dictionary<Mode, string>
        {
            [Mode.Max] = "取最大值",
            [Mode.Add] = "叠加强度"
        }.ToArray();

        CurrentGamesStrengthMode = StrengthModes.First();
        CurrentGtaVcStrengthMode = StrengthModes.First();
    }

    public KeyValuePair<Mode, string>[] StrengthModes { get; }

    [ObservableProperty]
    public partial KeyValuePair<Mode, string> CurrentGamesStrengthMode { get; set; }

    [ObservableProperty]
    public partial int OutputStrength { get; private set; }

    [ObservableProperty]
    public partial int EnableGameCount { get; private set; }

    [ObservableProperty]
    public partial KeyValuePair<Mode, string> CurrentGtaVcStrengthMode { get; set; }
}
