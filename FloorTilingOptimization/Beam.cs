using SixLabors.ImageSharp;
using System;

namespace FloorTilingOptimization
{
    public class Beam : PlottableRect
    {
        public static int Tolerance { get; set; } = 5;

        public Beam(int x, int y, int l, int w, int overlap, int id, Color c, 
            RectLocationReference reference = RectLocationReference.TopLeft)
            : base(Algorithms.FromReferencedTo(reference, x ,y, l, w), c, id)
        {
            if (Rect.Width > Rect.Height)
            {
                int h = CheckPositiveWithTolerance(Rect.Height - 2 * overlap);
                OverlapAccounted = new Rectangle(Rect.X, Rect.Y + (Rect.Height - h) / 2, Rect.Width, h);
            }
            else
            {
                int ww = CheckPositiveWithTolerance(Rect.Width - 2 * overlap);
                OverlapAccounted = new Rectangle(Rect.X + (Rect.Width - ww) / 2, Rect.Y, ww, Rect.Height);
            }
        }

        public Rectangle OverlapAccounted { get; }
        public bool IsWall { get; set; } = false;

        private static int CheckPositiveWithTolerance(int i)
        {
            if (i < 0)
            {
                if (i > -Tolerance)
                {
                    return Tolerance;
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            else if (i < Tolerance)
            {
                return Tolerance;
            }
            else
            {
                return i;
            }
        }
    }
}
