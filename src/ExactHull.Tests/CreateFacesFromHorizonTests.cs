using ExactHull.ExactGeometry;
using Xunit;

namespace ExactHull.Tests;

public sealed class CreateFacesFromHorizonTests
{
    [Fact]
    public void OneTriangleHorizon_CreatesThreeFaces()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0), // 0
            new Exact3(1.0, 0.0, 0.0), // 1
            new Exact3(0.0, 1.0, 0.0), // 2
            new Exact3(0.0, 0.0, 1.0), // 3 inside tetra vertex
            new Exact3(0.25, 0.25, -1.0), // 4 new outside point
        };

        Span<Edge> horizon = stackalloc Edge[3];
        horizon[0] = new Edge(0, 1);
        horizon[1] = new Edge(1, 2);
        horizon[2] = new Edge(2, 0);

        Span<Face> faces = stackalloc Face[3];
        int count = ExactHullTopology3D.CreateFacesFromHorizon(
            points, horizon, 4, points[3], faces);

        Assert.Equal(3, count);

        for (int i = 0; i < count; i++)
        {
            Assert.Equal(4, OneOf(faces[i].A, faces[i].B, faces[i].C, 4));
            AssertFacePointsOutward(points, faces[i], points[3]);
        }
    }

    [Fact]
    public void FourEdgeHorizon_CreatesFourFaces()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0), // 0
            new Exact3(1.0, 0.0, 0.0), // 1
            new Exact3(0.0, 1.0, 0.0), // 2
            new Exact3(0.0, 0.0, 1.0), // 3
            new Exact3(2.0, 2.0, -1.0), // 4
            new Exact3(0.1, 0.1, 0.1), // 5 inside point
        };

        Span<Edge> horizon = stackalloc Edge[4];
        horizon[0] = new Edge(0, 1);
        horizon[1] = new Edge(1, 3);
        horizon[2] = new Edge(3, 2);
        horizon[3] = new Edge(2, 0);

        Span<Face> faces = stackalloc Face[4];
        int count = ExactHullTopology3D.CreateFacesFromHorizon(
            points, horizon, 4, points[5], faces);

        Assert.Equal(4, count);

        for (int i = 0; i < count; i++)
        {
            AssertFacePointsOutward(points, faces[i], points[5]);
        }
    }

    [Fact]
    public void CreateOrientedFace_Throws_WhenInsidePointIsCoplanar()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.25, 0.25, 0.0),
        };

        Assert.Throws<ArgumentException>(() =>
            ExactHullTopology3D.CreateOrientedFace(points, 0, 1, 2, points[3]));
    }

    [Fact]
    public void RealTetraExpansionPath_Works()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0), // 0
            new Exact3(1.0, 0.0, 0.0), // 1
            new Exact3(0.0, 1.0, 0.0), // 2
            new Exact3(0.0, 0.0, 1.0), // 3
            new Exact3(0.25, 0.25, -1.0), // 4 outside base face
            new Exact3(0.1, 0.1, 0.1), // 5 inside tetrahedron
        };

        Span<Face> hullFaces = stackalloc Face[4];
        ExactHullTopology3D.CreateInitialTetrahedronFaces(points, 0, 1, 2, 3, hullFaces);

        Span<int> visible = stackalloc int[4];
        int visibleCount = ExactHullTopology3D.CollectVisibleFaces(points, hullFaces, points[4], visible);

        Assert.Equal(1, visibleCount);

        Span<Edge> horizon = stackalloc Edge[12];
        int horizonCount = ExactHullTopology3D.CollectHorizonEdges(
            hullFaces, visible[..visibleCount], horizon);

        Assert.Equal(3, horizonCount);

        Span<Face> newFaces = stackalloc Face[3];
        int newFaceCount = ExactHullTopology3D.CreateFacesFromHorizon(
            points, horizon[..horizonCount], 4, points[5], newFaces);

        Assert.Equal(3, newFaceCount);

        for (int i = 0; i < newFaceCount; i++)
        {
            AssertFacePointsOutward(points, newFaces[i], points[5]);
        }
    }

    private static int OneOf(int a, int b, int c, int expected)
    {
        if (a == expected) return a;
        if (b == expected) return b;
        if (c == expected) return c;
        return -1;
    }

    private static void AssertFacePointsOutward(
        ReadOnlySpan<Exact3> points,
        Face face,
        Exact3 insidePoint)
    {
        Exact orient = ExactGeometry3D.Orient3D(
            points[face.A],
            points[face.B],
            points[face.C],
            insidePoint);

        Assert.True(orient.Sign() < 0);
    }
}
