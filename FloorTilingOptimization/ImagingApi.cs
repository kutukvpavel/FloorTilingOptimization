using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Rectangle = System.Drawing.Rectangle;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace FloorTilingOptimization
{
    public static class ImagingApi
    {
        public static string CreateFilePathInCurrentDir(string name)
        {
            return Path.Combine(Environment.CurrentDirectory, name);
        }

        public static void SaveRectanglesAsImage(Rectangle[] rectangles, in Rectangle bounds, string file)
        {
            using Image<Rgba32> image = new Image<Rgba32>(bounds.Width, bounds.Height);
            image.Mutate(x => x.BackgroundColor(Color.Black));

            int xOffset = bounds.X < 0 ? -bounds.X : 0;
            int yOffset = bounds.Y < 0 ? -bounds.Y : 0;
            for (int i = 0; i < rectangles.Length; i++)
            {
                Rectangle r = rectangles[i];
                Rgba32 color = FromHue(i / 64f % 1);
                for (int x = 0; x < r.Width; x++)
                {
                    for (int y = 0; y < r.Height; y++)
                    {
                        int xx = x + r.X + xOffset;
                        int yy = y + r.Y + yOffset;
                        if (xx < image.Width && yy < image.Height)
                        {
                            image[xx, yy] = color;
                        }
                    }
                }
            }

            image.SaveAsPng(file);
        }

        public static Rgba32 FromHue(float hue)
        {
            hue *= 360.0f;

            float h = hue / 60.0f;
            float x = (1.0f - Math.Abs((h % 2.0f) - 1.0f));

            float r, g, b;
            if (h >= 0.0f && h < 1.0f)
            {
                r = 1;
                g = x;
                b = 0.0f;
            }
            else if (h >= 1.0f && h < 2.0f)
            {
                r = x;
                g = 1;
                b = 0.0f;
            }
            else if (h >= 2.0f && h < 3.0f)
            {
                r = 0.0f;
                g = 1;
                b = x;
            }
            else if (h >= 3.0f && h < 4.0f)
            {
                r = 0.0f;
                g = x;
                b = 1;
            }
            else if (h >= 4.0f && h < 5.0f)
            {
                r = x;
                g = 0.0f;
                b = 1;
            }
            else if (h >= 5.0f && h < 6.0f)
            {
                r = 1;
                g = 0.0f;
                b = x;
            }
            else
            {
                r = 0.0f;
                g = 0.0f;
                b = 0.0f;
            }

            return new Rgba32(r, g, b);
        }
    }
}
