﻿using System;
using System.Collections.Generic;
using Path = System.IO.Path;
using System.Text;
using Rectangle = System.Drawing.Rectangle;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace FloorTilingOptimization
{
    public static class ImagingApi
    {
        public static float ImageExtraSpaceMultiplier { get; set; } = 1.2f;

        public static string CreateFilePathInCurrentDir(string name)
        {
            return Path.Combine(Environment.CurrentDirectory, name);
        }

        public static void SaveRectanglesAsImage(Rectangle[] rectangles, Rectangle bounds, string file)
        {
            using Image<Rgba32> image = new Image<Rgba32>(
                (int)Math.Ceiling(bounds.Width * ImageExtraSpaceMultiplier), 
                (int)Math.Ceiling(bounds.Height * ImageExtraSpaceMultiplier));

            float xOffset = bounds.Width * (ImageExtraSpaceMultiplier - 1) / 2;
            float yOffset = bounds.Height * (ImageExtraSpaceMultiplier - 1) / 2;

            image.Mutate(x => x.BackgroundColor(Color.Black));
            image.Mutate(x => x.Draw(Color.White, 2.0f, new RectangleF(
                bounds.X + xOffset,
                bounds.Y + yOffset, 
                bounds.Width, bounds.Height)));

            for (int i = 0; i < rectangles.Length; i++)
            {
                Rectangle r = rectangles[i];
                Rgba32 color = FromHue(i / 64f % 1, 0.5f);
                image.Mutate(x => x.Fill(color, new RectangleF(r.X + xOffset, r.Y + yOffset, r.Width, r.Height)));
            }

            image.SaveAsPng(file);
        }

        public static Rgba32 FromHue(float hue, float a = 1)
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

            return new Rgba32(r, g, b, a);
        }
    }
}
