namespace Faross.Models
{
    public abstract class ModelBase
    {
        protected ModelBase(long id)
        {
            Id = id;
        }

        public long Id { get; }
    }
}