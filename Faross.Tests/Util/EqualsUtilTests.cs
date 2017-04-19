using System.Collections.Generic;
using System.Linq;
using Faross.Util;
using Xunit;

namespace Faross.Tests.Util
{
    public class EqualsUtilTests
    {
        public class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as Person;
                return other != null && other.Id == Id && other.Name == Name;
            }

            public override int GetHashCode()
            {
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                return Id;
            }
        }

        // ReSharper disable InconsistentNaming
        private readonly Person Ion = new Person {Id = 1, Name = "Ion"};

        private readonly Person Andrei = new Person {Id = 2, Name = "Andrei"};

        private readonly Person Petru = new Person {Id = 3, Name = "Petru"};
        // ReSharper restore InconsistentNaming

        private readonly IReadOnlyCollection<Person> _one;
        private readonly IReadOnlyCollection<Person> _anotherOne;
        private readonly IReadOnlyCollection<Person> _yetAnotherOne;
        private readonly IReadOnlyCollection<Person> _yetYetAnotherOne;
        private readonly IReadOnlyCollection<Person> _two;
        private readonly IReadOnlyCollection<Person> _all;

        public EqualsUtilTests()
        {
            _one = new[] {Ion, Andrei};
            _anotherOne = new[] {Ion, Andrei};
            _yetAnotherOne = new[]
            {
                new Person {Id = 1, Name = "Ion"},
                new Person {Id = 2, Name = "Andrei"}
            };
            _yetYetAnotherOne = new[] {Andrei, Ion};
            _two = new[] {Andrei, Petru};
            _all = new[] {Ion, Andrei, Petru};
        }

        [Theory]
        [InlineData(NullParams.First)]
        [InlineData(NullParams.Second)]
        [InlineData(NullParams.Both)]
        public void Equivalent_ReturnsFalse_ForNullCollections(NullParams @params)
        {
            var first = @params == NullParams.First || @params == NullParams.Both ? null : _one;
            var second = @params == NullParams.Second || @params == NullParams.Both ? null : _one;
            Assert.False(first.Equivalent(second));
        }

        [Fact]
        public void Equivalent_ReturnsTrue_ForSameInstance()
        {
            Assert.True(_one.Equivalent(_one));
        }

        [Fact]
        public void Equivalent_ReturnsFalse_ForDifferentSized()
        {
            Assert.False(_one.Equivalent(_all));
        }

        [Fact]
        public void Equivalent_ReturnsTrue_ForEquivalentCollections()
        {
            var r1 = _one.Equivalent(_anotherOne);
            var r2 = _one.Equivalent(_yetAnotherOne);
            var r3 = _one.Equivalent(_yetYetAnotherOne);
            Assert.True(r1, "r1");
            Assert.True(r2, "r2");
            Assert.True(r3, "r3");
        }

        [Theory]
        [InlineData(NullParams.First)]
        [InlineData(NullParams.Second)]
        [InlineData(NullParams.Both)]
        public void ArraysEqual_ReturnsNull_ForNullArrays(NullParams @params)
        {
            var first = @params == NullParams.First || @params == NullParams.Both ? null : _one.ToArray();
            var second = @params == NullParams.Second || @params == NullParams.Both ? null : _one.ToArray();
            Assert.False(first.ArraysEqual(second));
        }

        [Fact]
        public void ArraysEqual_ReturnsFalse_ForDifferentlySizedArrays()
        {
            var first = new[] {1, 2};
            var second = new[] {1, 2, 3};
            Assert.False(first.ArraysEqual(second));
        }

        [Fact]
        public void ArraysEqual_ReturnsTrue_ForSameInstance()
        {
            var arr = _one.ToArray();
            Assert.True(arr.ArraysEqual(arr));
        }

        [Fact]
        public void ArraysEqual_ReturnsTrue_ForSameContents()
        {
            var one = _one.ToArray();
            var two = _yetAnotherOne.ToArray();
            Assert.True(one.ArraysEqual(two));
        }

        [Fact]
        public void ArraysEqual_ReturnsFalse_DifferentOrderSameContents()
        {
            var one = new[] {Ion, Andrei};
            var two = new[] {Andrei, Ion};
            Assert.False(one.ArraysEqual(two));
        }
    }
}