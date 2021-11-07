using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SixLabors.ImageSharp;
using CsvHelper;

namespace FloorTilingOptimization
{
    public class SupportStructure : ICsvAware, IPlottableRectContainer
    {
        public SupportStructure() { Beams = new Beam[0]; }
        public SupportStructure(Beam[] data)
        {
            Beams = data;
            Init();
        }

        public Beam[] Beams { get; private set; }
        public int BoundedArea { get; private set; }
        public Rectangle Bounds { get; private set; }
        public string Name { get; set; }

        public void ReadCsv(CsvReader r, ColorPalette c)
        {
            var beamColor = c.GetNextColor();
            var wallColor = c.GetNextColor();
            Beams = r.GetRecords<CsvType>().Select((x, i) =>
            {
                bool wall = x.IsWall != 0;
                return new Beam(x.X, x.Y, x.Length, x.Width, x.RequiredOverlap, i,
                wall ? wallColor : beamColor, x.Reference)
                {
                    IsWall = wall
                };
            }).ToArray();
            Init();
        }

        public void WriteExampleCsv(CsvWriter w)
        {
            w.WriteHeader<CsvType>();
            w.NextRecord();
        }

        public PlottableRect[] GetPlottableRects()
        {
            return Beams;
        }

        private void Init()
        {
            Bounds = Algorithms.GetBounds(Beams.Select(x => x.Rect));
            BoundedArea = Algorithms.Area(Bounds);
        }

        private class CsvType
        {
            public CsvType() { }
            public int X { get; set; }
            public int Y { get; set; }
            public int Length { get; set; }
            public int Width { get; set; }
            public int RequiredOverlap { get; set; }
            public RectLocationReference Reference { get; set; }
            public int IsWall { get; set; }
        }
    }
}
