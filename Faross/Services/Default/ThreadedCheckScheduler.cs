using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using Faross.Models;

namespace Faross.Services.Default
{
    public class ThreadedCheckScheduler : ICheckScheduler
    {
        private class CheckRunnerInfo
        {
            public CheckRunnerInfo(Thread thread, CheckBase check)
            {
                Thread = thread;
                Check = check;
            }

            public Thread Thread { get; }
            public CheckBase Check { get; }
            public bool CancelRequested { get; private set; }

            public void Cancel()
            {
                CancelRequested = true;
            }
        }

        private readonly ICheckerFactory _checkerFactory;
        private readonly ICheckLog _checkLog;
        private readonly ICheckStats _checkStats;
        private readonly ILog _log;

        private Configuration _configuration;
        private ReadOnlyDictionary<CheckBase, CheckRunnerInfo> _checkThreads;

        public ThreadedCheckScheduler(
            ICheckLog checkLog,
            ICheckStats checkStats,
            ICheckerFactory checkerFactory,
            ILog log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _checkLog = checkLog ?? throw new ArgumentNullException(nameof(checkLog));
            _checkStats = checkStats ?? throw new ArgumentNullException(nameof(checkStats));
            _checkerFactory = checkerFactory ?? throw new ArgumentNullException(nameof(checkerFactory));
        }

        public void Init(Configuration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            var threads = new Dictionary<CheckBase, CheckRunnerInfo>();
            foreach (var check in _configuration.Checks)
            {
                var thread = new Thread(DoCheck);
                var info = new CheckRunnerInfo(thread, check);
                threads.Add(check, info);
                thread.Start(info);
            }
            _checkThreads = new ReadOnlyDictionary<CheckBase, CheckRunnerInfo>(threads);
        }

        private void DoCheck(object obj)
        {
            try
            {
                var checkInfo = (CheckRunnerInfo) obj;
                var check = checkInfo.Check;
                var checker = _checkerFactory.GetChecker(check);

                var checkTimer = new Stopwatch();
                var sleepTimer = new Stopwatch();

                while (true)
                {
                    if (checkInfo.CancelRequested)
                        return;

                    checkTimer.Reset();
                    checkTimer.Start();
                    var result = checker.Check(check);
                    _checkLog.LogCheck(result);
                    _checkStats.AddCheckResult(result);
                    checkTimer.Stop();

                    if (checkInfo.CancelRequested)
                        return;

                    var sleepTime = check.Interval - checkTimer.Elapsed;

                    sleepTimer.Reset();
                    sleepTimer.Start();
                    while (sleepTimer.Elapsed < sleepTime)
                    {
                        if (checkInfo.CancelRequested)
                            return;
                        Thread.Sleep(TimeSpan.FromMilliseconds(50));
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Fatal(ex.ToString());
                return;
            }
        }

        public void Update(Configuration configuration)
        {
            throw new NotImplementedException();
        }
    }
}