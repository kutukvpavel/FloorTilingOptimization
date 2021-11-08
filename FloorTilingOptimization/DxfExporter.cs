using netDxf;
using netDxf.Entities;
using netDxf.Objects;
using netDxf.Tables;
using SixLabors.ImageSharp;
using System.Collections.Generic;

namespace FloorTilingOptimization
{
    public static class DxfExporter
    {
        public static float TextScalingFactor { get; set; } = 0.1f;

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
                var rectEntity = RectangleToPolyline(r, color);
                if (layer != null) rectEntity.Layer = layer;
                //string tag = item.GetDxfSafeTag();
                if (item.Tag != null)
                {
                    Group g = new Group($"{rectEntity.Layer.Name} - {item.Id}");
                    g.Entities.Add(rectEntity);
                    var c = RectangleF.Center(r);
                    bool o = item.Orientation;
                    float th = item.SmallestDimension * TextScalingFactor;
                    float tLenOffset = -item.Tag.Length * th / 3;
                    float tHOffset = -th / 2;
                    c.Offset(o ? -tHOffset : tLenOffset, o ? tLenOffset : tHOffset);
                    g.Entities.Add(new Text(item.Tag, new Vector3(c.X, c.Y, 0), th)
                    {
                        Rotation = o ? 90 : 0,
                        Color = color,
                        Layer = rectEntity.Layer
                    });
                    doc.Groups.Add(g);
                }
                else
                {
                    doc.AddEntity(rectEntity);
                }
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
