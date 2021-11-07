using CsvHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FloorTilingOptimization
{
    public class Stock : ICsvAware, IPlottableRectContainer
    {
        public Stock() { Sheets = new Sheet[0]; }
        public Stock(Stock source) //This constructor copies stock (used to copy initial stock)
        {
            Sheets = CopySheets(source.Sheets);
            TotalArea = source.TotalArea;
        }
        public Stock(Stock source, bool[] orientation) : this(source)
        {
            Orient(orientation);
        }
        public Stock(IEnumerable<Sheet> source) //This constructor doesn't copy the stock (used to construct a stock instance from a single-use array)
        {
            Sheets = source.ToArray();
            CalculateTotalArea();
        }
        public Stock(IEnumerable<Sheet> source, bool[] orientation) : this(source)
        {
            Orient(orientation);
        }

        public Sheet[] Sheets { get; private set; }
        public int TotalArea { get; private set; }
        public string Name { get; set; }

        public void ReadCsv(CsvReader r, ColorPalette c)
        {
            Sheets = r.GetRecords<CsvType>().Select((x, i) =>
                new Sheet(x.Length, x.Width, x.Thickness, i, c.GetNextColor())).ToArray();
            CalculateTotalArea();
        }

        public void WriteExampleCsv(CsvWriter w)
        {
            w.WriteHeader<CsvType>();
            w.NextRecord();
        }
        public PlottableRect[] GetPlottableRects()
        {
            return Sheets;
        }

        private class CsvType
        {
            public CsvType() { }
            public int Length { get; set; }
            public int Width { get; set; }
            public int Thickness { get; set; }
        }

        private void Orient(bool[] orientation)
        {
            for (int i = 0; i < Sheets.Length; i++)
            {
                Sheets[i].Orient(orientation[i]);
            }
        }
        private Sheet[] CopySheets(IEnumerable<Sheet> source)
        {
            return source.Select(x => new Sheet(x)).ToArray();
        }
        private void CalculateTotalArea()
        {
            TotalArea = Sheets.Sum(x => x.Area);
        }

    }
}
