using CsvHelper.Configuration.Attributes;
using System.Drawing;

namespace FloorTilingOptimization
{
    public enum BeamLocationReference
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public class Beam
    {
        public Beam()
        {

        }
        public Beam(int x, int y, int l, int w, int overlap, 
            BeamLocationReference reference = BeamLocationReference.TopLeft)
        {
            X = x;
            Y = y;
            Length = l;
            Width = w;
            RequiredOverlap = overlap;
            Reference = reference;

        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Length { get; set; }
        public int Width { get; set; }
        public int RequiredOverlap { get; set; }
        [Optional]
        public BeamLocationReference Reference { get; set; } = BeamLocationReference.TopLeft;
        [Optional]
        public bool IsWall { get; set; } = false;
        [Ignore]
        public Rectangle Rect { get; set; }
    }
}
