using Dianty.ViewModels;
using DungeonToolkit.Coyote;

namespace Dianty.Services;

public interface ICoyoteItemBaseFactory
{
    CoyoteItemBase Create(IDeviceInfo deviceInfo);
}
