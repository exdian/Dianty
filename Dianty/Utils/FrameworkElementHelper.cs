using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.Graphics;

namespace Dianty.Utils;

internal class FrameworkElementHelper
{
    public static RectInt32 GetBounds(FrameworkElement element)
    {
        GeneralTransform transform = element.TransformToVisual(null);

        double width = element.ActualWidth;
        double height = element.ActualHeight;

        Rect bounds = transform.TransformBounds(new Rect(0, 0, width, height));

        double scale = element.XamlRoot.RasterizationScale;

        return new RectInt32
        {
            X = (int)(bounds.X * scale),
            Y = (int)(bounds.Y * scale),
            Width = (int)(bounds.Width * scale),
            Height = (int)(bounds.Height * scale)
        };
    }
}
