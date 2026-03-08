using ExactHull.ExactGeometry;
using Xunit;

namespace ExactHull.Tests;

public sealed class ExpandHullByPointTests
{
    [Fact]
    public void InsidePoint_DoesNotChangeHull()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0), // 0
            new Exact3(1.0, 0.0, 0.0), // 1
            new Exact3(0.0, 1.0, 0.0), // 2
            new Exact3(0.0, 0.0, 1.0), // 3
            new Exact3(0.1, 0.1, 0.1), // 4 inside
        };

        Span<Face> faces = stackalloc Face[16];
        ExactHullTopology3D.CreateInitialTetrahedronFaces(points, 0, 1, 2, 3, faces);

        int faceCount = 4;

        Span<int> visible = stackalloc int[16];
        Span<Edge> horizon = stackalloc Edge[32];
        Span<Face> newFaces = stackalloc Face[32];

        int newCount = ExactHullTopology3D.ExpandHullByPoint(
            points, faces, faceCount, 4, points[0] + new Exact3(0.1, 0.1, 0.1), visible, horizon, newFaces);

        Assert.Equal(4, newCount);
    }

    [Fact]
    public void PointOutsideOneFace_ReplacesOneFaceWithThree()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0), // 0
            new Exact3(1.0, 0.0, 0.0), // 1
            new Exact3(0.0, 1.0, 0.0), // 2
            new Exact3(0.0, 0.0, 1.0), // 3
            new Exact3(0.25, 0.25, -1.0), // 4 outside base face
            new Exact3(0.1, 0.1, 0.1), // 5 inside point
        };

        Span<Face> faces = stackalloc Face[16];
        ExactHullTopology3D.CreateInitialTetrahedronFaces(points, 0, 1, 2, 3, faces);

        int faceCount = 4;

        Span<int> visible = stackalloc int[16];
        Span<Edge> horizon = stackalloc Edge[32];
        Span<Face> newFaces = stackalloc Face[32];

        int newCount = ExactHullTopology3D.ExpandHullByPoint(
            points, faces, faceCount, 4, points[5], visible, horizon, newFaces);

        Assert.Equal(6, newCount);

        for (int i = 0; i < newCount; i++)
        {
            AssertFacePointsOutward(points, faces[i], points[5]);
        }

        Assert.Equal(3, CountFacesUsingVertex(faces[..newCount], 4));
    }

    [Fact]
    public void PointOutsideTwoFaces_ReplacesTwoFacesWithFour()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0), // 0
            new Exact3(1.0, 0.0, 0.0), // 1
            new Exact3(0.0, 1.0, 0.0), // 2
            new Exact3(0.0, 0.0, 1.0), // 3
            new Exact3(2.0, 2.0, -1.0), // 4 outside two faces
            new Exact3(0.1, 0.1, 0.1), // 5 inside point
        };

        Span<Face> faces = stackalloc Face[16];
        ExactHullTopology3D.CreateInitialTetrahedronFaces(points, 0, 1, 2, 3, faces);

        int faceCount = 4;

        Span<int> visible = stackalloc int[16];
        Span<Edge> horizon = stackalloc Edge[32];
        Span<Face> newFaces = stackalloc Face[32];

        int newCount = ExactHullTopology3D.ExpandHullByPoint(
            points, faces, faceCount, 4, points[5], visible, horizon, newFaces);

        Assert.Equal(6, newCount);

        for (int i = 0; i < newCount; i++)
        {
            AssertFacePointsOutward(points, faces[i], points[5]);
        }

        Assert.Equal(4, CountFacesUsingVertex(faces[..newCount], 4));
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
