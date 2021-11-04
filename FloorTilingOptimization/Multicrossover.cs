using System;
using System.Collections.Generic;
using System.Text;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using System.Linq;

namespace FloorTilingOptimization
{
    public class Multicrossover : CrossoverBase
    {
        public Multicrossover(params ICrossover[] crossovers) 
            : base(crossovers.Max(x => x.ParentsNumber), crossovers.Max(x => x.ChildrenNumber))
        {
            _Crossovers = crossovers;
        }

        protected override IList<IChromosome> PerformCross(IList<IChromosome> parents)
        {
            var multisomes = parents.Select(x => (Multichromosome)x).ToArray();
            Multichromosome[] results = new Multichromosome[ChildrenNumber];
            for (int i = 0; i < ChildrenNumber; i++)
            {
                results[i] = (Multichromosome)parents[0].CreateNew();
            }
            for (int i = 0; i < parents[0].Length; i++)
            {
                var currentLocus = multisomes.Select(x => (IChromosome)x.GetGene(i).Value).ToList();
                if (i < _Crossovers.Length)
                {
                    var locusChildren = _Crossovers[i].Cross(currentLocus);
                    for (int j = 0; j < ChildrenNumber; j++)
                    {
                        if (j < _Crossovers[i].ChildrenNumber)
                        {
                            results[j].ReplaceGene(i, new Gene(locusChildren[j]));
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            return results;
        }

        private readonly ICrossover[] _Crossovers;
    }
}
