using RectpackSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;

namespace FloorTilingOptimization
{
    public static class RectPackProvider
    {
        public static void PackAndSaveImage(Sheet[] sheets, float density)
        {
            PackingRectangle[] r = sheets.Select(x => x.GetPackingRectangle()).ToArray();
            RectanglePacker.Pack(r, out PackingRectangle b, PackingHints.FindBest, density);
            SaveAsImage(r, in b, Program.CreateFilePathInCurrentDir("pack.png"), new ColorPalette());
        }

        static void SaveAsImage(PackingRectangle[] rectangles, in PackingRectangle bounds, string file, ColorPalette c)
        {
            using Image<Rgba32> image =
                ImageExporter.CreateImage(ToRectangle(in bounds), out float xOffset, out float yOffset);
            image.AddRects(xOffset, yOffset, rectangles.Select(x => ToRectangle(x)), c);
            image.Downscale();
            image.Save(file);
        }

        static Rectangle ToRectangle(in PackingRectangle r)
        {
            return new Rectangle((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);
        }
    }
}
