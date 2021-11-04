using System;
using System.Collections.Generic;
using System.Text;
using GeneticSharp.Domain.Crossovers;

namespace FloorTilingOptimization
{
    public class TilingCrossover : Multicrossover
    {
        public TilingCrossover() : base(new OrderedCrossover(), new UniformCrossover())
        {

        }
    }
}
