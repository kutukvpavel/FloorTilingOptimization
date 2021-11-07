using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloorTilingOptimization
{
    public class ColorPalette
    {
        public ColorPalette(int colors = 32, int seed = 0)
        {
            Length = colors;
            Current = seed;
        }
        public ColorPalette(Color fixedColor)
        {
            Length = 1;
            CurrentColor = fixedColor;
        }

        public int Current { get; private set; }
        public Rgba32 CurrentColor { get; private set; }
        public int Length { get; }
        public float Alpha { get; set; } = 0.5f;

        public Rgba32 GetNextColor()
        {
            return GetNextColor(Alpha);
        }
        public Rgba32 GetNextColor(float alphaOverride)
        {
            if (Length > 1)
            {
                CurrentColor = FromHue(Current++ / (float)Length);
                if (Current > Length) Reset();
            }
            return new Rgba32(CurrentColor.R, CurrentColor.G, CurrentColor.B, (byte)(alphaOverride * 255));
        }

        public void Reset()
        {
            Current = 0;
        }

        public static Rgba32 FromHue(float hue)
        {
            hue *= 360.0f;

            float h = hue / 60.0f;
            float x = (1.0f - Math.Abs((h % 2.0f) - 1.0f));

            float r, g, b;
            if (h >= 0.0f && h < 1.0f)
            {
                r = 1;
                g = x;
                b = 0.0f;
            }
            else if (h >= 1.0f && h < 2.0f)
            {
                r = x;
                g = 1;
                b = 0.0f;
            }
            else if (h >= 2.0f && h < 3.0f)
            {
                r = 0.0f;
                g = 1;
                b = x;
            }
            else if (h >= 3.0f && h < 4.0f)
            {
                r = 0.0f;
                g = x;
                b = 1;
            }
            else if (h >= 4.0f && h < 5.0f)
            {
                r = x;
                g = 0.0f;
                b = 1;
            }
            else if (h >= 5.0f && h < 6.0f)
            {
                r = 1;
                g = 0.0f;
                b = x;
            }
            else
            {
                r = 0.0f;
                g = 0.0f;
                b = 0.0f;
            }

            return new Rgba32(r, g, b);
        }
    }
}
