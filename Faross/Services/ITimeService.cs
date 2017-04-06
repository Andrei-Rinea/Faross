using System;

namespace Faross.Services
{
    public interface ITimeService
    {
        DateTimeOffset UtcNow { get; }
    }
}