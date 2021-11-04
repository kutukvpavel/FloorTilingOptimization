using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Mutations;

namespace FloorTilingOptimization
{
    public class Multimutation : MutationBase
    {
        public Multimutation(params IMutation[] mutations) : base()
        {
            _Mutations = mutations;
            IsOrdered = mutations.Any(x => x.IsOrdered);
        }

        protected override void PerformMutate(IChromosome chromosome, float probability)
        {
            var m = (Multichromosome)chromosome;
            for (int i = 0; i < _Mutations.Length; i++)
            {
                var locus = (IChromosome)m.GetGene(i).Value;
                _Mutations[i].Mutate(locus, probability);
                m.ReplaceGene(i, new Gene(locus));
            }
        }

        private readonly IMutation[] _Mutations;
    }
}
