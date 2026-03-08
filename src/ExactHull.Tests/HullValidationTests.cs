using ExactHull.ExactGeometry;
using Xunit;

namespace ExactHull.Tests;

public sealed class HullValidationTests
{
    [Fact]
    public void ValidatesInitialTetrahedron()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0),
        };

        Span<Face> faces = stackalloc Face[16];
        bool success = ExactHullBruteForceBuilder3D.TryBuildHull(points, faces, out int faceCount);

        Assert.True(success);
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces[..faceCount]));
    }

    [Fact]
    public void ValidatesHullWithInteriorPoint()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0),
            new Exact3(0.1, 0.1, 0.1),
        };

        Span<Face> faces = stackalloc Face[32];
        bool success = ExactHullBruteForceBuilder3D.TryBuildHull(points, faces, out int faceCount);

        Assert.True(success);
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces[..faceCount]));
    }

    [Fact]
    public void ValidatesHullWithExteriorPoint()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0),
            new Exact3(0.25, 0.25, -1.0),
        };

        Span<Face> faces = stackalloc Face[32];
        bool success = ExactHullBruteForceBuilder3D.TryBuildHull(points, faces, out int faceCount);

        Assert.True(success);
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces[..faceCount]));
    }

    [Fact]
    public void RejectsFaceWithInvalidIndex()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0),
        };

        var faces = new[]
        {
            new Face(0, 1, 99)
        };

        Assert.False(ExactHullValidation3D.IsHullValid(points, faces));
    }

    [Fact]
    public void RejectsDegenerateFace()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0),
        };

        var faces = new[]
        {
            new Face(0, 1, 1)
        };

        Assert.False(ExactHullValidation3D.IsHullValid(points, faces));
    }

    [Fact]
    public void RejectsFacePointingInward()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0),
        };

        var faces = new[]
        {
            new Face(0, 1, 2) // opposite point 3 lies on positive side for this winding
        };

        Assert.False(ExactHullValidation3D.IsHullValid(points, faces));
    }
}
