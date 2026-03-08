using ExactHull.ExactGeometry;
using Xunit;

namespace ExactHull.Tests;

public sealed class BuildHullTests
{
    [Fact]
    public void ReturnsFalse_ForTooFewPoints()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
        };

        bool success = ExactHullBuilder3D.TryBuildHull(points, out Face[] faces, out int faceCount);

        Assert.False(success);
        Assert.Equal(0, faceCount);
    }

    [Fact]
    public void ReturnsFalse_ForCoplanarPoints()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(1.0, 1.0, 0.0),
            new Exact3(0.25, 0.25, 0.0),
        };

        bool success = ExactHullBuilder3D.TryBuildHull(points, out Face[] faces, out int faceCount);

        Assert.False(success);
        Assert.Equal(0, faceCount);
    }

    [Fact]
    public void BuildsInitialTetrahedron()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0),
        };

        bool success = ExactHullBuilder3D.TryBuildHull(points, out Face[] faces, out int faceCount);

        Assert.True(success);
        Assert.Equal(4, faceCount);
    }

    [Fact]
    public void IgnoresInteriorPoint()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0), // 0
            new Exact3(1.0, 0.0, 0.0), // 1
            new Exact3(0.0, 1.0, 0.0), // 2
            new Exact3(0.0, 0.0, 1.0), // 3
            new Exact3(0.1, 0.1, 0.1), // 4 interior
        };

        bool success = ExactHullBuilder3D.TryBuildHull(points, out Face[] faces, out int faceCount);

        Assert.True(success);
        Assert.Equal(4, faceCount);
    }

    [Fact]
    public void ExpandsForExteriorPoint()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0), // 0
            new Exact3(1.0, 0.0, 0.0), // 1
            new Exact3(0.0, 1.0, 0.0), // 2
            new Exact3(0.0, 0.0, 1.0), // 3
            new Exact3(0.25, 0.25, -1.0), // 4 exterior
        };

        bool success = ExactHullBuilder3D.TryBuildHull(points, out Face[] faces, out int faceCount);

        Assert.True(success);
        Assert.Equal(6, faceCount);
        Assert.Equal(3, CountFacesUsingVertex(faces[..faceCount], 4));
    }

    [Fact]
    public void HandlesDuplicates()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0),
            new Exact3(0.1, 0.1, 0.1),
            new Exact3(1.0, 0.0, 0.0),
        };

        bool success = ExactHullBuilder3D.TryBuildHull(points, out Face[] faces, out int faceCount);

        Assert.True(success);
        Assert.Equal(4, faceCount);
    }

    private static int CountFacesUsingVertex(ReadOnlySpan<Face> faces, int vertex)
    {
        int count = 0;

        for (int i = 0; i < faces.Length; i++)
        {
            if (faces[i].A == vertex || faces[i].B == vertex || faces[i].C == vertex)
                count++;
        }

        return count;
    }
}
