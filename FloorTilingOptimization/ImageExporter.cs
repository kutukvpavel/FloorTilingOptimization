using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;

namespace FloorTilingOptimization
{
    public static class ImageExporter
    {
        public static float ImageExtraSpaceMultiplier { get; set; } = 1.2f;
        public static Font TagFont { get; set; } = new Font(SystemFonts.Find("Arial"), 200);

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
            GC.Collect();
        }

        public static void AddRects(this Image<Rgba32> image, float xOffset, float yOffset, 
            IEnumerable<PlottableRect> rectangles)
        {
            foreach (var item in rectangles)
            {
                if (item.Area == 0) continue;
                RectangleF r = item.Rect;
                r.Offset(xOffset, yOffset);
                image.Mutate(x => x.Fill(item.Color, r));
                if (item.Tag == null) continue;
                bool o = item.Orientation;
                var textPoint = RectangleF.Center(r);
                var tMeasure = TextMeasurer.Measure(item.Tag, new RendererOptions(TagFont));
                float tLenOffset = -tMeasure.Width / 2;
                float tHOffset = -tMeasure.Height / 2;
                textPoint.Offset(o ? tHOffset : tLenOffset, o ? -tLenOffset : tHOffset);
                image.Mutate(x => 
                {
                    var rdo = o ? new DrawingOptions { Transform = GetTextRotationMatrix(textPoint) } 
                        : new DrawingOptions();
                    x.DrawText(rdo, item.Tag, TagFont, Color.White, textPoint);
                    //x.Draw(rdo, Color.White, 5.0f, new RectangleF(textPoint, new SizeF(tMeasure.Width, tMeasure.Height)));
                });
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
            return Matrix3x2Extensions.CreateRotationDegrees(-90, p);
        }
    }
}
