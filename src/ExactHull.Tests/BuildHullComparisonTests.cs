using ExactHull;
using ExactHull.ExactGeometry;
using Xunit;

namespace ExactHull.Tests;

public sealed class BuildHullComparisonTests
{
    [Fact]
    public void OldAndNewBuildersBothProduceValidHulls()
    {
        var random = new Random(424242);

        for (int test = 0; test < 100; test++)
        {
            int pointCount = random.Next(12, 60);
            var points = new (double X, double Y, double Z)[pointCount];

            for (int i = 0; i < pointCount; i++)
            {
                points[i] = (
                    random.NextDouble() * 20.0 - 10.0,
                    random.NextDouble() * 20.0 - 10.0,
                    random.NextDouble() * 20.0 - 10.0);
            }

            var hullA = ExactHull3D.BuildBruteForce(points);
            var hullB = ExactHull3D.Build(points);

            Assert.True(ExactHullValidation3D.IsHullValid(hullA.Points, hullA.Faces));
            Assert.True(ExactHullValidation3D.IsHullValid(hullB.Points, hullB.Faces));
        }
    }
}