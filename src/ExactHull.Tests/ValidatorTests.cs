using ExactHull.ExactGeometry;
using Xunit;

namespace ExactHull.Tests;

/// <summary>
/// Tests that prove the hull validator itself catches invalid hulls.
/// A validator that accepts everything is useless.
/// </summary>
public sealed class ValidatorTests
{
    private static readonly Exact3[] TetraPoints =
    {
        new Exact3(0.0, 0.0, 0.0),
        new Exact3(1.0, 0.0, 0.0),
        new Exact3(0.0, 1.0, 0.0),
        new Exact3(0.0, 0.0, 1.0),
    };

    [Fact]
    public void ValidTetrahedron_Passes()
    {
        bool ok = ExactHullBuilder3D.TryBuildHull(TetraPoints, out var faces, out int fc);
        Assert.True(ok);
        Assert.True(ExactHullValidation3D.IsHullValid(TetraPoints, faces.AsSpan(0, fc)));
    }

    // ── The validator SHOULD reject these: ──

    [Fact]
    public void EmptyFaceList_ShouldBeRejected()
    {
        // An empty face array is NOT a valid convex hull
        bool valid = ExactHullValidation3D.IsHullValid(TetraPoints, ReadOnlySpan<Face>.Empty);
        Assert.False(valid);
    }

    [Fact]
    public void SingleTriangle_ShouldBeRejected()
    {
        // A single face is not a closed polyhedron
        var faces = new[] { new Face(0, 1, 2) };
        bool valid = ExactHullValidation3D.IsHullValid(TetraPoints, faces);
        Assert.False(valid);
    }

    [Fact]
    public void TwoFaces_ShouldBeRejected()
    {
        // Two faces can't form a closed polyhedron
        var faces = new[] { new Face(0, 1, 2), new Face(0, 2, 3) };
        bool valid = ExactHullValidation3D.IsHullValid(TetraPoints, faces);
        Assert.False(valid);
    }

    [Fact]
    public void ThreeFaces_ShouldBeRejected()
    {
        // Three faces can't form a closed polyhedron either (min is 4)
        var faces = new[] { new Face(0, 1, 2), new Face(0, 2, 3), new Face(0, 3, 1) };
        bool valid = ExactHullValidation3D.IsHullValid(TetraPoints, faces);
        Assert.False(valid);
    }

    [Fact]
    public void SubsetOfTetrahedronFaces_ShouldBeRejected()
    {
        // Build real hull, then drop a face — the remaining 3 are not a closed hull
        bool ok = ExactHullBuilder3D.TryBuildHull(TetraPoints, out var faces, out int fc);
        Assert.True(ok);
        Assert.Equal(4, fc);

        // Remove last face
        bool valid = ExactHullValidation3D.IsHullValid(TetraPoints, faces.AsSpan(0, fc - 1));
        Assert.False(valid);
    }

    [Fact]
    public void DuplicatedFace_ShouldBeRejected()
    {
        // Duplicating a face makes a non-manifold mesh
        bool ok = ExactHullBuilder3D.TryBuildHull(TetraPoints, out var faces, out int fc);
        Assert.True(ok);

        var badFaces = new Face[fc + 1];
        faces.AsSpan(0, fc).CopyTo(badFaces);
        badFaces[fc] = faces[0]; // duplicate first face

        bool valid = ExactHullValidation3D.IsHullValid(TetraPoints, badFaces);
        Assert.False(valid);
    }

    [Fact]
    public void FlippedWindingOnOneFace_ShouldBeRejected()
    {
        // Flip winding of one face — its normal points inward
        bool ok = ExactHullBuilder3D.TryBuildHull(TetraPoints, out var faces, out int fc);
        Assert.True(ok);

        // Swap B and C of first face to flip its normal
        faces[0] = new Face(faces[0].A, faces[0].C, faces[0].B);

        bool valid = ExactHullValidation3D.IsHullValid(TetraPoints, faces.AsSpan(0, fc));
        Assert.False(valid);
    }

    [Fact]
    public void PointOutsideHull_IsDetected()
    {
        // Build tetra hull, then add a point outside — validator should catch it
        var extended = new Exact3[5];
        TetraPoints.CopyTo(extended.AsSpan());
        extended[4] = new Exact3(1.0, 1.0, 1.0); // outside tetrahedron

        bool ok = ExactHullBuilder3D.TryBuildHull(TetraPoints, out var faces, out int fc);
        Assert.True(ok);

        // Validate the OLD faces against the EXTENDED point set
        bool valid = ExactHullValidation3D.IsHullValid(extended, faces.AsSpan(0, fc));
        Assert.False(valid);
    }

    [Fact]
    public void DegenerateFace_IsDetected()
    {
        var faces = new[] { new Face(0, 0, 1), new Face(0, 1, 2), new Face(0, 2, 3), new Face(1, 2, 3) };
        bool valid = ExactHullValidation3D.IsHullValid(TetraPoints, faces);
        Assert.False(valid);
    }

    [Fact]
    public void OutOfBoundsIndex_IsDetected()
    {
        var faces = new[] { new Face(0, 1, 99) };
        bool valid = ExactHullValidation3D.IsHullValid(TetraPoints, faces);
        Assert.False(valid);
    }

    [Fact]
    public void NonManifoldEdge_ShouldBeRejected()
    {
        // Three faces sharing the same edge = non-manifold
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0),
            new Exact3(0.0, 0.0, -1.0),
        };

        // Build valid hull of first 4 points, then tack on an extra face sharing edge 0-1
        bool ok = ExactHullBuilder3D.TryBuildHull(points[..4], out var faces, out int fc);
        Assert.True(ok);

        var badFaces = new Face[fc + 1];
        faces.AsSpan(0, fc).CopyTo(badFaces);
        badFaces[fc] = new Face(0, 1, 4); // shares edge 0-1 with existing face

        bool valid = ExactHullValidation3D.IsHullValid(points, badFaces);
        Assert.False(valid);
    }
}
