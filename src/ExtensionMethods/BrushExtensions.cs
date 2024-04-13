using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SolutionColors
{
    public static class BrushExtensions
    {
        public static ImageSource GetImageSource(this Brush brush, int size, General options)
        {
            if (options.TaskbarIconMode == IconMode.ColoredSquare)
            {
                return brush.GetColoredSquareSource(size);
            }
            else if (options.TaskbarIconMode == IconMode.CustomIcon)
            {
                return brush.GetCustomLogoSource(options.CustomTaskBarIconPath);
            }
            else
            {
                return brush.GetEmptyImageSource();
            }
        }

        private static ImageSource GetColoredSquareSource(this Brush brush, int size) 
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

        private static ImageSource GetCustomLogoSource(this Brush brush, string filepath)
        {
            Uri uri = new Uri(filepath);

            if (File.Exists(uri.LocalPath))
            {
                BitmapImage logo = new BitmapImage(uri);

                DrawingVisual dVisual = new();
                using (DrawingContext dc = dVisual.RenderOpen())
                {
                    dc.DrawImage(logo, new Rect(0, 0, logo.Width, logo.Height));
                }
                RenderTargetBitmap targetBitmap = new((int)logo.Width, (int)logo.Height, 96, 96, PixelFormats.Default);
                targetBitmap.Render(dVisual);

                return new WriteableBitmap(targetBitmap);
            }

            // Return empty, file does not exist
            return brush.GetEmptyImageSource();
        }

        private static ImageSource GetEmptyImageSource(this Brush brush)
        {
            return new WriteableBitmap(new RenderTargetBitmap(0, 0, 96, 96, PixelFormats.Default));
        }
    }
}
