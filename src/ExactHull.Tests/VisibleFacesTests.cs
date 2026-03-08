using ExactHull.ExactGeometry;
using Xunit;

namespace ExactHull.Tests;

public sealed class VisibleFacesTests
{
    [Fact]
    public void InsidePoint_SeesNoFaces()
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

        var p = new Exact3(0.1, 0.1, 0.1);

        Span<int> visible = stackalloc int[4];
        int count = ExactHullTopology3D.CollectVisibleFaces(points, faces, p, visible);

        Assert.Equal(0, count);
    }

    [Fact]
    public void OutsidePoint_SeesAtLeastOneFace()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0),
        };

        Span<Face> faces = stackalloc Face[4];
        ExactHullTopology3D.CreateInitialTetrahedronFaces(points, 0, 1, 2, 3, faces);

        var p = new Exact3(2.0, 2.0, 2.0);

        Span<int> visible = stackalloc int[4];
        int count = ExactHullTopology3D.CollectVisibleFaces(points, faces, p, visible);

        Assert.True(count > 0);
    }

    [Fact]
    public void PointOnFacePlane_DoesNotSeeThatFace()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0),
        };

        Span<Face> faces = stackalloc Face[4];
        ExactHullTopology3D.CreateInitialTetrahedronFaces(points, 0, 1, 2, 3, faces);

        Face baseFace = FindFaceUsingVertices(faces, 0, 1, 2);

        var p = new Exact3(0.25, 0.25, 0.0);

        bool visible = ExactHullTopology3D.IsFaceVisible(points, baseFace, p);

        Assert.False(visible);
    }

    [Fact]
    public void PointAboveBaseFace_SeesBaseFace()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0),
        };

        Span<Face> faces = stackalloc Face[4];
        ExactHullTopology3D.CreateInitialTetrahedronFaces(points, 0, 1, 2, 3, faces);

        Face baseFace = FindFaceUsingVertices(faces, 0, 1, 2);

        var p = new Exact3(0.25, 0.25, -1.0);

        bool visible = ExactHullTopology3D.IsFaceVisible(points, baseFace, p);

        Assert.True(visible);
    }

    [Fact]
    public void PointAboveTopFace_SeesTopFace()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0),
        };

        Span<Face> faces = stackalloc Face[4];
        ExactHullTopology3D.CreateInitialTetrahedronFaces(points, 0, 1, 2, 3, faces);

        Face topFace = FindFaceUsingVertices(faces, 1, 2, 3);

        var p = new Exact3(2.0, 2.0, 2.0);

        bool visible = ExactHullTopology3D.IsFaceVisible(points, topFace, p);

        Assert.True(visible);
    }

    private static Face FindFaceUsingVertices(ReadOnlySpan<Face> faces, int a, int b, int c)
    {
        for (int i = 0; i < faces.Length; i++)
        {
            if (UsesSameVertexSet(faces[i], a, b, c))
                return faces[i];
        }

        throw new InvalidOperationException("Face not found.");
    }

    private static bool UsesSameVertexSet(Face face, int a, int b, int c)
    {
        int matchCount = 0;

        if (face.A == a || face.A == b || face.A == c) matchCount++;
        if (face.B == a || face.B == b || face.B == c) matchCount++;
        if (face.C == a || face.C == b || face.C == c) matchCount++;

        return matchCount == 3;
    }
}
