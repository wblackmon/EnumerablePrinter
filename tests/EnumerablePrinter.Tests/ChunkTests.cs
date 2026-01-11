using Xunit;

public class ChunkTests
{
    [Fact]
    public void Chunk_SplitsSequenceIntoCorrectSizedChunks()
    {
        // Arrange
        var numbers = Enumerable.Range(1, 7); // 1..7

        // Act
        var chunks = numbers.Chunk(3).ToList();

        // Assert
        Assert.Equal(3, chunks.Count);

        Assert.Equal(new[] { 1, 2, 3 }, chunks[0]);
        Assert.Equal(new[] { 4, 5, 6 }, chunks[1]);
        Assert.Equal(new[] { 7 }, chunks[2]); // last chunk smaller
    }
}
