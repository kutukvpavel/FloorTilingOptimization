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
            _LastAssessed.Clear();
            _LastAssessed.Capacity = s.Length;
            int lastColumnBoundary = 0;
            int currentColumnBoundary = 0;
            int lastBottom = 0;
            double coveredArea = 0;
            double totalSheetArea = 0;
            foreach (var item in placementOrder)
            {
                var it = s[item];
                it.X = lastColumnBoundary;
                it.Y = lastBottom;
                var c = GetCutRect(it);
                coveredArea += GetRectArea(c);
                _LastAssessed.Add(c);
                totalSheetArea += it.Area;
                lastBottom += c.IsEmpty ? 0 : it.Width;
                if (c.Right > currentColumnBoundary) currentColumnBoundary = c.Right;
                if (lastBottom > Bounds.Bottom)
                {
                    lastColumnBoundary = currentColumnBoundary - Tolerance;
                    lastBottom = 0;
                }
            }
            double score = coveredArea * coveredArea / (totalSheetArea * BoundedArea);
            return new Tuple<double, double>(score, coveredArea);
        }

        #region Private

        private Rectangle[] _MountingZones;
        private List<Rectangle> _LastAssessed = new List<Rectangle>();

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

        #endregion
    }
}
