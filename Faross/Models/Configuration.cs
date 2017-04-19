using System;
using System.Collections.Generic;
using Faross.Util;

namespace Faross.Models
{
    public class Configuration
    {
        public IReadOnlyCollection<Environment> Environments { get; }
        public IReadOnlyCollection<Service> Services { get; }
        public IReadOnlyCollection<CheckBase> Checks { get; }

        public Configuration(
            IReadOnlyCollection<Environment> environments,
            IReadOnlyCollection<Service> services,
            IReadOnlyCollection<CheckBase> checks)
        {
            Environments = environments ?? throw new ArgumentNullException(nameof(environments));
            Services = services ?? throw new ArgumentNullException(nameof(services));
            Checks = checks ?? throw new ArgumentNullException(nameof(checks));
        }

        public override bool Equals(object obj)
        {
            var other = obj as Configuration;
            return other != null &&
                   other.Environments.Equivalent(Environments) &&
                   other.Services.Equivalent(Services) &&
                   other.Checks.Equivalent(Checks);
        }

        public override int GetHashCode()
        {
            return HashCodeUtil.GetCombinedHash(Environments, Services, Checks);
        }
    }
}