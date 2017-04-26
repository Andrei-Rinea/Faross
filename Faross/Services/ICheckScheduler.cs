using Faross.Models;

namespace Faross.Services
{
    public interface ICheckScheduler
    {
        void Init(Configuration configuration);
        void Update(Configuration configuration);
    }
}