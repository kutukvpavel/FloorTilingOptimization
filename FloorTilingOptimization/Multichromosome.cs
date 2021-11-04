using GeneticSharp.Domain.Chromosomes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FloorTilingOptimization
{
    public class Multichromosome : ChromosomeBase
    {
        public Multichromosome(params IChromosome[] loci) : base(loci.Length)
        {
            _Loci = loci.Select(x => x.Clone()).ToArray();
            for (int i = 0; i < Length; i++)
            {
                ReplaceGene(i, new Gene(loci[i].CreateNew()));
            }
        }

        public override IChromosome CreateNew()
        {
            return new Multichromosome(_Loci);
        }

        public override Gene GenerateGene(int geneIndex)
        {
            return new Gene(_Loci[geneIndex].CreateNew());
        }

        public override IChromosome Clone()
        {
            var c = base.Clone() as Multichromosome;
            c._Loci = new IChromosome[Length];
            for (int i = 0; i < Length; i++)
            {
                c._Loci[i] = _Loci[i].Clone();
            }
            return c;
        }

        private IChromosome[] _Loci;
    }
}
