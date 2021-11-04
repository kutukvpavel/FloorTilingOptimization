using System;
using System.Collections.Generic;
using System.Text;
using GeneticSharp.Domain.Chromosomes;

namespace FloorTilingOptimization
{
    public class TilingChromosome : Multichromosome, IChromosome
    {
        public TilingChromosome(int length) 
            : base(new OrderedChromosome(length), new BinaryChromosome(length))
        {

        }

        public double TotalAreaCovered { get; set; }

        public int[] GetSequence()
        {
            return (GetGene(0).Value as OrderedChromosome).GetSequence();
        }

        public bool GetFlip(int i)
        {
            return (GetGene(1).Value as BinaryChromosome).GetValue(i);
        }

        public override IChromosome CreateNew()
        {
            return new TilingChromosome(Length);
        }
    }
}
