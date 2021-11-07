using CsvHelper.Configuration.Attributes;
using RectpackSharp;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.Linq;

namespace FloorTilingOptimization
{
    public class Sheet : PlottableRect
    {
        public Sheet(int l, int w, int t, int index, Color c) 
            : this(new Rectangle(0, 0, l, w), t, index, c)
        { }
        public Sheet(Rectangle r, int t, int index, Color c)
            : base(r, c, index)
        {
            Thickness = t;
        }
        public Sheet(Sheet source) : base(source)
        {
            Thickness = source.Thickness;
            IsChild = source.IsChild;
            IsUsed = source.IsUsed;
        }

        public int Thickness { get; }
        public bool IsChild { get; private set; } = false;
        public bool IsUsed { get; private set; } = true;
        public override string Tag { get => $"{Id},t={Thickness}"; }

        public Sheet GetChild(Rectangle cut)
        {
            var r = Algorithms.LargestRectangleChild(Rect, cut);
            if (r == Rectangle.Empty) return null;
            return new Sheet(r, Thickness, Id, Color) 
            { 
                IsChild = true, IsUsed = false 
            };
        }

        public Sheet GetCutSheet(Beam[] beams)
        {
            var r = Algorithms.CutSheet(Rect, beams);
            if (r != Rectangle.Empty)
            {
                IsUsed = true;
                return new Sheet(r, Thickness, Id, Color);
            }
            else
            {
                IsUsed = false;
                return null;
            }
        }

        public void SetOffset(int x, int y)
        {
            Rect.Offset(x - Rect.X, y - Rect.Y);
        }
    }
}
