using CsvHelper.Configuration.Attributes;
using RectpackSharp;
using System.Collections.Generic;
using System.Drawing;

namespace FloorTilingOptimization
{
    public class Sheet
    {
        private int _InitialArea = -1;

        public Sheet()
        {

        }
        public Sheet(int l, int w, int t)
        {
            Width = w;
            Length = l;
            Thickness = t;
        }

        [Optional]
        public int X { get; set; } = 0;
        [Optional]
        public int Y { get; set; } = 0;
        public int Length { get; set; }
        public int Width { get; set; }
        public int Thickness { get; set; }
        [Ignore]
        public int Tag { get; set; }
        [Ignore]
        public int Area { get => Length * Width; }
        [Ignore]
        public bool IsChild { get; set; } = false;
        [Ignore]
        public int Bottom { get => Y + Width; }
        [Ignore]
        public int Right { get => X + Length; }

        public void FlipSides()
        {
            int t = Length;
            Length = Width;
            Width = t;
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle(X, Y, Length, Width);
        }

        public PackingRectangle ToPackingRectangle()
        {
            return new PackingRectangle((uint)X, (uint)Y, (uint)Length, (uint)Width, Thickness);
        }

        public static void RotateAndTag(IEnumerable<Sheet> sheets)
        {
            int tag = 0;
            foreach (var item in sheets)
            {
                if (item.Length < item.Width) item.FlipSides();
                item.Tag = tag++;
            }
        }
    }
}
