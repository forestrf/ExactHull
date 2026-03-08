using ExactHull.ExactGeometry;
using Xunit;

namespace ExactHull.Tests;

public sealed class InitialTetrahedronTests
{
    [Fact]
    public void ReturnsFalse_ForEmptyInput()
    {
        var points = Array.Empty<Exact3>();

        bool success = ExactHullBuilder3D.TryFindInitialTetrahedron(
            points, out int i0, out int i1, out int i2, out int i3);

        Assert.False(success);
        Assert.Equal(-1, i0);
        Assert.Equal(-1, i1);
        Assert.Equal(-1, i2);
        Assert.Equal(-1, i3);
    }

    [Fact]
    public void ReturnsFalse_ForSinglePoint()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0)
        };

        bool success = ExactHullBuilder3D.TryFindInitialTetrahedron(
            points, out _, out _, out _, out _);

        Assert.False(success);
    }

    [Fact]
    public void ReturnsFalse_WhenAllPointsAreEqual()
    {
        var points = new[]
        {
            new Exact3(1.0, 2.0, 3.0),
            new Exact3(1.0, 2.0, 3.0),
            new Exact3(1.0, 2.0, 3.0),
            new Exact3(1.0, 2.0, 3.0)
        };

        bool success = ExactHullBuilder3D.TryFindInitialTetrahedron(
            points, out _, out _, out _, out _);

        Assert.False(success);
    }

    [Fact]
    public void ReturnsFalse_WhenAllPointsAreCollinear()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(2.0, 0.0, 0.0),
            new Exact3(3.0, 0.0, 0.0),
            new Exact3(4.0, 0.0, 0.0)
        };

        bool success = ExactHullBuilder3D.TryFindInitialTetrahedron(
            points, out _, out _, out _, out _);

        Assert.False(success);
    }

    [Fact]
    public void ReturnsFalse_WhenAllPointsAreCoplanar()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(1.0, 1.0, 0.0),
            new Exact3(0.25, 0.25, 0.0)
        };

        bool success = ExactHullBuilder3D.TryFindInitialTetrahedron(
            points, out _, out _, out _, out _);

        Assert.False(success);
    }

    [Fact]
    public void FindsSimpleTetrahedron()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0)
        };

        bool success = ExactHullBuilder3D.TryFindInitialTetrahedron(
            points, out int i0, out int i1, out int i2, out int i3);

        Assert.True(success);
        AssertAllDistinct(i0, i1, i2, i3);

        Exact orient = ExactGeometry3D.Orient3D(
            points[i0], points[i1], points[i2], points[i3]);

        Assert.False(orient.IsZero());
    }

    [Fact]
    public void FindsTetrahedron_WithDuplicatesMixedIn()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0),
            new Exact3(0.25, 0.25, 0.25)
        };

        bool success = ExactHullBuilder3D.TryFindInitialTetrahedron(
            points, out int i0, out int i1, out int i2, out int i3);

        Assert.True(success);
        AssertAllDistinct(i0, i1, i2, i3);

        Exact orient = ExactGeometry3D.Orient3D(
            points[i0], points[i1], points[i2], points[i3]);

        Assert.False(orient.IsZero());
    }

    private static void AssertAllDistinct(int i0, int i1, int i2, int i3)
    {
        Assert.NotEqual(i0, i1);
        Assert.NotEqual(i0, i2);
        Assert.NotEqual(i0, i3);
        Assert.NotEqual(i1, i2);
        Assert.NotEqual(i1, i3);
        Assert.NotEqual(i2, i3);
    }
}
