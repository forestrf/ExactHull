using ExactHull.ExactGeometry;
using Xunit;

namespace ExactHull.Tests;

public sealed class NearlyCoplanarBurnTests
{
    [Fact]
    public void ManyNearlyCoplanarPointClouds_ProduceValidHulls()
    {
        var random = new Random(24681357);

        for (int test = 0; test < 300; test++)
        {
            int pointCount = random.Next(16, 80);
            var points = new Exact3[pointCount];

            // Most points lie very close to the z=0 plane.
            for (int i = 0; i < pointCount - 2; i++)
            {
                double x = random.NextDouble() * 2000.0 - 1000.0;
                double y = random.NextDouble() * 2000.0 - 1000.0;
                double z = (random.NextDouble() * 2.0 - 1.0) * 1e-12;

                points[i] = new Exact3(x, y, z);
            }

            // Force the cloud to be truly 3D.
            points[pointCount - 2] = new Exact3(0.0, 0.0, 1e-6);
            points[pointCount - 1] = new Exact3(0.0, 0.0, -1e-6);

            InjectDuplicates(random, points);
            Shuffle(random, points);

            var faces = new Face[Math.Max(128, pointCount * 8)];

            bool success = ExactHullBruteForceBuilder3D.TryBuildHull(points, faces, out int faceCount);

            Assert.True(success, $"Hull build failed in nearly-coplanar test {test}.");
            Assert.True(faceCount >= 4, $"faceCount={faceCount} in nearly-coplanar test {test}");
            Assert.True(
                ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)),
                $"Hull validation failed in nearly-coplanar test {test}.");
        }
    }

    [Fact]
    public void LargeCoordinatesWithTinyOffsets_ProduceValidHulls()
    {
        var random = new Random(97531);

        for (int test = 0; test < 200; test++)
        {
            int pointCount = random.Next(16, 64);
            var points = new Exact3[pointCount];

            for (int i = 0; i < pointCount - 2; i++)
            {
                double x = random.NextDouble() * 2.0e9 - 1.0e9;
                double y = random.NextDouble() * 2.0e9 - 1.0e9;
                double z = (random.NextDouble() * 2.0 - 1.0) * 1e-9;

                points[i] = new Exact3(x, y, z);
            }

            points[pointCount - 2] = new Exact3(0.0, 0.0, 1.0);
            points[pointCount - 1] = new Exact3(0.0, 0.0, -1.0);

            Shuffle(random, points);

            var faces = new Face[Math.Max(128, pointCount * 8)];

            bool success = ExactHullBruteForceBuilder3D.TryBuildHull(points, faces, out int faceCount);

            Assert.True(success, $"Hull build failed in large-coordinate test {test}.");
            Assert.True(faceCount >= 4, $"faceCount={faceCount} in large-coordinate test {test}");
            Assert.True(
                ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)),
                $"Hull validation failed in large-coordinate test {test}.");
        }
    }

    private static void InjectDuplicates(Random random, Exact3[] points)
    {
        int duplicateCount = random.Next(0, Math.Min(8, points.Length / 4 + 1));

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
