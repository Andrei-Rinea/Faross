using System.Collections.Generic;
using Faross.Models;
using Environment = Faross.Models.Environment;

namespace Faross.ViewModels.Stats
{
    public class IndexViewModel
    {
        public IndexViewModel(IDictionary<Environment, IEnumerable<Statistics>> envStats)
        {
            EnvStats = envStats;
        }

        public IDictionary<Environment, IEnumerable<Statistics>> EnvStats { get; }
    }
}