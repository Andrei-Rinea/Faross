using System;

namespace Faross.Models
{
    public abstract class CheckBase : ModelBase
    {
        protected CheckBase(long id, Environment environment, Service service, TimeSpan interval) : base(id)
        {
            if (interval <= TimeSpan.MinValue) throw new ArgumentOutOfRangeException(nameof(interval));

            Environment = environment ?? throw new ArgumentNullException(nameof(environment));
            Service = service ?? throw new ArgumentNullException(nameof(service));
            Interval = interval;
        }

        public abstract CheckType Type { get; }

        public Environment Environment { get; }
        public Service Service { get; }
        public TimeSpan Interval { get; }
    }
}