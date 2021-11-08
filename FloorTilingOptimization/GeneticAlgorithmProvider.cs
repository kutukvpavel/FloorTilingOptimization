using GeneticSharp.Domain;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Infrastructure.Framework.Threading;
using System;
using System.Threading;

namespace FloorTilingOptimization
{
    public static class GeneticAlgorithmProvider
    {
        public static event EventHandler<Tuple<int, double, double, double>> GenerationRan;

        public static TilingChromosome Run(SupportStructure support, Stock stock, 
            int steps, float mutationProbability, float crossoverProbability,
            CancellationToken cancel)
        {
            var fitness = new FuncFitness((x) =>
            {
                var c = (TilingChromosome)x;
                c.PlacedSheets = new Stock(stock, c.GetFlipString()) { Name = "Overlapped" };
                var res = Algorithms.PlaceAndAssessSheets(c.PlacedSheets, support, c.GetSequence(),
                    out Stock children, out Stock assessed);
                c.Children = children;
                c.Assessed = assessed; 
                return res;
            });
            var chromosome = new TilingChromosome(stock.Sheets.Length, true);
            var crossover = new TilingCrossover();
            var mutation = new TilingMutation();
            var selection = new RouletteWheelSelection();
            var population = new Population(50, 100, chromosome);
            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
            {
                Termination = new GenerationNumberTermination(steps),
                MutationProbability = mutationProbability,
                CrossoverProbability = crossoverProbability,
                TaskExecutor = new ParallelTaskExecutor()
                {
                    MaxThreads = 4,
                    MinThreads = 2
                }
            };

            double maxFitness = 0;
            TilingChromosome best = null;
            ga.GenerationRan += (o, e) =>
            {
                var c = (TilingChromosome)ga.BestChromosome;
                if (c.Fitness.Value >= maxFitness)
                {
                    maxFitness = c.Fitness.Value;
                    best = (TilingChromosome)c.Clone();
                }
                GenerationRan?.Invoke(ga, new Tuple<int, double, double, double>(
                    ga.GenerationsNumber, c.Fitness.Value, c.Assessed.TotalArea, best.Fitness.Value));
            };

            var gaThread = new Thread(() => ga.Start());
            gaThread.Start();
            while (gaThread.IsAlive)
            {
                Thread.Sleep(100);
                if (cancel.IsCancellationRequested)
                {
                    ga.Stop();
                    Thread.Sleep(1000);
                }
            }
            return best;
        }
    }
}
