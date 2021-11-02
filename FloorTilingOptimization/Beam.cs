using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace FloorTilingOptimization
{
    public enum BeamLocationReference
    {
        BottomLeft,
        TopLeft,
        BottomRight,
        TopRight
    }

    public class Beam
    {
        public int Length { get; set; }
        public int Width { get; set; }
        public Point Location { get; set; }
        public BeamLocationReference Reference { get; set; }
        public int RequiredOverlap { get; set; }
    }
}
