using System;
using System.Collections.Generic;
using System.Linq;
using Faross.Util;

namespace Faross.Models
{
    public class Service : ModelBase
    {
        public Service(long id, string name, IReadOnlyCollection<Environment> runsOn) : base(id)
        {
            Name = name;
            RunsOn = runsOn ?? throw new ArgumentNullException(nameof(runsOn));

            if (RunsOn.Any(r => r == null)) throw new ArgumentException("runsOn contains a null");
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