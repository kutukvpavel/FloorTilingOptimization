using System;
using System.Collections.Generic;
using System.Text;

namespace FloorTilingOptimization
{
    public interface IPlottableRectContainer
    {
        public string Name { get; }
        public PlottableRect[] GetPlottableRects();
    }
}
