using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Infrastructure.Framework.Threading;
using System;
using System.Linq;
using System.Threading;

namespace FloorTilingOptimization
{
    public static class GeneticAlgorithmProvider
    {
        public static event EventHandler<Tuple<int, double, double, double>> GenerationRan;

        public static TilingChromosome Run(SupportStructure support, Sheet[] sheets, int steps, CancellationToken cancel)
        {
            var fitness = new FuncFitness((x) =>
            {
                var c = (TilingChromosome)x;
                for (int i = 0; i < sheets.Length; i++)
                {
                    sheets[i].Orient(c.GetFlip(i));
                }
                var res = support.PlaceAndAssess(sheets, c.GetSequence());
                c.Sheets = Sheet.DeepCopy(sheets);
                c.AssessedRects = support.GetLastAssessed();
                c.Children = support.GetLastChildren();
                c.TotalAreaCovered = res.Item2;
                return res.Item1;
            });
            var chromosome = new TilingChromosome(sheets.Length, true);
            var crossover = new TilingCrossover();
            var mutation = new TilingMutation();
            var selection = new RouletteWheelSelection();
            var population = new Population(50, 100, chromosome);
            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
            ga.Termination = new GenerationNumberTermination(steps);
            ga.MutationProbability = 0.5f;
            ga.CrossoverProbability = 0.75f;

            ga.TaskExecutor = new LinearTaskExecutor();

            double maxFitness = 0;
            TilingChromosome best = null;
            ga.GenerationRan += (o, e) =>
            {
                var c = (TilingChromosome)ga.BestChromosome;
                if (c.Fitness.Value > maxFitness)
                {
                    maxFitness = c.Fitness.Value;
                    best = (TilingChromosome)c.Clone();
                }
                GenerationRan?.Invoke(ga, new Tuple<int, double, double, double>(
                    ga.GenerationsNumber, c.Fitness.Value, c.TotalAreaCovered, best.Fitness.Value));
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
