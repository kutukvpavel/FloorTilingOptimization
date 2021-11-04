using System;
using System.Collections.Generic;
using System.Text;
using GeneticSharp.Domain.Mutations;

namespace FloorTilingOptimization
{
    public class TilingMutation : Multimutation, IMutation
    {
        public TilingMutation() : base(new ReverseSequenceMutation(), new FlipBitMutation())
        {

        }
    }
}
