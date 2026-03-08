using ExactHull;
using ExactHull.ExactGeometry;
using Xunit;

namespace ExactHull.Tests;

public sealed class BuildHullOutsideSetsTests
{
    [Fact]
    public void BuildsInitialTetrahedron()
    {
        var hull = ExactHull3D.Build(
            (0.0, 0.0, 0.0),
            (1.0, 0.0, 0.0),
            (0.0, 1.0, 0.0),
            (0.0, 0.0, 1.0));

        Assert.Equal(4, hull.Faces.Length);
        Assert.True(ExactHullValidation3D.IsHullValid(hull.Points, hull.Faces));
    }

    [Fact]
    public void IgnoresInteriorPoint()
    {
        var hull = ExactHull3D.Build(
            (0.0, 0.0, 0.0),
            (1.0, 0.0, 0.0),
            (0.0, 1.0, 0.0),
            (0.0, 0.0, 1.0),
            (0.1, 0.1, 0.1));

        Assert.Equal(4, hull.Faces.Length);
        Assert.True(ExactHullValidation3D.IsHullValid(hull.Points, hull.Faces));
    }

    [Fact]
    public void ExpandsForExteriorPoint()
    {
        var hull = ExactHull3D.Build(
            (0.0, 0.0, 0.0),
            (1.0, 0.0, 0.0),
            (0.0, 1.0, 0.0),
            (0.0, 0.0, 1.0),
            (0.25, 0.25, -1.0));

        Assert.Equal(6, hull.Faces.Length);
        Assert.True(ExactHullValidation3D.IsHullValid(hull.Points, hull.Faces));
    }

    [Fact]
    public void HandlesDuplicates()
    {
        var hull = ExactHull3D.Build(
            (0.0, 0.0, 0.0),
            (0.0, 0.0, 0.0),
            (1.0, 0.0, 0.0),
            (0.0, 1.0, 0.0),
            (0.0, 0.0, 1.0),
            (0.1, 0.1, 0.1),
            (1.0, 0.0, 0.0));

        Assert.Equal(4, hull.Faces.Length);
        Assert.True(ExactHullValidation3D.IsHullValid(hull.Points, hull.Faces));
    }

    [Fact]
    public void CubeProducesValidHull()
    {
        var hull = ExactHull3D.Build(
            (0.0, 0.0, 0.0),
            (1.0, 0.0, 0.0),
            (1.0, 1.0, 0.0),
            (0.0, 1.0, 0.0),
            (0.0, 0.0, 1.0),
            (1.0, 0.0, 1.0),
            (1.0, 1.0, 1.0),
            (0.0, 1.0, 1.0));

        Assert.Equal(12, hull.Faces.Length);
        Assert.True(ExactHullValidation3D.IsHullValid(hull.Points, hull.Faces));
    }

    [Fact]
    public void OctahedronProducesValidHull()
    {
        var hull = ExactHull3D.Build(
            ( 1.0,  0.0,  0.0),
            (-1.0,  0.0,  0.0),
            ( 0.0,  1.0,  0.0),
            ( 0.0, -1.0,  0.0),
            ( 0.0,  0.0,  1.0),
            ( 0.0,  0.0, -1.0));

        Assert.Equal(8, hull.Faces.Length);
        Assert.True(ExactHullValidation3D.IsHullValid(hull.Points, hull.Faces));
    }
}