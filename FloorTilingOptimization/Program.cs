using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using CommandLine;

namespace FloorTilingOptimization
{
    public class Program
    {
        public class Options
        {
            [Option('s', SetName = "n", Required = true, HelpText = "CSV file containing sheet sizes")]
            public string SheetsFile { get; set; }
            [Option('b', SetName = "n", Required = true, HelpText = "CSV file containing beam sizes and locations")]
            public string BeamsFile { get; set; }
            [Option('g', SetName = "g", Required = true, HelpText = "Generate example CSV files with required headers")]
            public bool GenerateExampleCsv { get; set; }
            [Option('d', SetName = "n", Default = 1, Required = false, HelpText = "Density for RectpackSharp")]
            public float PackingDensity { get; set; }
            [Option('p', SetName = "n", Default = false, Required = false, HelpText = "Run RectpackSharp")]
            public bool RunRectpack { get; set; }
            [Option('f', SetName = "n", Default = 100000, Required = false, HelpText = "GA max steps")]
            public int MaxSteps { get; set; }
            [Option('c', SetName = "n", Default = 1, Required = false, HelpText = "Cycle GA algorithm C times")]
            public int Cycles { get; set; }
        }

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(x =>
            {
                if (x.GenerateExampleCsv)
                {
                    ExampleMain();
                }
                else
                {
                    for (int i = 0; i < x.Cycles; i++)
                    {
                        RealMain(x.SheetsFile, x.BeamsFile,
                            x.RunRectpack, x.PackingDensity, x.MaxSteps);
                    }
                }
            });
        }

        public static void ExampleMain()
        {
            CsvApi.SaveExampleCsv<Beam>(ImagingApi.CreateFilePathInCurrentDir("beams.csv"));
            CsvApi.SaveExampleCsv<Sheet>(ImagingApi.CreateFilePathInCurrentDir("sheets.csv"));
        }

        public static void RealMain(string sheetsPath, string beamsPath,
            bool runRectpack, float packingDensity, int maxSteps)
        {
            _TokenSource = new CancellationTokenSource();
            Console.WriteLine("Processing sheets...");
            var sheets = CsvApi.LoadCsv<Sheet>(sheetsPath);
            Sheet.RotateAndTag(sheets, false);
            Console.WriteLine("Processing beams...");
            var beams = CsvApi.LoadCsv<Beam>(beamsPath);
            var structure = new SupportStructure(beams);
            structure.SaveStructureImage();
            if (runRectpack)
            {
                Console.WriteLine("Running RectpackSharp...");
                RectPack.PackAndSaveImage(sheets, packingDensity);
            }
            Console.WriteLine("Running GA...");
            Console.CancelKeyPress += Console_CancelKeyPress;
            GeneticAlgorithmProvider.GenerationRan += GeneticAlgorithmProvider_GenerationRan;
            Console.WriteLine($"Max generations = {maxSteps}");
            TilingChromosome best = GeneticAlgorithmProvider.Run(structure, sheets, maxSteps, _TokenSource.Token);
            int[] sequence = best.GetSequence();
            string time = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            File.WriteAllText(ImagingApi.CreateFilePathInCurrentDir($"order_{time}.txt"),
                string.Join(", ", sequence) + Environment.NewLine + string.Join(", ", best.GetFlipString()));
            Console.WriteLine($"Drawing best result (F = {best.Fitness.Value})...");
            ImagingApi.SaveSheetsAsImage(best.Sheets, structure.Bounds,
                ImagingApi.CreateFilePathInCurrentDir($"overlap_{time}.png"));
            ImagingApi.SaveRectanglesAsImage(best.AssessedRects, structure.Bounds,
                ImagingApi.CreateFilePathInCurrentDir($"cut_{time}.png"), 
                sequence.Select(x => (object)$"{x},t={sheets[x].Thickness}").ToArray());
            ImagingApi.SaveSheetsAsImage(best.Children, structure.Bounds,
                ImagingApi.CreateFilePathInCurrentDir($"children_{time}.png"));
            Console.WriteLine("Exporting DXF...");
            DxfExporter.ExportSheetsAndMounting(best.Sheets, structure.GetMountingZones(),
                ImagingApi.CreateFilePathInCurrentDir($"export_overlap_{time}.dxf"));
            DxfExporter.ExportRectangles(best.AssessedRects,
                ImagingApi.CreateFilePathInCurrentDir($"export_cut_{time}.dxf"));
            DxfExporter.ExportRectangles(best.Children.Select(x => x.ToRectangle()).ToArray(),
                ImagingApi.CreateFilePathInCurrentDir($"export_children_{time}.dxf"));
            Console.WriteLine("Finished.");
        }

        private static CancellationTokenSource _TokenSource;

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Cancellation requested...");
            _TokenSource.Cancel();
            e.Cancel = true;
        }

        private static void GeneticAlgorithmProvider_GenerationRan(object sender, Tuple<int, double, double, double> e)
        {
            Console.WriteLine($"G = {e.Item1}, F = {e.Item2}, A = {e.Item3}, BF = {e.Item4}");
        }
    }
}
