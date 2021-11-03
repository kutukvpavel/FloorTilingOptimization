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
    public static class CsvApi
    {
        public static CsvConfiguration CsvConfig { get; } = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            MissingFieldFound = null
        };

        public static T[] LoadCsv<T>(string path)
        {
            using TextReader tr = new StreamReader(path);
            using CsvReader cr = new CsvReader(tr, CsvConfig);
            return cr.GetRecords<T>().ToArray();
        }

        public static void SaveExampleCsv<T>(string path)
        {
            using TextWriter tw = new StreamWriter(path);
            using CsvWriter cw = new CsvWriter(tw, CsvConfig);
            cw.WriteHeader<T>();
            cw.NextRecord();
        }
    }
}
