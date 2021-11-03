
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using System.Linq;

namespace FloorTilingOptimization
{
    public class IndexPermutationChromosome : ChromosomeBase
    {
        public IndexPermutationChromosome(int length) : base(length)
        {
            var numbers = RandomizationProvider.Current.GetUniqueInts(Length, 0, length);
            for (int i = 0; i < length; i++)
            {
                ReplaceGene(i, new Gene(numbers[i]));
            }
        }

        public override IChromosome CreateNew()
        {
            return new IndexPermutationChromosome(Length);
        }

        public override Gene GenerateGene(int geneIndex)
        {
            return new Gene(RandomizationProvider.Current.GetInt(0, Length));
        }

        public int[] GetSequence()
        {
            return GetGenes().Select(x => (int)x.Value).ToArray();
        }

        public double TotalAreaCovered { get; set; }

        public override IChromosome Clone()
        {
            var c = base.Clone() as IndexPermutationChromosome;
            c.TotalAreaCovered = TotalAreaCovered;
            return c;
        }
    }
}
