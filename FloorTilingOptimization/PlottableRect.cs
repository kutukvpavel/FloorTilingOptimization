using netDxf;
using RectpackSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using System.Text;

namespace FloorTilingOptimization
{
    public class PlottableRect
    {
        private Rectangle _Rect;

        public static char DxfInvalidCharacterReplacement { get; set; } = ' ';

        public PlottableRect(Rectangle r, Color c, int id)
        {
            Rect = r;
            Color = c;
            Id = id;
            Area = Algorithms.Area(r);
        }
        public PlottableRect(PlottableRect source) : this(source.Rect, source.Color, source.Id)
        { }

        public Rectangle Rect { get => _Rect; //This accessor copies the Rectangle struct isolating _Rect
            protected set
            {
                _Rect = value;
            }
        }
        public Color Color { get; protected set; }
        public int Id { get; protected set; }
        public virtual string Tag { get; set; }
        public int Area { get; protected set; }
        public int LargestDimension { get => Orientation ? _Rect.Height : _Rect.Width; }
        public int SmallestDimension { get => Orientation ? _Rect.Width : _Rect.Height; }
        public bool Orientation { get => _Rect.Height > _Rect.Width; } //Vertical

        public void Orient(bool vertically = false)
        {
            if (_Rect.Width < _Rect.Height != vertically)
            {
                //Rect = new Rectangle(Rect.X, Rect.Y, Rect.Height, Rect.Width);
                int t = _Rect.Width;
                _Rect.Width = _Rect.Height;
                _Rect.Height = t;
            }
        }

        public void SetOffset(int x, int y)
        {
            //_Rect.Offset(x - _Rect.X, y - _Rect.Y);
            _Rect.X = x;
            _Rect.Y = y;
        }

        public void Offset(int dx, int dy)
        {
            _Rect.Offset(dx, dy);
        }

        public PackingRectangle GetPackingRectangle()
        {
            return new PackingRectangle((uint)_Rect.X, (uint)_Rect.Y, (uint)_Rect.Width, (uint)_Rect.Height, Id);
        }

        public AciColor GetAciColor()
        {
            var rgb = this.Color.ToPixel<Rgb24>();
            return new AciColor(rgb.R, rgb.G, rgb.B);
        }

        public string GetDxfSafeTag()
        {
            if ((Tag?.Length ?? 0) == 0) return null;
            var c = netDxf.Tables.TableObject.InvalidCharacters;
            StringBuilder sb = new StringBuilder(Tag.Length);
            foreach (var item in Tag)
            {
                sb.Append(c.Contains(item) ? DxfInvalidCharacterReplacement : item);
            }
            return sb.ToString();
        }
    }
}
