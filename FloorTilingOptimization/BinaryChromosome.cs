using System;
using System.Collections.Generic;
using System.Text;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;

namespace FloorTilingOptimization
{
    public class BinaryChromosome : BinaryChromosomeBase
    {
        public BinaryChromosome(int length) : base(length)
        {
            var vals = RandomizationProvider.Current.GetInts(length, 0, 2);
            for (int i = 0; i < length; i++)
            {
                ReplaceGene(i, new Gene(vals[i]));
            }
        }

        public override IChromosome CreateNew()
        {
            return new BinaryChromosome(Length);
        }

        public bool GetValue(int i)
        {
            return (int)GetGene(i).Value == 1;
        }
    }
}
