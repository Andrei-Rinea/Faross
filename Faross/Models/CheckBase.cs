using System;
using System.Collections.Generic;
using System.Linq;

namespace Faross.Models
{
    public abstract class CheckBase : ModelBase
    {
        protected CheckBase(
            long id,
            Environment environment,
            Service service,
            TimeSpan interval,
            IReadOnlyCollection<ConditionBase> conditions) : base(id)
        {
            if (interval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(interval));
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));
            if (!conditions.Any()) throw new ArgumentException("conditions is empty", nameof(conditions));

            Environment = environment ?? throw new ArgumentNullException(nameof(environment));
            Service = service ?? throw new ArgumentNullException(nameof(service));
            Interval = interval;
            Conditions = conditions;
        }

        public abstract CheckType Type { get; }

        public Environment Environment { get; }
        public Service Service { get; }
        public TimeSpan Interval { get; }
        public IReadOnlyCollection<ConditionBase> Conditions { get; }

        public abstract TimeSpan GetMaxDuration();

        protected override bool EqualsCore(ModelBase other)
        {
            var otherCheck = other as CheckBase;
            return otherCheck != null &&
                   otherCheck.Type == Type &&
                   otherCheck.Environment.Equals(Environment) &&
                   otherCheck.Service.Equals(Service) &&
                   otherCheck.Interval == Interval;
        }
    }
}