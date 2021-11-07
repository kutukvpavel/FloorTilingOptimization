using netDxf;
using RectpackSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FloorTilingOptimization
{
    public class PlottableRect
    {
        public PlottableRect(Rectangle r, Color c, int id)
        {
            Rect = r;
            Color = c;
            Id = id;
            Area = Algorithms.Area(r);
        }
        public PlottableRect(PlottableRect source) : this(source.Rect, source.Color, source.Id)
        { }

        public Rectangle Rect { get; protected set; }
        public Color Color { get; protected set; }
        public int Id { get; protected set; }
        public virtual string Tag { get; set; }
        public int Area { get; protected set; }
        public int LargestDimension { get => Orientation ? Rect.Height : Rect.Width; }
        public int SmallestDimension { get => Orientation ? Rect.Width : Rect.Height; }
        public bool Orientation { get => Rect.Height > Rect.Width; } //Vertical

        public void Orient(bool vertically = false)
        {
            if (Rect.Width < Rect.Height != vertically)
            {
                Rect = new Rectangle(Rect.X, Rect.Y, Rect.Height, Rect.Width);
            }
        }

        public PackingRectangle GetPackingRectangle()
        {
            return new PackingRectangle((uint)Rect.X, (uint)Rect.Y, (uint)Rect.Width, (uint)Rect.Height, Id);
        }

        public AciColor GetAciColor()
        {
            var rgb = this.Color.ToPixel<Rgb24>();
            return new AciColor(rgb.R, rgb.G, rgb.B);
        }
    }
}
