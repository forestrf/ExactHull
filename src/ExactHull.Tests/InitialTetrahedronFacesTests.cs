using ExactHull.ExactGeometry;
using Xunit;

namespace ExactHull.Tests;

public sealed class InitialTetrahedronFacesTests
{
    [Fact]
    public void CreateInitialTetrahedronFaces_CreatesFourFaces()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0), // 0
            new Exact3(1.0, 0.0, 0.0), // 1
            new Exact3(0.0, 1.0, 0.0), // 2
            new Exact3(0.0, 0.0, 1.0), // 3
        };

        Span<Face> faces = stackalloc Face[4];

        ExactHullTopology3D.CreateInitialTetrahedronFaces(points, 0, 1, 2, 3, faces);

        Assert.Equal(4, faces.Length);

        for (int i = 0; i < 4; i++)
        {
            AssertDistinct(faces[i].A, faces[i].B, faces[i].C);
        }
    }

    [Fact]
    public void CreateInitialTetrahedronFaces_OrientsAllFacesOutward()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0), // 0
            new Exact3(1.0, 0.0, 0.0), // 1
            new Exact3(0.0, 1.0, 0.0), // 2
            new Exact3(0.0, 0.0, 1.0), // 3
        };

        Span<Face> faces = stackalloc Face[4];

        ExactHullTopology3D.CreateInitialTetrahedronFaces(points, 0, 1, 2, 3, faces);

        AssertFacePointsOutward(points, faces[0], opposite: 3);
        AssertFacePointsOutward(points, faces[1], opposite: 2);
        AssertFacePointsOutward(points, faces[2], opposite: 1);
        AssertFacePointsOutward(points, faces[3], opposite: 0);
    }

    [Fact]
    public void CreateOrientedFace_ThrowsForCoplanarOppositePoint()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.25, 0.25, 0.0),
        };

        Assert.Throws<ArgumentException>(() =>
            ExactHullTopology3D.CreateOrientedFace(points, 0, 1, 2, 3));
    }

    [Fact]
    public void CreateInitialTetrahedronFaces_WorksWithIndicesReturnedByFinder()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0),
            new Exact3(0.25, 0.25, 0.25),
        };

        bool success = ExactHullBuilder3D.TryFindInitialTetrahedron(
            points, out int i0, out int i1, out int i2, out int i3);

        Assert.True(success);

        Span<Face> faces = stackalloc Face[4];
        ExactHullTopology3D.CreateInitialTetrahedronFaces(points, i0, i1, i2, i3, faces);

        AssertFacePointsOutward(points, faces[0], i3);
        AssertFacePointsOutward(points, faces[1], i2);
        AssertFacePointsOutward(points, faces[2], i1);
        AssertFacePointsOutward(points, faces[3], i0);
    }

    private static void AssertFacePointsOutward(
        ReadOnlySpan<Exact3> points,
        Face face,
        int opposite)
    {
        Exact orient = ExactGeometry3D.Orient3D(
            points[face.A],
            points[face.B],
            points[face.C],
            points[opposite]);

        Assert.True(orient.Sign() < 0);
    }

    private static void AssertDistinct(int a, int b, int c)
    {
        Assert.NotEqual(a, b);
        Assert.NotEqual(a, c);
        Assert.NotEqual(b, c);
    }
}
