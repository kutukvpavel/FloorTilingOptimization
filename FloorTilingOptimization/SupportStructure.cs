using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace FloorTilingOptimization
{
    public class SupportStructure
    {
        public static int Tolerance { get; set; } = 5;

        public SupportStructure(IEnumerable<Beam> data)
        {
            var a = new List<Rectangle>();
            foreach (var item in data)
            {
                int x = item.X;
                switch (item.Reference)
                {
                    case BeamLocationReference.BottomLeft:
                    case BeamLocationReference.TopLeft:
                        break;
                    case BeamLocationReference.BottomRight:
                    case BeamLocationReference.TopRight:
                        x -= item.Length;
                        break;
                    default: throw new ArgumentException();
                }
                int y = item.Y;
                switch (item.Reference)
                {
                    case BeamLocationReference.TopRight:
                    case BeamLocationReference.TopLeft:
                        break;
                    case BeamLocationReference.BottomLeft:
                    case BeamLocationReference.BottomRight:
                        y -= item.Width;
                        break;
                    default: throw new ArgumentException();
                }
                x += item.RequiredOverlap;
                y += item.RequiredOverlap;
                int l = item.Length - item.RequiredOverlap;
                int w = item.Width - item.RequiredOverlap;
                x = CheckPositiveWithTolerance(x);
                y = CheckPositiveWithTolerance(y);
                l = CheckPositiveWithTolerance(l);
                w = CheckPositiveWithTolerance(w);
                var r = new Rectangle(x, y, l, w);
                a.Add(r);
                Bounds = Rectangle.Union(Bounds, r);
            }
            BoundedArea = GetRectArea(Bounds);
            Bounds.Inflate(Tolerance, Tolerance);
            _MountingZones = a.ToArray();
        }

        public int BoundedArea { get; }
        public Rectangle Bounds { get; } = new Rectangle();

        public void SaveStructureImage()
        {
            ImagingApi.SaveRectanglesAsImage(_MountingZones, Bounds, 
                ImagingApi.CreateFilePathInCurrentDir("structure.png"));
        }

        public void SaveLastAssessedImage()
        {
            ImagingApi.SaveRectanglesAsImage(_LastAssessed.ToArray(), Bounds,
                ImagingApi.CreateFilePathInCurrentDir("last_assessed.png"));
        }

        public Sheet[] GetLastChildren()
        {
            return _Children.ToArray();
        }

        public Rectangle GetCutRect(Sheet s)
        {
            Rectangle r = s.ToRectangle();
            Rectangle u = Rectangle.Empty;
            int c = 0;
            foreach (var item in _MountingZones)
            {
                Rectangle i = Rectangle.Intersect(item, r);
                if (!i.IsEmpty)
                {
                    if (c++ == 0)
                    {
                        u = i;
                    }
                    else
                    {
                        u = Rectangle.Union(u, i);
                    }
                }
            }
            if (c == 1) return Rectangle.Empty;
            return u;
        }

        public Tuple<double, double> PlaceAndAssess(Sheet[] s, int[] placementOrder)
        {
            _Children.Clear();
            _LastAssessed.Clear();
            _LastAssessed.Capacity = s.Length;
            int lastBottom = 0;
            double coveredArea = 0;
            double totalSheetArea = 0;
            int bottomWithTolerance = Bounds.Bottom - 9 * Tolerance;
            List<Rectangle> lastColumn = new List<Rectangle>();
            List<Rectangle> currentColumn = new List<Rectangle>();
            foreach (var item in placementOrder)
            {
                var it = s[item];
                it.Y = lastBottom;
                it.X = GetLeftAlignment(it, lastColumn) - Tolerance;
                var c = GetCutRect(it);
                it.IsUsed = !c.IsEmpty;
                if (it.IsUsed)
                {
                    _Children.Add(new Sheet(it.Length - c.Width, it.Width - c.Width, it.Thickness) 
                    { 
                        Tag = it.Tag, IsUsed = false, IsChild = true
                    });
                    currentColumn.Add(c);
                    coveredArea += GetRectArea(c);
                    it.X += Tolerance;
                    c.Offset(Tolerance, 0);
                    _LastAssessed.Add(c);
                    totalSheetArea += it.Area;
                    lastBottom += it.Width + Tolerance;
                    if (lastBottom > bottomWithTolerance)
                    {
                        lastColumn = currentColumn;
                        currentColumn = new List<Rectangle>();
                        lastBottom = 0;
                    }
                }
            }
            double score = coveredArea / BoundedArea - (totalSheetArea - coveredArea) / totalSheetArea;
            return new Tuple<double, double>(score, coveredArea);
        }

        #region Private

        private Rectangle[] _MountingZones;
        private List<Rectangle> _LastAssessed = new List<Rectangle>();
        private List<Sheet> _Children = new List<Sheet>();

        private static int CheckPositiveWithTolerance(int i)
        {
            if (i < 0)
            {
                if (i > -Tolerance)
                {
                    return 0;
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            else
            {
                return i;
            }
        }

        private static int GetRectArea(Rectangle r)
        {
            return r.Width * r.Height;
        }

        private static int GetLeftAlignment(Sheet s, List<Rectangle> lastColumn)
        {
            int yMin = s.Y - Tolerance;
            int yMax = s.Bottom + Tolerance;
            int maxRight = 0;
            foreach (var item in lastColumn)
            {
                if (!(yMin > item.Bottom || yMax < item.Top))
                    if (item.Right > maxRight) maxRight = item.Right;
            }
            return maxRight;
        }

        #endregion
    }
}
