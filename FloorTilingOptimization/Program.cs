using System;
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
            [Option('f', SetName = "n", Default = 1, Required = false, HelpText = "GA target fitness reduction multiplier")]
            public float TargetFitnessReduction { get; set; }
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
                    RealMain(x.SheetsFile, x.BeamsFile,
                        x.RunRectpack, x.PackingDensity, x.TargetFitnessReduction);
                }
            });
        }

        public static void ExampleMain()
        {
            CsvApi.SaveExampleCsv<Beam>(ImagingApi.CreateFilePathInCurrentDir("beams.csv"));
            CsvApi.SaveExampleCsv<Sheet>(ImagingApi.CreateFilePathInCurrentDir("sheets.csv"));
        }

        public static void RealMain(string sheetsPath, string beamsPath,
            bool runRectpack, float packingDensity, float gaTargetReduction)
        {
            Console.WriteLine("Processing sheets...");
            var sheets = CsvApi.LoadCsv<Sheet>(sheetsPath);
            Sheet.RotateAndTag(sheets);
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
            double targetFitness = structure.BoundedArea * gaTargetReduction / sheets.Sum(x => x.Area);
            Console.WriteLine($"Target fitness = {targetFitness}");
            GeneticAlgorithmProvider.Run(structure, sheets, targetFitness, _TokenSource.Token);
            Console.WriteLine("Drawing best result...");
            ImagingApi.SaveRectanglesAsImage(sheets.Select(x => x.ToRectangle()).ToArray(), structure.Bounds,
                ImagingApi.CreateFilePathInCurrentDir("ga.png"));
            structure.SaveLastAssessedImage();
            Console.WriteLine("Finished.");
        }

        private static CancellationTokenSource _TokenSource = new CancellationTokenSource();

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Cancellation requested...");
            _TokenSource.Cancel();
            e.Cancel = true;
        }

        private static void GeneticAlgorithmProvider_GenerationRan(object sender, Tuple<int, double, double> e)
        {
            Console.WriteLine($"G = {e.Item1}, F = {e.Item2}, A = {e.Item3}");
        }
    }
}
