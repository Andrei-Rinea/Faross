using System.Collections.Generic;
using Faross.Util;

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

        protected override bool EqualsCore(ModelBase other)
        {
            var otherService = other as Service;
            return otherService != null &&
                   otherService.Name == Name &&
                   otherService.RunsOn.Equivalent(RunsOn);
        }
    }
}