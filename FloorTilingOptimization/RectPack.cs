using System;
using RectpackSharp;
using CsvHelper;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace FloorTilingOptimization
{
    public static class RectPack
    {
        public static void PackAndSaveImage(Sheet[] sheets, float density)
        {
            PackingRectangle[] r = sheets.Select(x => x.GetPackingRectangle()).ToArray();
            RectanglePacker.Pack(r, out PackingRectangle b, PackingHints.FindBest, density);
            SaveAsImage(r, in b, ImageExporter.CreateFilePathInCurrentDir("pack.png"));
        }

        static void SaveAsImage(PackingRectangle[] rectangles, in PackingRectangle bounds, string file)
        {
            ImageExporter.SaveRectanglesAsImage(rectangles.Select(x => ToRectangle(x)).ToArray(), ToRectangle(bounds), file);
        }

        static Rectangle ToRectangle(PackingRectangle r)
        {
            return new Rectangle((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);
        }
    }
}
