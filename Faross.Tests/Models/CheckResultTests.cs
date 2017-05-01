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

        public CheckResultTests()
        {
            var env = new Environment("env", 1);
            var srv = new Service(1, "srv", new List<Environment> {env});
            var cnd = new HttpStatusCondition("status", true, HttpStatusCondition.Operator.Equal, 200);
            var ck1 = new HttpCheck(1, env, srv, TimeSpan.FromMinutes(15), new Uri("http://localhost"), new List<HttpCheckCondition> {cnd}, HttpCheck.HttpMethod.Get);
            var ck2 = new HttpCheck(2, env, srv, TimeSpan.FromMinutes(15), new Uri("http://remotehost"), new List<HttpCheckCondition> {cnd}, HttpCheck.HttpMethod.Get);
            var rs1 = new ConditionResultDetail("status", true, "");
            var rs2 = new ConditionResultDetail("status", false, "Bad Gateway");
            var rs3 = new ConditionResultDetail("status", true, "Good but slow");

            var now = DateTimeOffset.UtcNow;
            
            _checkResultA1 = new CheckResult(ck1, now, CheckOutcome.Success, new List<ConditionResultDetail> {rs1});
            _checkResultA1Clone = new CheckResult(ck1, now, CheckOutcome.Success, new List<ConditionResultDetail> {rs1});
            _checkResultA2 = new CheckResult(ck1, now.AddHours(1), CheckOutcome.Success, new List<ConditionResultDetail> {rs1});
            _checkResultB1 = new CheckResult(ck2, now, CheckOutcome.Success, new List<ConditionResultDetail> {rs1});
            _checkResultC1 = new CheckResult(ck1, now.AddHours(1), CheckOutcome.Fail, new List<ConditionResultDetail>{rs2});
            _checkResultD1 = new CheckResult(ck1, now.AddHours(1), CheckOutcome.Success, new List<ConditionResultDetail>{rs3});
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