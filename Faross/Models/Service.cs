using System.Collections.Generic;

namespace Faross.Models
{
    public class Service : ModelBase
    {
        public Service(long id, string name, IReadOnlyCollection<Environment> runsOn) : base(id)
        {
            Name = name;
            RunsOn = runsOn;
        }

        public string Name { get; }
        public IReadOnlyCollection<Environment> RunsOn { get; }
    }
}