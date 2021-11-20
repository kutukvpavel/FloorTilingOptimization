using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FloorTilingOptimization
{
    public class Program
    {
        public const string TimeFormat = "yyyy-MM-dd_HH-mm-ss";

        public static ColorPalette SheetColors { get; } = new ColorPalette();
        public static ColorPalette BeamColors { get; } = new ColorPalette(3, 1) { Alpha = 0.75f };

        public class Options
        {
            [Option('s', SetName = "n", Required = true, HelpText = "CSV file containing [S]heet sizes")]
            public string SheetsFile { get; set; }
            [Option('b', SetName = "n", Required = true, HelpText = "CSV file containing [B]eam sizes and locations")]
            public string BeamsFile { get; set; }
            [Option('g', SetName = "g", Required = true, HelpText = "[G]enerate example CSV files with required headers")]
            public bool GenerateExampleCsv { get; set; }
            [Option('d', SetName = "n", Default = 1, Required = false, HelpText = "[D]ensity for RectpackSharp")]
            public float PackingDensity { get; set; }
            [Option('p', SetName = "n", Default = false, Required = false, HelpText = "Run Rect[P]ackSharp")]
            public bool RunRectpack { get; set; }
            [Option('a', SetName = "n", Default = true, Required = false, HelpText = "Run genetic [A]lgorithm")]
            public bool RunGA { get; set; }
            [Option('t', SetName = "n", Default = 100000, Required = false, HelpText = "GA steps before [T]ermination")]
            public int MaxSteps { get; set; }
            [Option('c', SetName = "n", Default = 1, Required = false, HelpText = "[C]ycle GA algorithm [C] times")]
            public int Cycles { get; set; }
            [Option('m', SetName = "n", Default = 0.5f, Required = false, HelpText = "[M]utation probability")]
            public float MutationProbability { get; set; }
            [Option('r', SetName = "n", Default = 0.75f, Required = false, HelpText = "C[R]ossover probability")]
            public float CrossoverProbability { get; set; }
            [Option('e', SetName = "n", Default = null, Required = false, HelpText = "R[E]generate GA output for specified order file")]
            public string RegenerateOutput { get; set; }
        }

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(x =>
            {
                if (x.GenerateExampleCsv)
                {
                    ExampleMain();
                }
                else if (x.RegenerateOutput != null)
                {
                    RegenerateMain(x);
                }
                else
                {
                    LoadInputData(x, out Stock sheets, out SupportStructure structure);
                    for (int i = 0; i < x.Cycles; i++)
                    {
                        _FitnessEvolution = new List<float>(x.MaxSteps);
                        RealMain(x, sheets, structure);
                    }
                }
            });
        }

        public static void ExampleMain()
        {
            CsvImporter.SaveExampleCsv<SupportStructure>(CreateFilePathInCurrentDir("beams.csv"));
            CsvImporter.SaveExampleCsv<Stock>(CreateFilePathInCurrentDir("sheets.csv"));
        }

        public static void RegenerateMain(Options o)
        {
            string[] data = File.ReadAllLines(o.RegenerateOutput);
            int[] order = data[0].Split(',').Select(x => int.Parse(x.Trim())).ToArray();
            bool[] flip = data[1].Split(',').Select(x => bool.Parse(x.Trim())).ToArray();
            double? originalFitness = data.Length > 2 ? (double?)double.Parse(data[2]) : null;
            string time = Path.GetFileNameWithoutExtension(o.RegenerateOutput);
            time = time.Substring(time.Length - TimeFormat.Length);
            LoadInputData(o, out Stock sheets, out SupportStructure structure);
            for (int i = 0; i < sheets.Sheets.Length; i++)
            {
                sheets.Sheets[i].Orient(flip[i]);
            }
            var fitness 
                = Algorithms.PlaceAndAssessSheets(sheets, structure, order, out Stock children, out Stock assessed);
            string report = $"Regenerated F = {fitness}, A = {assessed.TotalArea}";
            Console.WriteLine(report);
            if (originalFitness == fitness || originalFitness == null)
            {
                File.AppendAllLines(o.RegenerateOutput, new string[] { report });
                SaveOutputGAData(time, structure, sheets, assessed, children, "regen_");
            }
            else
            {
                Console.WriteLine("Regenerated fitness doesn't match the original fitness.");
            }
        }

        public static void RealMain(Options o, Stock sheets, SupportStructure structure)
        {
            _TokenSource = new CancellationTokenSource();
            if (o.RunRectpack)
            {
                Console.WriteLine("Running RectpackSharp...");
                RectPackProvider.PackAndSaveImage(sheets.Sheets, o.PackingDensity);
            }
            if (o.RunGA)
            {
                //Prepare and run the GA
                Console.WriteLine("Running GA...");
                Console.CancelKeyPress += Console_CancelKeyPress;
                GeneticAlgorithmProvider.GenerationRan += GeneticAlgorithmProvider_GenerationRan;
                Console.WriteLine($"Max generations = {o.MaxSteps}");
                TilingChromosome best = GeneticAlgorithmProvider.Run(structure, sheets, 
                    o.MaxSteps, o.MutationProbability, o.CrossoverProbability, _TokenSource.Token);

                //Save the results
                Console.WriteLine($"Saving best result (F = {best.Fitness.Value})...");
                string time = DateTime.Now.ToString(TimeFormat);
                File.WriteAllLines(CreateFilePathInCurrentDir($"evolution_{time}.txt"),
                    _FitnessEvolution.Select(x => x.ToString()));
                //Sheet placement order and it's flip value allow to reconstruct the rest of the data later
                int[] sequence = best.GetSequence();
                File.WriteAllText(CreateFilePathInCurrentDir($"order_{time}.txt"),
                    @$"{string.Join(", ", sequence)}
{string.Join(", ", best.GetFlipString())}
{best.Fitness}
{best.Assessed.TotalArea}");
                SaveOutputGAData(time, structure, best.PlacedSheets, best.Assessed, best.Children);
            }
            Console.WriteLine("Finished.");
        }

        private static string _ResultsSubfolder = null;
        public static string CreateFilePathInCurrentDir(string name)
        {
            var p = Path.Combine(Environment.CurrentDirectory, "results", name);
            if (_ResultsSubfolder == null)
            {
                _ResultsSubfolder = Path.GetDirectoryName(p);
                if (!Directory.Exists(_ResultsSubfolder)) Directory.CreateDirectory(_ResultsSubfolder);
            }
            return p;
        }

        private static CancellationTokenSource _TokenSource;
        private static List<float> _FitnessEvolution;

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Cancellation requested...");
            _TokenSource.Cancel();
            e.Cancel = true;
        }

        private static void GeneticAlgorithmProvider_GenerationRan(object sender, Tuple<int, double, double, double> e)
        {
            Console.WriteLine($"G = {e.Item1}, F = {e.Item2}, A = {e.Item3}, BF = {e.Item4}");
            _FitnessEvolution.Add((float)e.Item2);
        }

        private static void LoadInputData(Options o, out Stock sheets, out SupportStructure structure)
        {
            Console.WriteLine("Processing sheets...");
            sheets = CsvImporter.LoadCsv<Stock>(o.SheetsFile, SheetColors);
            sheets.Name = "As Loaded";
            Console.WriteLine("Processing beams...");
            structure = CsvImporter.LoadCsv<SupportStructure>(o.BeamsFile, BeamColors);
            structure.Name = "Support Structure (As Loaded)";
        }

        private static void SaveOutputGAData(string time, SupportStructure structure, Stock sheets,
            Stock assessed, Stock children, string prefix = "")
        {
            //Images are a fast and easy way to assess the solution
            Console.WriteLine($"Drawing...");
            using (Image<Rgba32> image = ImageExporter.CreateImage(structure.Bounds, out float xOffset, out float yOffset))
            {
                image.AddRects(xOffset, yOffset, structure.Beams);
                using (Image<Rgba32> overlap = image.Clone())
                {
                    overlap.AddRects(xOffset, yOffset, sheets.Sheets);
                    overlap.Downscale();
                    overlap.Save(CreateFilePathInCurrentDir($"{prefix}overlap_{time}.png"));
                }
                using (Image<Rgba32> cut = image.Clone())
                {
                    cut.AddRects(xOffset, yOffset, assessed.Sheets);
                    cut.Downscale();
                    cut.Save(CreateFilePathInCurrentDir($"{prefix}cut_{time}.png"));
                }
                using (Image<Rgba32> c = image.Clone())
                {
                    c.AddRects(xOffset, yOffset, children.Sheets);
                    c.Downscale();
                    c.Save(CreateFilePathInCurrentDir($"{prefix}children_{time}.png"));
                }
                image.Downscale();
                image.Save(CreateFilePathInCurrentDir($"{prefix}structure_{time}.png"));
            }
            //DXF can be imported into AutoCAD for further manual tweaking
            Console.WriteLine("Exporting DXF...");
            DxfExporter.Export(CreateFilePathInCurrentDir($"{prefix}export_{time}.dxf"), 
                structure, sheets, assessed, children);
        }
    }
}
