using ExactHull.ExactGeometry;
using Xunit;

namespace ExactHull.Tests;

public sealed class BuildHullBurnTests
{
    [Fact]
    public void ManyRandomPointClouds_ProduceValidHulls()
    {
        var random = new Random(123456);

        for (int test = 0; test < 500; test++)
        {
            int pointCount = random.Next(8, 80);
            var points = new Exact3[pointCount];

            for (int i = 0; i < pointCount; i++)
            {
                points[i] = NextRandomPoint(random);
            }

            InjectDuplicates(random, points);
            Shuffle(random, points);

            var faces = new Face[Math.Max(64, pointCount * 8)];

            bool success = ExactHullBruteForceBuilder3D.TryBuildHull(points, faces, out int faceCount);

            if (!success)
            {
                // Random points in full 3D should almost never be degenerate.
                Assert.Fail($"Hull build failed unexpectedly in test {test}.");
            }

            Assert.True(faceCount >= 4, $"faceCount={faceCount} in test {test}");
            Assert.True(
                ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)),
                $"Hull validation failed in test {test}.");
        }
    }

    [Fact]
    public void ManyRandomSpherePointClouds_ProduceValidHulls()
    {
        var random = new Random(789012);

        for (int test = 0; test < 200; test++)
        {
            int pointCount = random.Next(12, 60);
            var points = new Exact3[pointCount];

            for (int i = 0; i < pointCount; i++)
            {
                points[i] = NextRandomSpherePoint(random);
            }

            Shuffle(random, points);

            var faces = new Face[Math.Max(64, pointCount * 8)];

            bool success = ExactHullBruteForceBuilder3D.TryBuildHull(points, faces, out int faceCount);

            Assert.True(success, $"Hull build failed unexpectedly in sphere test {test}.");
            Assert.True(faceCount >= 4, $"faceCount={faceCount} in sphere test {test}");
            Assert.True(
                ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)),
                $"Hull validation failed in sphere test {test}.");
        }
    }

    private static Exact3 NextRandomPoint(Random random)
    {
        double x = random.NextDouble() * 20.0 - 10.0;
        double y = random.NextDouble() * 20.0 - 10.0;
        double z = random.NextDouble() * 20.0 - 10.0;
        return new Exact3(x, y, z);
    }

    private static Exact3 NextRandomSpherePoint(Random random)
    {
        while (true)
        {
            double x = random.NextDouble() * 2.0 - 1.0;
            double y = random.NextDouble() * 2.0 - 1.0;
            double z = random.NextDouble() * 2.0 - 1.0;

            double lenSq = x * x + y * y + z * z;
            if (lenSq < 1e-12 || lenSq > 1.0)
                continue;

            double invLen = 1.0 / Math.Sqrt(lenSq);
            return new Exact3(x * invLen, y * invLen, z * invLen);
        }
    }

    private static void InjectDuplicates(Random random, Exact3[] points)
    {
        int duplicateCount = random.Next(0, Math.Min(6, points.Length / 4 + 1));

        for (int i = 0; i < duplicateCount; i++)
        {
            int src = random.Next(points.Length);
            int dst = random.Next(points.Length);
            points[dst] = points[src];
        }
    }

    private static void Shuffle(Random random, Exact3[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
