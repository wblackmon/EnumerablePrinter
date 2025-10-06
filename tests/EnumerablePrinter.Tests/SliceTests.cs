namespace EnumerablePrinter.Tests
{
    public class SliceTests
    {
        [Fact]
        public void Slice_FullSequence_WhenNoArgs_ReturnsAll()
        {
            var data = Enumerable.Range(1, 5);
            var result = data.Slice().ToList();

            Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result);
        }

        [Fact]
        public void Slice_WithStartAndEnd_ReturnsCorrectRange()
        {
            var data = Enumerable.Range(1, 10);
            var result = data.Slice(2, 5).ToList();

            Assert.Equal(new[] { 3, 4, 5 }, result);
        }

        [Fact]
        public void Slice_WithNegativeStart_ReturnsFromEnd()
        {
            var data = Enumerable.Range(1, 10);
            var result = data.Slice(-3, null).ToList();

            Assert.Equal(new[] { 8, 9, 10 }, result);
        }

        [Fact]
        public void Slice_WithNegativeEnd_ReturnsCorrectRange()
        {
            var data = Enumerable.Range(1, 10);
            var result = data.Slice(null, -5).ToList();

            Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result);
        }

        [Fact]
        public void Slice_WithStep_ReturnsEveryNthElement()
        {
            var data = Enumerable.Range(1, 10);
            var result = data.Slice(0, null, 2).ToList();

            Assert.Equal(new[] { 1, 3, 5, 7, 9 }, result);
        }

        [Fact]
        public void Slice_EmptyRange_ReturnsEmpty()
        {
            var data = Enumerable.Range(1, 10);
            var result = data.Slice(5, 2).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void Slice_StartBeyondEnd_ReturnsEmpty()
        {
            var data = Enumerable.Range(1, 5);
            var result = data.Slice(10, 20).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void Slice_StepLessThanOrEqualToZero_Throws()
        {
            var data = Enumerable.Range(1, 5);

            Assert.Throws<ArgumentOutOfRangeException>(() => data.Slice(0, 5, 0).ToList());
            Assert.Throws<ArgumentOutOfRangeException>(() => data.Slice(0, 5, -1).ToList());
        }

        [Fact]
        public void Slice_NullSource_Throws()
        {
            IEnumerable<int>? data = null;
            Assert.Throws<ArgumentNullException>(() => data!.Slice().ToList());
        }
    }
}