using System.Collections.Generic;
using Faross.Models;

namespace Faross.ViewModels.Stats
{
    public class IndexViewModel
    {
        public IEnumerable<Statistics> Stats { get; }

        public IndexViewModel(IEnumerable<Statistics> stats)
        {
            Stats = stats;
        }
    }
}