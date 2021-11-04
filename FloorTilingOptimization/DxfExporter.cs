using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using netDxf;
using netDxf.Entities;

namespace FloorTilingOptimization
{
    public static class DxfExporter
    {
        public static void ExportRectangles(Rectangle[] data, string path)
        {
            var doc = new DxfDocument();
            foreach (var item in data)
            {
                doc.AddEntity(RectangleToPolyline(item));
            }
            doc.Save(path);
        }

        public static void ExportSheetsAndMounting(Sheet[] data, Rectangle[] mount, string path)
        {
            var doc = new DxfDocument();
            foreach (var item in data)
            {
                if (item.Area == 0) continue;
                var r = item.ToRectangle();
                doc.AddEntity(RectangleToPolyline(r));
                doc.AddEntity(new Text($"{item.Tag}, t={item.Thickness}", 
                    new Vector3(r.X + r.Width / 2, r.Y + r.Height / 2, 0), r.Height / 10));
            }
            foreach (var item in mount)
            {
                doc.AddEntity(RectangleToPolyline(item));
            }
            doc.Save(path);
        }

        private static Polyline RectangleToPolyline(Rectangle r)
        {
            return new Polyline(new Vector3[] 
            { 
                new Vector3(r.X, r.Y, 0), new Vector3(r.X + r.Width, r.Y, 0),
                new Vector3(r.X + r.Width, r.Y + r.Height, 0), new Vector3(r.X, r.Y + r.Height, 0)
            }, true);
        }
    }
}
