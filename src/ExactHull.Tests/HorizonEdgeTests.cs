using ExactHull.ExactGeometry;
using Xunit;

namespace ExactHull.Tests;

public sealed class HorizonEdgesTests
{
    [Fact]
    public void InsidePoint_ProducesNoHorizon()
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
        int visibleCount = ExactHullTopology3D.CollectVisibleFaces(points, faces, p, visible);

        Assert.Equal(0, visibleCount);

        Span<Edge> horizon = stackalloc Edge[12];
        int horizonCount = ExactHullTopology3D.CollectHorizonEdges(faces, visible[..visibleCount], horizon);

        Assert.Equal(0, horizonCount);
    }

    [Fact]
    public void PointSeeingOneFace_ProducesThreeHorizonEdges()
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

        var p = new Exact3(0.25, 0.25, -1.0);

        Span<int> visible = stackalloc int[4];
        int visibleCount = ExactHullTopology3D.CollectVisibleFaces(points, faces, p, visible);

        Assert.Equal(1, visibleCount);

        Span<Edge> horizon = stackalloc Edge[12];
        int horizonCount = ExactHullTopology3D.CollectHorizonEdges(faces, visible[..visibleCount], horizon);

        Assert.Equal(3, horizonCount);

        Face visibleFace = faces[visible[0]];
        AssertContainsUndirectedEdgeSet(horizon[..horizonCount], visibleFace);
    }

    [Fact]
    public void PointSeeingMultipleFaces_CancelsSharedEdges()
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

        var p = new Exact3(2.0, 2.0, -1.0);

        Span<int> visible = stackalloc int[4];
        int visibleCount = ExactHullTopology3D.CollectVisibleFaces(points, faces, p, visible);

        Assert.Equal(2, visibleCount);

        Span<Edge> horizon = stackalloc Edge[12];
        int horizonCount = ExactHullTopology3D.CollectHorizonEdges(faces, visible[..visibleCount], horizon);

        Assert.Equal(4, horizonCount);
        AssertNoReversePairs(horizon[..horizonCount]);
    }

    [Fact]
    public void TwoVisibleFacesSharingOneEdge_ProduceFourBoundaryEdges()
    {
        Span<Face> faces = stackalloc Face[2];
        faces[0] = new Face(0, 1, 2);
        faces[1] = new Face(1, 0, 3);

        Span<int> visible = stackalloc int[2];
        visible[0] = 0;
        visible[1] = 1;

        Span<Edge> horizon = stackalloc Edge[6];
        int horizonCount = ExactHullTopology3D.CollectHorizonEdges(faces, visible, horizon);

        Assert.Equal(4, horizonCount);

        AssertContainsUndirectedEdge(horizon[..horizonCount], 1, 2);
        AssertContainsUndirectedEdge(horizon[..horizonCount], 2, 0);
        AssertContainsUndirectedEdge(horizon[..horizonCount], 0, 3);
        AssertContainsUndirectedEdge(horizon[..horizonCount], 3, 1);

        Assert.DoesNotContain(horizon[..horizonCount].ToArray(), e =>
            (e.A == 0 && e.B == 1) || (e.A == 1 && e.B == 0));
    }

    private static void AssertContainsUndirectedEdgeSet(ReadOnlySpan<Edge> edges, Face face)
    {
        AssertContainsUndirectedEdge(edges, face.A, face.B);
        AssertContainsUndirectedEdge(edges, face.B, face.C);
        AssertContainsUndirectedEdge(edges, face.C, face.A);
    }

    private static void AssertContainsUndirectedEdge(ReadOnlySpan<Edge> edges, int a, int b)
    {
        for (int i = 0; i < edges.Length; i++)
        {
            if ((edges[i].A == a && edges[i].B == b) ||
                (edges[i].A == b && edges[i].B == a))
            {
                return;
            }
        }

        Assert.Fail($"Expected edge {{{a}, {b}}} was not found.");
    }

    private static void AssertNoReversePairs(ReadOnlySpan<Edge> edges)
    {
        for (int i = 0; i < edges.Length; i++)
        {
            for (int j = i + 1; j < edges.Length; j++)
            {
                Assert.False(edges[i].A == edges[j].B && edges[i].B == edges[j].A,
                    $"Found reverse pair ({edges[i].A}, {edges[i].B}) and ({edges[j].A}, {edges[j].B}).");
            }
        }
    }
}
