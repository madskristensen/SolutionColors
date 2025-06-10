using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static SolutionColors.ColorHelper;

namespace SolutionColors
{
    public static class BrushExtensions
    {
        public static ImageSource GetImageSource(this Brush brush, int size)
        {
            string iconName = GetFileName(false);
            Uri uri = new Uri(iconName);

            if (File.Exists(uri.LocalPath))
                return brush.GetCustomLogoSource(iconName);
            else
                return brush.GetColoredSquareSource(size);
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
                // It's important to load the BitmapImage via a MemoryStream rather than giving the class the URI as BitmapImage can be slow to release the file handle
                BitmapImage logo = new BitmapImage();
                logo.BeginInit();
                logo.StreamSource = new MemoryStream(File.ReadAllBytes(uri.LocalPath));
                logo.EndInit();

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
