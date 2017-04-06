using System;
using System.Collections.Generic;

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
    }
}