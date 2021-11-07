using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using System.Linq;
using System.Collections.Generic;

namespace FloorTilingOptimization
{
    public static class ImageExporter
    {
        public static float ImageExtraSpaceMultiplier { get; set; } = 1.2f;
        public static Font IndexFont { get; set; } = new Font(SystemFonts.Find("Arial"), 200);

        public static Image<Rgba32> CreateImage(Rectangle bounds, out float xOffset, out float yOffset)
        {
            Image<Rgba32> image = new Image<Rgba32>(
                (int)Math.Ceiling(bounds.Width * ImageExtraSpaceMultiplier),
                (int)Math.Ceiling(bounds.Height * ImageExtraSpaceMultiplier));
            float _xOffset = bounds.Width * (ImageExtraSpaceMultiplier - 1) / 2;
            float _yOffset = bounds.Height * (ImageExtraSpaceMultiplier - 1) / 2;

            image.Mutate(x => x.BackgroundColor(Color.Black));
            image.Mutate(x => x.Draw(Color.White, 2.0f, new RectangleF(
                bounds.X + _xOffset,
                bounds.Y + _yOffset,
                bounds.Width, bounds.Height)));

            xOffset = _xOffset;
            yOffset = _yOffset;
            return image;
        }

        public static void Downscale(this Image<Rgba32> image, int targetHorizontalPixels = 2048)
        {
            double downscaling = (double)targetHorizontalPixels / image.Width;
            image.Mutate(x => x.Resize(targetHorizontalPixels, (int)Math.Round(image.Height * downscaling)));
        }

        public static void AddRects(this Image<Rgba32> image, float xOffset, float yOffset, 
            IEnumerable<PlottableRect> rectangles)
        {
            foreach (var item in rectangles)
            {
                if (item.Area == 0) continue;
                Rectangle r = item.Rect;
                image.Mutate(x => x.Fill(item.Color, new RectangleF(r.X + xOffset, r.Y + yOffset, r.Width, r.Height)));
                var textPoint = Rectangle.Center(r);
                textPoint.Offset((int)xOffset - IndexFont.LineHeight, (int)yOffset - IndexFont.LineHeight / 2);
                image.Mutate(x => x.SetDrawingTransform(GetTextRotationMatrix(textPoint))
                    .DrawText(item.Tag, IndexFont, Color.White, textPoint));
            }
        }
        public static void AddRects(this Image<Rgba32> image, float xOffset, float yOffset,
           IEnumerable<Rectangle> rectangles, ColorPalette c)
        {
            foreach (var item in rectangles)
            {
                if (Algorithms.Area(item) == 0) continue;
                item.Offset((int)xOffset, (int)yOffset);
                image.Mutate(x => x.Fill(c.GetNextColor(), item));
            }
        }

        private static System.Numerics.Matrix3x2 GetTextRotationMatrix(PointF p)
        {
            return Matrix3x2Extensions.CreateRotationDegrees(90, p);
        }
    }
}
