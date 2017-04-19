using System;
using Faross.Util;
using Xunit;

namespace Faross.Tests.Util
{
    public class HashCodeUtilTests
    {
        public enum ArrayType
        {
            Null,
            Empty,
            OnlyNulls,
            SomeNulls,
            NoNulls
        }

        private readonly object[] _empty;
        private readonly object[] _onlyNulls;
        private readonly object[] _someNulls;
        private readonly object[] _noNulls;

        public HashCodeUtilTests()
        {
            _empty = new object[] { };
            _onlyNulls = new object[] {null, null};
            _someNulls = new object[] {null, "Ion", "Andrei", null};
            _noNulls = new object[] {"Ion", "Andrei"};
        }

        [Theory]
        [InlineData(ArrayType.Null)]
        [InlineData(ArrayType.Empty)]
        [InlineData(ArrayType.OnlyNulls)]
        public void GetCombinedHash_ReturnsZero_ForNullishArray(ArrayType type)
        {
            var arr = (object[]) null;
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (type)
            {
                case ArrayType.Empty:
                    arr = _empty;
                    break;
                case ArrayType.OnlyNulls:
                    arr = _onlyNulls;
                    break;
            }
            var hash = HashCodeUtil.GetCombinedHash(arr);
            Assert.Equal(0, hash);
        }

        [Theory]
        [InlineData(ArrayType.SomeNulls)]
        [InlineData(ArrayType.NoNulls)]
        public void GetCombinedHash_ReturnsNonZero_ForNonNullishArray(ArrayType type)
        {
            object[] arr;
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (type)
            {
                case ArrayType.SomeNulls:
                    arr = _someNulls;
                    break;
                case ArrayType.NoNulls:
                    arr = _noNulls;
                    break;
                default: throw new InvalidOperationException();
            }
            var hash = HashCodeUtil.GetCombinedHash(arr);
            Assert.NotEqual(0, hash);
        }
    }
}