using System;
using RectpackSharp;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace FloorTilingOptimization
{
    class Program
    {
        static readonly CsvConfiguration Config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        };

        static void Main(string[] args)
        {
            using TextReader tr = new StreamReader(args[0]);
            using CsvReader cr = new CsvReader(tr, Config);
            List<PackingRectangle> rects = new List<PackingRectangle>();
            while (cr.Read())
            {
                uint w = cr.GetField<uint>(0);
                uint h = cr.GetField<uint>(1);
                if (h > w)
                {
                    uint t = h;
                    h = w;
                    w = t;
                }
                int thickness = cr.GetField<int>(2);
                rects.Add(new PackingRectangle(0, 0, w, h, thickness));
            }
            var r = rects.ToArray();
            RectanglePacker.Pack(r, out PackingRectangle b, PackingHints.FindBest, 10);
            SaveAsImage(r, in b, Path.Combine(Environment.CurrentDirectory, "pack.png"));
        }

        static void SaveAsImage(PackingRectangle[] rectangles, in PackingRectangle bounds, string file)
        {
            using Image<Rgba32> image = new Image<Rgba32>((int)bounds.Width, (int)bounds.Height);
            image.Mutate(x => x.BackgroundColor(Color.Black));

            for (int i = 0; i < rectangles.Length; i++)
            {
                PackingRectangle r = rectangles[i];
                Rgba32 color = FromHue(i / 64f % 1);
                for (int x = 0; x < r.Width; x++)
                    for (int y = 0; y < r.Height; y++)
                        image[x + (int)r.X, y + (int)r.Y] = color;
            }

            image.SaveAsPng(file);
        }

        static Rgba32 FromHue(float hue)
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
