using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dianty.Localization;

[JsonSerializable(typeof(AppText))]
internal partial class AppTextJsonContext : JsonSerializerContext
{
    private static AppTextJsonContext? _unsafe;

    public static AppTextJsonContext Unsafe
    {
        get
        {
            if (_unsafe is null)
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                _unsafe = new AppTextJsonContext(options);
            }
            return _unsafe;
        }
    }
}
