using ExactHull.ExactGeometry;
using Xunit;

namespace ExactHull.Tests;

public sealed class BuildHullOrderTests
{
    [Fact]
    public void RandomPointCloud_RemainsValidUnderInputPermutation()
    {
        var random = new Random(12345);

        Exact3[] original = new Exact3[32];
        for (int i = 0; i < original.Length; i++)
        {
            double x = random.NextDouble() * 2.0 - 1.0;
            double y = random.NextDouble() * 2.0 - 1.0;
            double z = random.NextDouble() * 2.0 - 1.0;
            original[i] = new Exact3(x, y, z);
        }

        Exact3[] shuffled = new Exact3[original.Length];

        for (int run = 0; run < 50; run++)
        {
            Array.Copy(original, shuffled, original.Length);
            Shuffle(shuffled, random);

            bool success = ExactHullBuilder3D.TryBuildHull(shuffled, out Face[] faces, out int faceCount);

            Assert.True(success);
            Assert.True(faceCount >= 4);
            Assert.True(ExactHullValidation3D.IsHullValid(shuffled, faces.AsSpan(0, faceCount)));
        }
    }

    private static void Shuffle(Exact3[] array, Random random)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
