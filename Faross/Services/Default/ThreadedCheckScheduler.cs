using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Faross.Models;

namespace Faross.Services.Default
{
    public class ThreadedCheckScheduler : ICheckScheduler
    {
        private readonly ICheckerFactory _checkerFactory;
        private readonly ICheckLog _checkLog;
        private readonly ICheckStats _checkStats;

        private Configuration _configuration;
        private ReadOnlyCollection<Thread> _threads;

        public ThreadedCheckScheduler(ICheckLog checkLog, ICheckStats checkStats, ICheckerFactory checkerFactory)
        {
            _checkLog = checkLog ?? throw new ArgumentNullException(nameof(checkLog));
            _checkStats = checkStats ?? throw new ArgumentNullException(nameof(checkStats));
            _checkerFactory = checkerFactory ?? throw new ArgumentNullException(nameof(checkerFactory));
        }

        public void Init(Configuration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            var threads = new List<Thread>();
            foreach (var check in _configuration.Checks)
            {
                var thread = new Thread(DoCheck);
                threads.Add(thread);
                thread.Start(check);
            }
            _threads = threads.AsReadOnly();
        }

        private void DoCheck(object obj)
        {
            var check = (CheckBase) obj;
//             check.Type

        }

        public void Update(Configuration configuration)
        {
            throw new NotImplementedException();
        }
    }
}