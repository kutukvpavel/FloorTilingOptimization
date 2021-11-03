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
        public static event EventHandler<Tuple<int, double, double>> GenerationRan;

        public static void Run(SupportStructure support, Sheet[] sheets, double expectedFitness, CancellationToken cancel)
        {
            var fitness = new FuncFitness((x) =>
            {
                var c = x as IndexPermutationChromosome;
                var res = support.PlaceAndAssess(sheets, c.GetSequence());
                c.TotalAreaCovered = res.Item2;
                return res.Item1;
            });
            var chromosome = new IndexPermutationChromosome(sheets.Length);
            var crossover = new OrderedCrossover();
            var mutation = new ReverseSequenceMutation();
            var selection = new RouletteWheelSelection();
            var population = new Population(50, 100, chromosome);
            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
            ga.Termination = new FitnessThresholdTermination(expectedFitness);
            ga.MutationProbability = 0.5f;
            ga.CrossoverProbability = 0.75f;

            // The fitness evaluation of whole population will be running on parallel.
            ga.TaskExecutor = new LinearTaskExecutor();

            // Everty time a generation ends, we log the best solution.
            ga.GenerationRan += (o, e) =>
            {
                var c = (IndexPermutationChromosome)ga.BestChromosome;
#if DEBUG
                Console.WriteLine($"Current order: {string.Join(", ", c.GetSequence())}");
#endif
                GenerationRan?.Invoke(ga, new Tuple<int, double, double>(
                    ga.GenerationsNumber, c.Fitness.Value, c.TotalAreaCovered));
            };

            // Starts the genetic algorithm in a separate thread.
            var gaThread = new Thread(() => ga.Start());
            gaThread.Start();
            while (!cancel.IsCancellationRequested && gaThread.IsAlive)
            {
                Thread.Sleep(100);
            }
            if (cancel.IsCancellationRequested) ga.Stop();
        }
    }
}
