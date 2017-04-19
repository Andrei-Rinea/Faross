namespace Faross.Models
{
    public class Environment : ModelBase
    {
        public Environment(string name, long id) : base(id)
        {
            Name = name;
        }

        public string Name { get; }

        protected override bool EqualsCore(ModelBase other)
        {
            var otherEnv = other as Environment;
            return otherEnv != null && otherEnv.Name == Name;
        }
    }
}