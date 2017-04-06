using System;

namespace Faross.Services.Default
{
    public class TimeService : ITimeService
    {
        public DateTimeOffset UtcNow => DateTime.UtcNow;
    }
}