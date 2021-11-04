using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using GeneticSharp.Domain.Chromosomes;

namespace FloorTilingOptimization
{
    public class TilingChromosome : Multichromosome, IChromosome
    {
        public TilingChromosome(int length, bool startFromHorizotalOrientation = true) 
            : base(new OrderedChromosome(length), new BinaryChromosome(length, startFromHorizotalOrientation))
        {
            Tiles = length;
        }

        public int Tiles { get; }

        public double TotalAreaCovered { get; set; }

        public Rectangle[] AssessedRects { get; set; }

        public Sheet[] Sheets { get; set; }

        public Sheet[] Children { get; set; }

        public int[] GetSequence()
        {
            return (GetGene(0).Value as OrderedChromosome).GetSequence();
        }

        public bool GetFlip(int i)
        {
            return (GetGene(1).Value as BinaryChromosome).GetValue(i);
        }

        public bool[] GetFlipString()
        {
            return (GetGene(1).Value as BinaryChromosome).GetGenes().Select(x => (int)x.Value != 0).ToArray();
        }

        public override IChromosome CreateNew()
        {
            return new TilingChromosome(Tiles);
        }

        public override IChromosome Clone()
        {
            var c = (TilingChromosome)base.Clone();
            c.TotalAreaCovered = TotalAreaCovered;
            c.AssessedRects = AssessedRects;
            c.Children = Children;
            c.Sheets = Sheets;
            return c;
        }
    }
}
