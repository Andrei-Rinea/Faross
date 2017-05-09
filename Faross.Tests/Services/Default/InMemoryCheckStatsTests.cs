using System;
using System.Linq;
using Faross.Models;
using Xunit;
using Faross.Services.Default;
using Faross.Tests.Models;

namespace Faross.Tests.Services.Default
{
    public class InMemoryCheckStatsTests
    {
        private readonly InMemoryCheckStats _systemUnderTest;

        public InMemoryCheckStatsTests()
        {
            _systemUnderTest = new InMemoryCheckStats();
        }

        [Fact]
        public void AddCheckResult_RejectsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _systemUnderTest.AddCheckResult(null));
        }

        [Fact]
        public void AddCheckResult_AddsStats()
        {
            var beforeCnt = _systemUnderTest.GetAllStats().Count();
            var result = CheckResultTests.CreateCheckResult();

            _systemUnderTest.AddCheckResult(result);

            var afterCnt = _systemUnderTest.GetAllStats().Count();
            Assert.True(afterCnt == beforeCnt + 1);
        }

        [Fact]
        public void AddCheckResult_ProducesFirstStats()
        {
            var result = CheckResultTests.CreateCheckResult();
            _systemUnderTest.AddCheckResult(result);

            var stats = _systemUnderTest.GetAllStats().Single();

            Assert.Null(stats.ChangeSince);
            Assert.Equal(Statistics.Variation.NoData, stats.ChangeType);
            Assert.Null(stats.PreviousDifferentResult);
        }

        [Fact]
        public void AddCheckResult_UpdatesExistingStats_ForSameStatus()
        {
            var now = DateTimeOffset.UtcNow;
            var result1 = CheckResultTests.CreateCheckResult(now);
            var result2 = CheckResultTests.CreateCheckResult(now.AddMinutes(15));

            _systemUnderTest.AddCheckResult(result1);
            _systemUnderTest.AddCheckResult(result2);

            var allStats = _systemUnderTest.GetAllStats().ToArray();
            Assert.Equal(1, allStats.Length);
            var stats = allStats.Single();

            Assert.NotNull(stats.ChangeSince);
            Assert.InRange(stats.ChangeSince.Value, TimeSpan.FromMinutes(14), TimeSpan.FromMinutes(16));
            Assert.Equal(Statistics.Variation.NoData, stats.ChangeType);
            Assert.Null(stats.PreviousDifferentResult);
        }

        [Fact]
        public void AddCheckResult_UpdatesExistingStats_ForDifferentStatus()
        {
        }
    }
}