using System;
using System.Collections.Generic;
using Faross.Models;
using Xunit;
using Environment = Faross.Models.Environment;

namespace Faross.Tests.Models
{
    public class CheckResultTests
    {
        private readonly CheckResult _checkResultA1;
        private readonly CheckResult _checkResultA2;
        private readonly CheckResult _checkResultA1Clone;
        private readonly CheckResult _checkResultB1;
        private readonly CheckResult _checkResultC1;
        private readonly CheckResult _checkResultD1;

        private static readonly Environment Env = new Environment("env", 1);
        private static readonly Service Srv = new Service(1, "srv", new List<Environment> {Env});
        private static readonly HttpStatusCondition Cnd = new HttpStatusCondition("status", true, HttpStatusCondition.Operator.Equal, 200);
        private static readonly HttpCheck Ck1 = new HttpCheck(1, Env, Srv, TimeSpan.FromMinutes(15), new Uri("http://localhost"), new List<HttpCheckCondition> {Cnd}, HttpCheck.HttpMethod.Get);
        private static readonly ConditionResultDetail Rs1 = new ConditionResultDetail("status", true, "");
        private static readonly ConditionResultDetail Rs2 = new ConditionResultDetail("status", false, "Bad Gateway");

        public CheckResultTests()
        {
            var ck2 = new HttpCheck(2, Env, Srv, TimeSpan.FromMinutes(15), new Uri("http://remotehost"), new List<HttpCheckCondition> {Cnd}, HttpCheck.HttpMethod.Get);
            var rs3 = new ConditionResultDetail("status", true, "Good but slow");

            var now = DateTimeOffset.UtcNow;

            _checkResultA1 = CreateCheckResult();
            _checkResultA1Clone = new CheckResult(Ck1, now, CheckOutcome.Success, new List<ConditionResultDetail> {Rs1});
            _checkResultA2 = new CheckResult(Ck1, now.AddHours(1), CheckOutcome.Success, new List<ConditionResultDetail> {Rs1});
            _checkResultB1 = new CheckResult(ck2, now, CheckOutcome.Success, new List<ConditionResultDetail> {Rs1});
            _checkResultC1 = new CheckResult(Ck1, now.AddHours(1), CheckOutcome.Fail, new List<ConditionResultDetail> {Rs2});
            _checkResultD1 = new CheckResult(Ck1, now.AddHours(1), CheckOutcome.Success, new List<ConditionResultDetail> {rs3});
        }

        internal static CheckResult CreateCheckResult(DateTimeOffset? now = null, bool success = true)
        {
            var outcome = success ? CheckOutcome.Success : CheckOutcome.Fail;
            var res = success ? Rs1 : Rs2;
            return new CheckResult(Ck1, now ?? DateTimeOffset.UtcNow, outcome, new List<ConditionResultDetail> {res});
        }

        [Fact]
        public void SameStatus_Rejects_NullComparand()
        {
            Assert.Throws<ArgumentNullException>(() => _checkResultA1.SameStatus(null));
        }

        [Fact]
        public void SameStatus_ReturnsTrue_ForSecondResult()
        {
            Assert.True(_checkResultA1.SameStatus(_checkResultA2));
        }

        [Fact]
        public void SameStatus_ReturnsTrue_ForSameInstance()
        {
            Assert.True(_checkResultA1.SameStatus(_checkResultA1));
        }

        [Fact]
        public void SameStatus_ReturnsTrue_ForClone()
        {
            Assert.True(_checkResultA1.SameStatus(_checkResultA1Clone));
        }

        [Fact]
        public void SameStatus_ReturnsFalse_ForDifferentCheck()
        {
            Assert.False(_checkResultA1.SameStatus(_checkResultB1));
        }

        [Fact]
        public void SameStatus_ReturnsFalse_ForDifferentOutcome()
        {
            Assert.False(_checkResultA1.SameStatus(_checkResultC1));
        }

        [Fact]
        public void SameStatus_ReturnsFalse_ForOtherDetails()
        {
            Assert.False(_checkResultA1.SameStatus(_checkResultD1));
        }
    }
}