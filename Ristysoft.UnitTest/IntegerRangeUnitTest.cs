namespace Ristysoft.UnitTest
{
    public class IntegerRangeUnitTest
    {
        [Theory]
        [InlineData("1-4, 7-9, 10-11", "1-4,7-11", 9, new int[] { 1, 4, 7, 11 }, new int[] { 0, -1, 5, 6, 12 })]
        [InlineData("1-2, 0-9, 10-11", "0-11", 12, new int[] { 0, 3, 9, 10, 11 }, new int[] { -1, -2, -5, 12, 13 })]
        [InlineData("-100--50,-10-10, 11, 12, 13, 5, 3, 1", "-100--50,-10-13", 75, new int[] { -100, -99, -50, -10, 0, 10, 11, 12, 13, 1 }, new int[] { -101, -49, -11, 14, 15, 16, 17 })]
        [InlineData("1-10,-5--4,100-150,80-90,91-99", "-5--4,1-10,80-150", 83, new int[] { 1, 81, 101, -4, -5, 91 }, new int[] { 0, -1, 12, 79, 151 })]
        public void RangeIncludeTest(string ranges, string result, int count, int[] positiveTestCases, int[] negativeTestCases)
        {
            IntegerRanges intRanges = IntegerRanges.Parse(ranges);
            Assert.Equal(result, intRanges.ToString());
            Assert.Equal(count, intRanges.GetCount());
            Assert.Equal(count, intRanges.ToArray().Length);
            foreach (var i in positiveTestCases)
                Assert.True(intRanges.Included(i));
            foreach (var i in negativeTestCases)
                Assert.False(intRanges.Included(i));
        }

        [Theory]
        [InlineData("1-4, 7-9, 10-11", new int[] { 1, 2, 3, 4, 7, 8, 9, 10, 11 })]
        [InlineData("-3-4, 7-9, 11-13", new int[] { -3, -2, -1, 0, 1, 2, 3, 4, 7, 8, 9, 11, 12, 13 })]
        public void RangeArrayTest(string ranges, int[] result)
        {
            IntegerRanges intRanges = IntegerRanges.Parse(ranges);
            int[] arr = intRanges.ToArray();
            Assert.Equal(result.Length, arr.Length);
            Assert.True(result.SequenceEqual(arr));
        }

        [Theory]
        [InlineData("4-1, 7-9, 10-11", new int[] { 1, 2, 3, 4, 7, 8, 9, 10, 11 })]
        [InlineData("4--3, 7-9, 13-11", new int[] { -3, -2, -1, 0, 1, 2, 3, 4, 7, 8, 9, 11, 12, 13 })]
        public void RangeInverseTest(string ranges, int[] result)
        {
            IntegerRanges intRanges = IntegerRanges.Parse(ranges);
            int[] arr = intRanges.ToArray();
            Assert.Equal(result.Length, arr.Length);
            Assert.True(result.SequenceEqual(arr));
        }

        [Fact]
        public void RangeInvalidTest()
        {
            Assert.Throws<FormatException>(() => IntegerRanges.Parse("1-3.1,4-5"));
        }

        [Fact]
        public void RangeRemoveTest()
        {
            var intRanges = IntegerRanges.Parse("1-4, 7-9, 10-11");
            intRanges.RemoveRange(3, 8);
            Assert.Equal("1-2,9-11", intRanges.ToString());

            intRanges = IntegerRanges.Parse("1-4, 7-9, 10-11");
            intRanges.RemoveRange(1, 100);
            Assert.Equal("", intRanges.ToString());

            intRanges = IntegerRanges.Parse("1-4, 7-9, 10-11");
            intRanges.RemoveRange(6, 9);
            Assert.Equal("1-4,10-11", intRanges.ToString());
        }
    }
}