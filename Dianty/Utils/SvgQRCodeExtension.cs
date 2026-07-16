using QRCoder;
using System.IO;
using System.Xml;

namespace Dianty.Utils;

public static class SvgQRCodeExtension
{
    extension(SvgQRCode svgQRCode)
    {
        public string GetSvgPath(bool needMargin = false)
        {
            var svgString = svgQRCode.GetGraphic(1, darkColorHex: "#000", lightColorHex: "#FFF", drawQuietZones: needMargin);
            using StringReader stringReader = new(svgString);
            using XmlReader reader = XmlReader.Create(stringReader);
            string? path = string.Empty;
            string border = string.Empty;

            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element)
                    continue;

                if (reader.Name == "path")
                {
                    path = reader.GetAttribute("d");
                }
                else if (needMargin && reader.Name == "rect")
                {
                    var x = reader.GetAttribute("x");
                    var y = reader.GetAttribute("y");
                    var width = reader.GetAttribute("width");
                    var height = reader.GetAttribute("height");
                    border = $"M{x} {y}v{height}h{width}V{y}Z";
                }
            }
            return border + path;
        }
    }
}
