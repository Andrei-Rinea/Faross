namespace Faross.Models
{
    public abstract class ModelBase
    {
        protected ModelBase(long id)
        {
            Id = id;
        }

        public long Id { get; }

        protected abstract bool EqualsCore(ModelBase other);

        public override bool Equals(object obj)
        {
            var other = obj as ModelBase;
            if (other == null) return false;
            return Id == other.Id && EqualsCore(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}