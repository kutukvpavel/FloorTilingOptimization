using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FloorTilingOptimization
{
    public enum RectLocationReference
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public static class Algorithms
    {
        public static int PlacementTolerance { get; set; }

        private static int GetLeftAlignment(Rectangle s, IEnumerable<Rectangle> lastColumn)
        {
            int yMin = s.Y - PlacementTolerance;
            int yMax = s.Bottom + PlacementTolerance;
            int maxRight = 0;
            foreach (var item in lastColumn)
            {
                if (!(yMin > item.Bottom || yMax < item.Top))
                    if (item.Right > maxRight) maxRight = item.Right;
            }
            return maxRight;
        }

        public static double PlaceAndAssessSheets(Stock stock, SupportStructure ss, int[] placementOrder,
            out Stock children, out Stock assessed)
        {
            var _children = new List<Sheet>(stock.Sheets.Length); //Only 1 generation of children
            var _assessed = new List<Sheet>(stock.Sheets.Length);
            int lastBottom = 0;
            int bottomWithTolerance = ss.Bounds.Bottom - 9 * PlacementTolerance;
            List<Rectangle> lastColumn = new List<Rectangle>();
            List<Rectangle> currentColumn = new List<Rectangle>();
            foreach (var item in placementOrder)
            {
                var it = stock.Sheets[item];
                it.SetOffset(GetLeftAlignment(it.Rect, lastColumn) - PlacementTolerance, lastBottom);
                var c = it.GetCutSheet(ss.Beams);
                if (it.IsUsed)
                {
                    _children.Add(it.GetChild(c.Rect));
                    currentColumn.Add(c.Rect);
                    it.Rect.Offset(PlacementTolerance, 0);
                    c.Rect.Offset(PlacementTolerance, 0);
                    _assessed.Add(c);
                    lastBottom = it.Rect.Bottom + PlacementTolerance;
                    if (lastBottom > bottomWithTolerance)
                    {
                        lastColumn.Clear();
                        var t = lastColumn;
                        lastColumn = currentColumn;
                        currentColumn = t;
                        lastBottom = 0;
                    }
                }
            }
            children = new Stock(_children) { Name = "Children" };
            assessed = new Stock(_assessed) { Name = "Assessed" };
            return assessed.TotalArea / ss.BoundedArea - (stock.TotalArea - assessed.TotalArea) / stock.TotalArea;
        }

        public static Rectangle CutSheet(Rectangle s, Beam[] beams)
        {
            Rectangle u = Rectangle.Empty;
            int c = 0;
            foreach (var item in beams)
            {
                if (item.IsWall) continue;
                Rectangle i = Rectangle.Intersect(item.OverlapAccounted, s);
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

        public static Rectangle FromReferencedTo(RectLocationReference reference,
            int x, int y, int length, int width)
        {
            switch (reference)
            {
                case RectLocationReference.BottomLeft:
                case RectLocationReference.TopLeft:
                    break;
                case RectLocationReference.BottomRight:
                case RectLocationReference.TopRight:
                    x -= length;
                    break;
                default: throw new ArgumentException();
            }
            switch (reference)
            {
                case RectLocationReference.TopRight:
                case RectLocationReference.TopLeft:
                    break;
                case RectLocationReference.BottomLeft:
                case RectLocationReference.BottomRight:
                    y -= width;
                    break;
                default: throw new ArgumentException();
            }
            return new Rectangle(x, y, length, width);
        }

        public static Rectangle LargestRectangleChild(Rectangle stock, Rectangle cutout)
        {
            var points = RectanglePoints(cutout);
            var sc = Rectangle.Center(stock);
            var cc = Rectangle.Center(cutout);
            var closestToCenter = ClosestTo(points, sc);
            //Determine location of possible first pieces to be cut off
            bool upperHalf = sc.Y < cc.Y;
            bool leftHalf = sc.X < cc.X;
            //Construct resulting pieces
            var verticalCut = leftHalf ? Rectangle.FromLTRB(stock.X, stock.Top, closestToCenter.X, stock.Bottom)
                : Rectangle.FromLTRB(closestToCenter.X, stock.Top, stock.Right, stock.Bottom);
            var horizontalCut = upperHalf ? Rectangle.FromLTRB(stock.X, stock.Top, stock.Right, closestToCenter.Y)
                : Rectangle.FromLTRB(stock.X, closestToCenter.Y, stock.Right, stock.Bottom);
            int va = Area(verticalCut);
            int ha = Area(horizontalCut);
            if (ha > va)
            {
                if (ha <= 0) return Rectangle.Empty;
                return horizontalCut;
            }
            else
            {
                if (va <= 0) return Rectangle.Empty;
                return verticalCut;
            }
        }

        public static Rectangle GetBounds(IEnumerable<Rectangle> rects)
        {
            var res = new Rectangle();
            foreach (var item in rects)
            {
                res = Rectangle.Union(res, item);
            }
            return res;
        }

        public static int Area(Rectangle r)
        {
            return r.Width * r.Height;
        }

        public static void SortByDistance(Point[] points, Point reference)
        {
            Array.Sort(points, (x, y) => Distance(x, reference) - Distance(y, reference));
        }

        public static int Distance(Point x, Point y)
        {
            int dx = x.X - y.X;
            int dy = x.Y - y.Y;
            return dx * dx + dy * dy;
        }

        public static Point ClosestTo(Point[] arr, Point r)
        {
            int temp = Distance(arr[0], r);
            Point res = arr[0];
            for (int i = 1; i < arr.Length; i++)
            {
                var d = Distance(arr[i], r);
                if (d < temp)
                {
                    temp = d;
                    res = arr[i];
                }
            }
            return res;
        }

        public static Point[] RectanglePoints(Rectangle r)
        {
            return new Point[] { new Point(r.Left, r.Top), new Point(r.Left, r.Bottom), 
                new Point(r.Right, r.Bottom), new Point(r.Right, r.Top) };
        }
    }
}
