using System.Collections.Generic;

namespace Faross.Models
{
    public abstract class HttpCheckCondition
    {
        protected HttpCheckCondition(bool stopOnFail)
        {
            StopOnFail = stopOnFail;
        }

        public bool StopOnFail { get; }

        public abstract HttpCheckConditionType Type { get; }
    }
}