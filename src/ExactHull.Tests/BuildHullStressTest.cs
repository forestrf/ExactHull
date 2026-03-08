using ExactHull.ExactGeometry;
using Xunit;

namespace ExactHull.Tests;

public sealed class BuildHullStressTests
{
    [Fact]
    public void CubeCorners_ProducesValidHull()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0), // 0
            new Exact3(1.0, 0.0, 0.0), // 1
            new Exact3(1.0, 1.0, 0.0), // 2
            new Exact3(0.0, 1.0, 0.0), // 3
            new Exact3(0.0, 0.0, 1.0), // 4
            new Exact3(1.0, 0.0, 1.0), // 5
            new Exact3(1.0, 1.0, 1.0), // 6
            new Exact3(0.0, 1.0, 1.0), // 7
        };

        bool success = ExactHullBuilder3D.TryBuildHull(points, out Face[] faces, out int faceCount);

        Assert.True(success);
        Assert.True(faceCount > 0);
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces[..faceCount]));

        // A cube triangulates to 12 triangles if all coplanar face points are handled nicely.
        // If this currently fails later, that is a useful signal.
        Assert.Equal(12, faceCount);
    }

    [Fact]
    public void Octahedron_ProducesValidHull()
    {
        var points = new[]
        {
            new Exact3( 1.0,  0.0,  0.0),
            new Exact3(-1.0,  0.0,  0.0),
            new Exact3( 0.0,  1.0,  0.0),
            new Exact3( 0.0, -1.0,  0.0),
            new Exact3( 0.0,  0.0,  1.0),
            new Exact3( 0.0,  0.0, -1.0),
        };

        bool success = ExactHullBuilder3D.TryBuildHull(points, out Face[] faces, out int faceCount);

        Assert.True(success);
        Assert.Equal(8, faceCount);
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces[..faceCount]));
    }

    [Fact]
    public void Duplicates_DoNotBreakHull()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0),

            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0),

            new Exact3(0.2, 0.2, 0.2),
            new Exact3(0.2, 0.2, 0.2),
        };

        bool success = ExactHullBuilder3D.TryBuildHull(points, out Face[] faces, out int faceCount);

        Assert.True(success);
        Assert.Equal(4, faceCount);
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces[..faceCount]));
    }

    [Fact]
    public void PointsOnFacesAndEdges_DoNotBreakHull()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0),

            new Exact3(0.5, 0.0, 0.0), // edge point
            new Exact3(0.0, 0.5, 0.0), // edge point
            new Exact3(0.5, 0.5, 0.0), // face point
            new Exact3(0.2, 0.2, 0.2), // interior point
        };

        bool success = ExactHullBuilder3D.TryBuildHull(points, out Face[] faces, out int faceCount);

        Assert.True(success);
        Assert.Equal(4, faceCount);
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces[..faceCount]));
    }

    [Fact]
    public void RandomPointCloud_ProducesValidHull()
    {
        var random = new Random(12345);
        Exact3[] points = new Exact3[32];

        for (int i = 0; i < points.Length; i++)
        {
            double x = random.NextDouble() * 2.0 - 1.0;
            double y = random.NextDouble() * 2.0 - 1.0;
            double z = random.NextDouble() * 2.0 - 1.0;
            points[i] = new Exact3(x, y, z);
        }

        bool success = ExactHullBuilder3D.TryBuildHull(points, out Face[] faces, out int faceCount);

        Assert.True(success);
        Assert.True(faceCount >= 4);
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces[..faceCount]));
    }
    
    [Fact]
    public void ManyRandomPointClouds_ProduceValidHulls()
    {
        var random = new Random(12345);

        for (int test = 0; test < 100; test++)
        {
            Exact3[] points = new Exact3[32];

            for (int i = 0; i < points.Length; i++)
            {
                double x = random.NextDouble() * 2.0 - 1.0;
                double y = random.NextDouble() * 2.0 - 1.0;
                double z = random.NextDouble() * 2.0 - 1.0;
                points[i] = new Exact3(x, y, z);
            }

            bool success = ExactHullBuilder3D.TryBuildHull(points, out Face[] faces, out int faceCount);

            Assert.True(success);
            Assert.True(faceCount >= 4);
            Assert.True(ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)));
        }
    }
}
