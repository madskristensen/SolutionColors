using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SolutionColors
{
    public static class BrushExtensions
    {
        public static async Task<ImageSource> GetImageSourceAsync(this Brush brush, int size)
        {
            string iconName = await ColorHelper.GetFileNameAsync(isColor: false);

            if (!string.IsNullOrEmpty(iconName) && File.Exists(iconName))
            {
                return brush.GetCustomLogoSource(iconName);
            }

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
            if (!File.Exists(filepath))
            {
                return brush.GetEmptyImageSource();
            }

            // Load the BitmapImage via a MemoryStream rather than giving the class the URI
            // as BitmapImage can be slow to release the file handle.
            // Read bytes first, then create stream to ensure proper disposal.
            byte[] imageBytes = File.ReadAllBytes(filepath);
            
            BitmapImage logo = new();
            logo.BeginInit();
            logo.CacheOption = BitmapCacheOption.OnLoad;
            logo.StreamSource = new MemoryStream(imageBytes);
            logo.EndInit();
            logo.Freeze(); // Freeze to make it cross-thread accessible and release resources

            DrawingVisual dVisual = new();
            using (DrawingContext dc = dVisual.RenderOpen())
            {
                dc.DrawImage(logo, new Rect(0, 0, logo.Width, logo.Height));
            }
            RenderTargetBitmap targetBitmap = new((int)logo.Width, (int)logo.Height, 96, 96, PixelFormats.Default);
            targetBitmap.Render(dVisual);

            return new WriteableBitmap(targetBitmap);
        }

        private static ImageSource GetEmptyImageSource(this Brush brush)
        {
            return new WriteableBitmap(new RenderTargetBitmap(0, 0, 96, 96, PixelFormats.Default));
        }
    }
}
