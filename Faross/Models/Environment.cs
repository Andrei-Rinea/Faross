namespace Faross.Models
{
    public class Environment : ModelBase
    {
        public Environment(string name, long id) : base(id)
        {
            Name = name;
        }

        public string Name { get; }
    }
}