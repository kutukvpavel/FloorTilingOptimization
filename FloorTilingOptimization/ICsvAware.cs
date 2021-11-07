using System;
using System.Collections.Generic;
using System.Text;
using CsvHelper;

namespace FloorTilingOptimization
{
    public interface ICsvAware
    {
        public void ReadCsv(CsvReader r, ColorPalette c);
        public void WriteExampleCsv(CsvWriter w);
    }
}
