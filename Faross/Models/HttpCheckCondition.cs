namespace Faross.Models
{
    public abstract class HttpCheckCondition : ConditionBase
    {
        protected HttpCheckCondition(string name, bool stopOnFail) : base(name)
        {
            StopOnFail = stopOnFail;
        }

        public bool StopOnFail { get; }

        public abstract HttpCheckConditionType Type { get; }
    }
}