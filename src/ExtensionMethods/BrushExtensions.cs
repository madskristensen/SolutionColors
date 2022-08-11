using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SolutionColors
{
    public static class BrushExtensions
    {
        public static ImageSource GetImageSource(this Brush brush, int size)
        {
            DrawingVisual dVisual = new();
            using (DrawingContext dc = dVisual.RenderOpen())
            {
                dc.DrawRectangle(brush, null, new Rect(0, 0, size, size));
            }
            RenderTargetBitmap targetBitmap = new(size, size, 96, 96, PixelFormats.Default);
            targetBitmap.Render(dVisual);

            return new WriteableBitmap(targetBitmap);
        }
    }
}
