using DungeonToolkit.Coyote;
using QRCoder;

namespace Dianty.Utils;

internal class CoyoteHelper
{
    public static string CreateQrCodeSvgPathString(CoyoteWS coyote)
    {
        var clientId = coyote.ClientId;
        using QRCodeGenerator qrGenerator = new();
        using QRCodeData qrCodeData = qrGenerator.CreateQrCode(
            "https://www.dungeon-lab.com/app-download.php#DGLAB-SOCKET#" +
            $"wss://ws.dungeon-lab.cn/{clientId}", QRCodeGenerator.ECCLevel.L);
        using SvgQRCode svgQrCode = new(qrCodeData);
        return svgQrCode.GetSvgPath(needMargin: false);
    }
}
