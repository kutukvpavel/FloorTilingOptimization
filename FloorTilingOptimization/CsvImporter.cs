using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FloorTilingOptimization
{
    public static class CsvImporter
    {
        public static CsvConfiguration CsvConfig { get; } = new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            Delimiter = ";",
            MissingFieldFound = null
        };

        public static T LoadCsv<T>(string path, ColorPalette c) where T : ICsvAware, new()
        {
            using TextReader tr = new StreamReader(path);
            using CsvReader cr = new CsvReader(tr, CsvConfig);
            var res = new T();
            res.ReadCsv(cr, c);
            return res;
        }

        public static void SaveExampleCsv<T>(string path) where T : ICsvAware, new()
        {
            using TextWriter tw = new StreamWriter(path);
            using CsvWriter cw = new CsvWriter(tw, CsvConfig);
            new T().WriteExampleCsv(cw);
        }
    }
}
