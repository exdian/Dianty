using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dianty.Localization;

[JsonSerializable(typeof(AppText))]
internal sealed partial class AppTextJsonContext : JsonSerializerContext
{
    private static AppTextJsonContext? s_Unsafe;

    public static AppTextJsonContext Unsafe
    {
        get
        {
            if (s_Unsafe is null)
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                s_Unsafe = new AppTextJsonContext(options);
            }
            return s_Unsafe;
        }
    }
}
