namespace Faross.Models
{
    public abstract class ConditionBase
    {
        protected ConditionBase(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}