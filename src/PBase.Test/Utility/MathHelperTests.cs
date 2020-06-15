using System;
using PBase.Utility;
using Xunit;

namespace PBase.Test.Utility
{
    public class MathHelperTests
    {
        [Fact]
        public void TestClampWithValueWithinBounds()
        {
            //Value is within than the bounds and should remain unchanged after being clamped
            var value = 0;
            var upperBound = 256;
            var lowerBound = -256;
            var clampedValue = value.Clamp(lowerBound, upperBound);
            Assert.Equal(value, clampedValue);
        }

        [Fact]
        public void TestClampWithValueGreaterThanMax()
        {
            //value to clamp is greater than than max value; the clamped value should equal the max value
            var value = 256;
            var max = 0;
            var min = -256;
            var clampedValue = value.Clamp(min, max);
            Assert.Equal(max, clampedValue);
        }

        [Fact]
        public void TestClampWithValueLessThanMin()
        {
            //value to clamp is less than min value; the clamped value should equal the min value
            var value = -256;
            var max = 256;
            var min = 0;
            var clampedValue = value.Clamp(min, max);
            Assert.Equal(min, clampedValue);
        }

        [Fact]
        public void TestClampWithMaxLessThanMin()
        {
            //max value is less than min value; an exception should be thrown
            var value = 0;
            var max = -256;
            var min = 256;
            Assert.Throws<ArgumentException>(() => { var clampedValue = value.Clamp(min, max); });
        }
    }
}