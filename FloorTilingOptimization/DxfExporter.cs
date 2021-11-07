using System;
using System.Collections.Generic;
using System.Text;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using SixLabors.ImageSharp;

namespace FloorTilingOptimization
{
    public static class DxfExporter
    {
        public static double TextScalingFactor { get; set; } = 0.1;

        public static void Export(string file, params IPlottableRectContainer[] data)
        {
            var doc = new DxfDocument();
            foreach (var item in data)
            {
                Layer current = new Layer(item.Name);
                doc.AddRects(item.GetPlottableRects(), current);
            }
            doc.Save(file);
        }

        public static void AddRects(this DxfDocument doc, IEnumerable<PlottableRect> data, Layer layer = null)
        {
            foreach (var item in data)
            {
                if (item.Area == 0) continue;
                var r = item.Rect;
                var color = item.GetAciColor();
                var e = RectangleToPolyline(r, color);
                if (layer != null) e.Layer = layer;
                doc.AddEntity(e);
                var c = Rectangle.Center(r);
                bool o = item.Orientation;
                double th = item.SmallestDimension * TextScalingFactor;
                int toff = (int)(-th / 2);
                c.Offset(o ? toff : (int)th, o ? -(int)th : toff);
                doc.AddEntity(new Text(item.Tag, new Vector3(c.X, c.Y, 0), th)
                {
                    Rotation = o ? 90 : 0,
                    Color = color,
                    Layer = e.Layer
                });
            }
        }

        private static Polyline RectangleToPolyline(Rectangle r, AciColor c = null)
        {
            var p = new Polyline(new Vector3[]
            {
                new Vector3(r.X, r.Y, 0), new Vector3(r.X + r.Width, r.Y, 0),
                new Vector3(r.X + r.Width, r.Y + r.Height, 0), new Vector3(r.X, r.Y + r.Height, 0)
            }, true)
            {
                Color = c ?? AciColor.ByLayer
            };
            return p;
        }
    }
}
