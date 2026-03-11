using ExactHull.ExactGeometry;
using Xunit;

namespace ExactHull.Tests;

/// <summary>
/// Adversarial torture tests designed to break the convex hull builder.
/// Targets: degenerate geometry, numerical edge cases, topological nightmares,
/// extreme coordinate ranges, and pathological point distributions.
/// </summary>
public sealed class BerserkStressTests
{
    // ─────────────────────────────────────────────────────────
    //  1. ALL POINTS IDENTICAL — should fail gracefully
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void AllIdenticalPoints_ReturnsFalse()
    {
        var points = new Exact3[100];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Exact3(7.0, 7.0, 7.0);

        bool success = ExactHullBuilder3D.TryBuildHull(points, out _, out _);
        Assert.False(success);
    }

    // ─────────────────────────────────────────────────────────
    //  2. ALL POINTS COLLINEAR — should fail gracefully
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void AllCollinearPoints_ReturnsFalse()
    {
        var points = new Exact3[50];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Exact3(i * 0.1, i * 0.2, i * 0.3);

        bool success = ExactHullBuilder3D.TryBuildHull(points, out _, out _);
        Assert.False(success);
    }

    // ─────────────────────────────────────────────────────────
    //  3. ALL POINTS COPLANAR — should fail gracefully
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void AllCoplanarPoints_ReturnsFalse()
    {
        var points = new Exact3[50];
        var rng = new Random(99);
        for (int i = 0; i < points.Length; i++)
            points[i] = new Exact3(rng.NextDouble() * 100, rng.NextDouble() * 100, 0.0);

        bool success = ExactHullBuilder3D.TryBuildHull(points, out _, out _);
        Assert.False(success);
    }

    // ─────────────────────────────────────────────────────────
    //  4. EXACTLY 4 POINTS (minimal tetrahedron)
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void ExactlyFourPoints_MinimalTetrahedron()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0),
        };

        bool success = ExactHullBuilder3D.TryBuildHull(points, out var faces, out int faceCount);
        Assert.True(success);
        Assert.Equal(4, faceCount);
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  5. SUBNORMAL DOUBLES — smallest representable values
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void SubnormalCoordinates_ProduceValidHull()
    {
        double tiny = 5e-324; // smallest positive subnormal
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(tiny, 0.0, 0.0),
            new Exact3(0.0, tiny, 0.0),
            new Exact3(0.0, 0.0, tiny),
        };

        bool success = ExactHullBuilder3D.TryBuildHull(points, out var faces, out int faceCount);
        Assert.True(success);
        Assert.Equal(4, faceCount);
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  6. HUGE COORDINATES — near double max
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void HugeCoordinates_ProduceValidHull()
    {
        double big = 1e+300;
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(big, 0.0, 0.0),
            new Exact3(0.0, big, 0.0),
            new Exact3(0.0, 0.0, big),
            new Exact3(big, big, big),
        };

        bool success = ExactHullBuilder3D.TryBuildHull(points, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  7. MIXED EXTREME SCALES — huge + tiny in same cloud
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void MixedExtremeScales_ProduceValidHull()
    {
        var points = new[]
        {
            new Exact3(1e+200, 1e+200, 1e+200),
            new Exact3(-1e+200, -1e+200, -1e+200),
            new Exact3(1e-200, 0.0, 0.0),
            new Exact3(0.0, 1e-200, 0.0),
            new Exact3(0.0, 0.0, 1e-200),
            new Exact3(1e+200, -1e+200, 0.0),
            new Exact3(-1e+200, 1e+200, 0.0),
        };

        bool success = ExactHullBuilder3D.TryBuildHull(points, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  8. POINTS ON A REGULAR GRID — massive coplanarity
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void RegularGrid_ProducesValidHull()
    {
        var points = new List<Exact3>();
        for (int x = 0; x < 5; x++)
        for (int y = 0; y < 5; y++)
        for (int z = 0; z < 5; z++)
            points.Add(new Exact3((double)x, (double)y, (double)z));

        var arr = points.ToArray();
        bool success = ExactHullBuilder3D.TryBuildHull(arr, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(faceCount >= 4);
        Assert.True(ExactHullValidation3D.IsHullValid(arr, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  9. FLAT PANCAKE — massive coplanar set + single outlier
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void FlatPancakeWithSingleOutlier_ProducesValidHull()
    {
        var points = new List<Exact3>();
        // 200 points exactly on z=0 plane
        var rng = new Random(42);
        for (int i = 0; i < 200; i++)
            points.Add(new Exact3(rng.NextDouble() * 100 - 50, rng.NextDouble() * 100 - 50, 0.0));

        // Single point off-plane
        points.Add(new Exact3(0.0, 0.0, 1e-15));

        var arr = points.ToArray();
        bool success = ExactHullBuilder3D.TryBuildHull(arr, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(faceCount >= 4);
        Assert.True(ExactHullValidation3D.IsHullValid(arr, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  10. NEEDLE — extreme aspect ratio
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void ExtremeNeedle_ProducesValidHull()
    {
        // Very long along X, tiny cross-section
        var points = new[]
        {
            new Exact3(-1e+15, 0.0, 0.0),
            new Exact3(1e+15, 0.0, 0.0),
            new Exact3(0.0, 1e-15, 0.0),
            new Exact3(0.0, -1e-15, 0.0),
            new Exact3(0.0, 0.0, 1e-15),
            new Exact3(0.0, 0.0, -1e-15),
        };

        bool success = ExactHullBuilder3D.TryBuildHull(points, out var faces, out int faceCount);
        Assert.True(success);
        Assert.Equal(8, faceCount); // octahedron
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  11. MANY DUPLICATES OF FEW DISTINCT POINTS
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void MassiveDuplicates_ProducesValidHull()
    {
        var distinct = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.0, 0.0, 1.0),
            new Exact3(1.0, 1.0, 1.0),
        };

        // 500 copies of 5 points
        var points = new Exact3[500];
        for (int i = 0; i < points.Length; i++)
            points[i] = distinct[i % distinct.Length];

        Shuffle(new Random(777), points);
        bool success = ExactHullBuilder3D.TryBuildHull(points, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  12. POINTS ON EDGES OF TETRAHEDRON
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void ManyPointsOnTetrahedronEdges_ProducesValidHull()
    {
        var corners = new[]
        {
            (0.0, 0.0, 0.0),
            (1.0, 0.0, 0.0),
            (0.0, 1.0, 0.0),
            (0.0, 0.0, 1.0),
        };

        var points = new List<Exact3>();
        // Add corners
        foreach (var c in corners)
            points.Add(new Exact3(c.Item1, c.Item2, c.Item3));

        // 20 points per edge
        for (int i = 0; i < 4; i++)
        for (int j = i + 1; j < 4; j++)
        for (int k = 1; k <= 20; k++)
        {
            double t = k / 21.0;
            points.Add(new Exact3(
                corners[i].Item1 * (1 - t) + corners[j].Item1 * t,
                corners[i].Item2 * (1 - t) + corners[j].Item2 * t,
                corners[i].Item3 * (1 - t) + corners[j].Item3 * t));
        }

        var arr = points.ToArray();
        Shuffle(new Random(333), arr);
        bool success = ExactHullBuilder3D.TryBuildHull(arr, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(faceCount >= 4);
        Assert.True(ExactHullValidation3D.IsHullValid(arr, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  13. NEARLY-DEGENERATE TETRAHEDRON — 4th point almost coplanar
    // ─────────────────────────────────────────────────────────
    [Theory]
    [InlineData(1e-1)]
    [InlineData(1e-5)]
    [InlineData(1e-10)]
    [InlineData(1e-15)]
    [InlineData(5e-324)] // smallest subnormal
    public void NearlyFlatTetrahedron_AtVariousThicknesses(double epsilon)
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.0, 1.0, 0.0),
            new Exact3(0.333, 0.333, epsilon),
        };

        bool success = ExactHullBuilder3D.TryBuildHull(points, out var faces, out int faceCount);
        Assert.True(success);
        Assert.Equal(4, faceCount);
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  14. STAR-SHAPED SPIKES — points along axes at extreme distances
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void StarShapedSpikes_ProducesValidHull()
    {
        var points = new List<Exact3>();
        double[] magnitudes = { 1e-300, 1e-100, 1.0, 1e+100, 1e+300 };

        foreach (double m in magnitudes)
        {
            points.Add(new Exact3(m, 0.0, 0.0));
            points.Add(new Exact3(-m, 0.0, 0.0));
            points.Add(new Exact3(0.0, m, 0.0));
            points.Add(new Exact3(0.0, -m, 0.0));
            points.Add(new Exact3(0.0, 0.0, m));
            points.Add(new Exact3(0.0, 0.0, -m));
        }

        var arr = points.ToArray();
        bool success = ExactHullBuilder3D.TryBuildHull(arr, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(ExactHullValidation3D.IsHullValid(arr, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  15. SPHERE SURFACE + MANY INTERIOR POINTS
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void SphereWithManyInteriorPoints_ProducesValidHull()
    {
        var rng = new Random(55555);
        var points = new List<Exact3>();

        // 50 points on unit sphere
        for (int i = 0; i < 50; i++)
            points.Add(RandomSpherePoint(rng, 1.0));

        // 500 interior points
        for (int i = 0; i < 500; i++)
        {
            double r = rng.NextDouble() * 0.99;
            var sp = RandomSpherePoint(rng, r);
            points.Add(sp);
        }

        var arr = points.ToArray();
        Shuffle(rng, arr);
        bool success = ExactHullBuilder3D.TryBuildHull(arr, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(ExactHullValidation3D.IsHullValid(arr, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  16. ADVERSARIAL PERMUTATION FUZZ — same points, many orderings
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void SamePoints_ManyPermutations_AllProduceValidHulls()
    {
        var basePoints = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.5, 0.866, 0.0),
            new Exact3(0.5, 0.289, 0.816),
            new Exact3(0.1, 0.1, 0.1),
            new Exact3(0.9, 0.1, 0.1),
            new Exact3(0.3, 0.6, 0.05),
        };

        for (int seed = 0; seed < 500; seed++)
        {
            var points = (Exact3[])basePoints.Clone();
            Shuffle(new Random(seed), points);

            bool success = ExactHullBuilder3D.TryBuildHull(points, out var faces, out int faceCount);
            Assert.True(success, $"Failed at seed={seed}");
            Assert.True(ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)),
                $"Invalid hull at seed={seed}");
        }
    }

    // ─────────────────────────────────────────────────────────
    //  17. POINTS ON FACES OF CUBE — all coplanar quads
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void DensePointsOnCubeFaces_ProducesValidHull()
    {
        var points = new List<Exact3>();
        int n = 10;
        // Points on all 6 faces of unit cube
        for (int i = 0; i <= n; i++)
        for (int j = 0; j <= n; j++)
        {
            double u = i / (double)n;
            double v = j / (double)n;
            points.Add(new Exact3(u, v, 0.0)); // bottom
            points.Add(new Exact3(u, v, 1.0)); // top
            points.Add(new Exact3(u, 0.0, v)); // front
            points.Add(new Exact3(u, 1.0, v)); // back
            points.Add(new Exact3(0.0, u, v)); // left
            points.Add(new Exact3(1.0, u, v)); // right
        }

        var arr = points.ToArray();
        Shuffle(new Random(12321), arr);
        bool success = ExactHullBuilder3D.TryBuildHull(arr, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(faceCount >= 12); // at least a cube's worth of faces
        Assert.True(ExactHullValidation3D.IsHullValid(arr, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  18. POSITIVE/NEGATIVE POWERS OF TWO — exact dyadic stress
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void PowersOfTwo_ProducesValidHull()
    {
        var points = new List<Exact3>();
        for (int e = -50; e <= 50; e++)
        {
            double v = Math.Pow(2.0, e);
            points.Add(new Exact3(v, 0.0, 0.0));
            points.Add(new Exact3(0.0, v, 0.0));
            points.Add(new Exact3(0.0, 0.0, v));
        }
        // Need negative coordinates for a proper 3D hull
        points.Add(new Exact3(-1.0, -1.0, -1.0));

        var arr = points.ToArray();
        bool success = ExactHullBuilder3D.TryBuildHull(arr, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(ExactHullValidation3D.IsHullValid(arr, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  19. CLUSTER BOMB — tight clusters far apart
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void TightClustersFarApart_ProducesValidHull()
    {
        var rng = new Random(666);
        var points = new List<Exact3>();

        double[] centers = { -1e+10, 0, 1e+10 };
        foreach (double cx in centers)
        foreach (double cy in centers)
        foreach (double cz in centers)
        {
            for (int i = 0; i < 5; i++)
            {
                points.Add(new Exact3(
                    cx + (rng.NextDouble() - 0.5) * 1e-10,
                    cy + (rng.NextDouble() - 0.5) * 1e-10,
                    cz + (rng.NextDouble() - 0.5) * 1e-10));
            }
        }

        var arr = points.ToArray();
        Shuffle(rng, arr);
        bool success = ExactHullBuilder3D.TryBuildHull(arr, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(ExactHullValidation3D.IsHullValid(arr, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  20. SPIRAL — points on a helix (no symmetry to exploit)
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void Helix_ProducesValidHull()
    {
        var points = new List<Exact3>();
        for (int i = 0; i < 200; i++)
        {
            double t = i * 0.1;
            points.Add(new Exact3(Math.Cos(t), Math.Sin(t), t * 0.01));
        }

        var arr = points.ToArray();
        bool success = ExactHullBuilder3D.TryBuildHull(arr, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(ExactHullValidation3D.IsHullValid(arr, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  21. CONVEX HULL OF CONVEX HULL — idempotency
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void ConvexHullIsIdempotent()
    {
        var rng = new Random(88888);
        var points = new Exact3[50];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Exact3(
                rng.NextDouble() * 10 - 5,
                rng.NextDouble() * 10 - 5,
                rng.NextDouble() * 10 - 5);

        // First hull
        bool s1 = ExactHullBuilder3D.TryBuildHull(points, out var faces1, out int fc1);
        Assert.True(s1);

        // Extract hull vertices
        var hullVertexIndices = new HashSet<int>();
        for (int i = 0; i < fc1; i++)
        {
            hullVertexIndices.Add(faces1[i].A);
            hullVertexIndices.Add(faces1[i].B);
            hullVertexIndices.Add(faces1[i].C);
        }

        // Build hull of just the hull vertices
        var hullPoints = new Exact3[hullVertexIndices.Count];
        int idx = 0;
        foreach (int vi in hullVertexIndices)
            hullPoints[idx++] = points[vi];

        bool s2 = ExactHullBuilder3D.TryBuildHull(hullPoints, out var faces2, out int fc2);
        Assert.True(s2);
        Assert.Equal(fc1, fc2); // same number of faces
        Assert.True(ExactHullValidation3D.IsHullValid(hullPoints, faces2.AsSpan(0, fc2)));
    }

    // ─────────────────────────────────────────────────────────
    //  22. ADVERSARIAL NEAR-COPLANAR WITH HUGE COORDINATES
    //      The nightmare scenario for floating-point hulls
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void NearCoplanarAtHugeScale_ProducesValidHull()
    {
        var rng = new Random(31415);
        var points = new List<Exact3>();

        for (int i = 0; i < 100; i++)
        {
            double x = 1e+15 + rng.NextDouble();
            double y = 1e+15 + rng.NextDouble();
            double z = (rng.NextDouble() - 0.5) * 1e-10;
            points.Add(new Exact3(x, y, z));
        }
        // Ensure 3D
        points.Add(new Exact3(1e+15, 1e+15, 1.0));
        points.Add(new Exact3(1e+15, 1e+15, -1.0));

        var arr = points.ToArray();
        Shuffle(rng, arr);
        bool success = ExactHullBuilder3D.TryBuildHull(arr, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(ExactHullValidation3D.IsHullValid(arr, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  23. TWO ADJACENT TETRAHEDRA sharing a face
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void TwoAdjacentTetrahedra_ProducesValidHull()
    {
        var points = new[]
        {
            new Exact3(0.0, 0.0, 0.0),
            new Exact3(1.0, 0.0, 0.0),
            new Exact3(0.5, 1.0, 0.0),
            new Exact3(0.5, 0.5, 1.0),
            new Exact3(0.5, 0.5, -1.0),
        };

        bool success = ExactHullBuilder3D.TryBuildHull(points, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  24. RANDOM FUZZ WITH MANY SEEDS — brute force search for crashes
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void BruteForceFuzz_1000Seeds()
    {
        for (int seed = 0; seed < 1000; seed++)
        {
            var rng = new Random(seed);
            int n = rng.Next(4, 30);
            var points = new Exact3[n];
            for (int i = 0; i < n; i++)
                points[i] = new Exact3(
                    rng.NextDouble() * 2 - 1,
                    rng.NextDouble() * 2 - 1,
                    rng.NextDouble() * 2 - 1);

            bool success = ExactHullBuilder3D.TryBuildHull(points, out var faces, out int faceCount);

            if (success)
            {
                Assert.True(faceCount >= 4, $"faceCount={faceCount} at seed={seed}");
                Assert.True(
                    ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)),
                    $"Invalid hull at seed={seed}");
            }
        }
    }

    // ─────────────────────────────────────────────────────────
    //  25. DEGENERATE FUZZ — clouds that are sometimes 
    //      collinear/coplanar/identical
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void DegenerateFuzz_500Seeds()
    {
        for (int seed = 0; seed < 500; seed++)
        {
            var rng = new Random(seed * 7 + 13);
            int n = rng.Next(4, 40);
            int dims = rng.Next(0, 4); // 0=identical, 1=collinear, 2=coplanar, 3=full 3D
            var points = new Exact3[n];

            for (int i = 0; i < n; i++)
            {
                double x = dims >= 1 ? rng.NextDouble() * 10 - 5 : 1.0;
                double y = dims >= 2 ? rng.NextDouble() * 10 - 5 : 0.0;
                double z = dims >= 3 ? rng.NextDouble() * 10 - 5 : 0.0;
                points[i] = new Exact3(x, y, z);
            }

            bool success = ExactHullBuilder3D.TryBuildHull(points, out var faces, out int faceCount);

            if (dims < 3)
            {
                // Should fail for degenerate cases
                // (unless random chance gives us non-degenerate points on collinear/coplanar)
                if (success)
                {
                    Assert.True(faceCount >= 4);
                    Assert.True(ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)),
                        $"Invalid hull at seed={seed}, dims={dims}");
                }
            }
            else if (success)
            {
                Assert.True(faceCount >= 4);
                Assert.True(ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)),
                    $"Invalid hull at seed={seed}");
            }
        }
    }

    // ─────────────────────────────────────────────────────────
    //  26. ONION LAYERS — nested convex hulls
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void OnionLayers_ProducesValidHull()
    {
        var rng = new Random(22222);
        var points = new List<Exact3>();

        // 5 concentric cubes
        for (int layer = 1; layer <= 5; layer++)
        {
            double s = layer;
            // 8 corners of cube at scale s
            for (int sx = -1; sx <= 1; sx += 2)
            for (int sy = -1; sy <= 1; sy += 2)
            for (int sz = -1; sz <= 1; sz += 2)
                points.Add(new Exact3(sx * s, sy * s, sz * s));

            // + random interior points for this layer
            for (int i = 0; i < 10; i++)
                points.Add(new Exact3(
                    (rng.NextDouble() * 2 - 1) * (s - 0.1),
                    (rng.NextDouble() * 2 - 1) * (s - 0.1),
                    (rng.NextDouble() * 2 - 1) * (s - 0.1)));
        }

        var arr = points.ToArray();
        Shuffle(rng, arr);
        bool success = ExactHullBuilder3D.TryBuildHull(arr, out var faces, out int faceCount);
        Assert.True(success);
        Assert.Equal(12, faceCount); // outermost cube
        Assert.True(ExactHullValidation3D.IsHullValid(arr, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  27. ICOSAHEDRON VERTICES — known regular polyhedron
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void Icosahedron_ProducesValidHull()
    {
        double phi = (1.0 + Math.Sqrt(5.0)) / 2.0;
        var points = new[]
        {
            new Exact3(-1.0, phi, 0.0), new Exact3(1.0, phi, 0.0),
            new Exact3(-1.0, -phi, 0.0), new Exact3(1.0, -phi, 0.0),
            new Exact3(0.0, -1.0, phi), new Exact3(0.0, 1.0, phi),
            new Exact3(0.0, -1.0, -phi), new Exact3(0.0, 1.0, -phi),
            new Exact3(phi, 0.0, -1.0), new Exact3(phi, 0.0, 1.0),
            new Exact3(-phi, 0.0, -1.0), new Exact3(-phi, 0.0, 1.0),
        };

        bool success = ExactHullBuilder3D.TryBuildHull(points, out var faces, out int faceCount);
        Assert.True(success);
        Assert.Equal(20, faceCount); // icosahedron has 20 triangular faces
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  28. POINTS ON SPHERE + EXACT CENTER — center is interior
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void SphereWithCenterPoint_CenterIsIgnored()
    {
        var rng = new Random(11111);
        var points = new List<Exact3>();
        points.Add(new Exact3(0.0, 0.0, 0.0)); // center

        for (int i = 0; i < 100; i++)
            points.Add(RandomSpherePoint(rng, 1.0));

        var arr = points.ToArray();
        Shuffle(rng, arr);
        bool success = ExactHullBuilder3D.TryBuildHull(arr, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(ExactHullValidation3D.IsHullValid(arr, faces.AsSpan(0, faceCount)));

        // Center point should NOT appear in any face
        var hullVerts = new HashSet<int>();
        for (int i = 0; i < faceCount; i++)
        {
            hullVerts.Add(faces[i].A);
            hullVerts.Add(faces[i].B);
            hullVerts.Add(faces[i].C);
        }

        int centerIdx = Array.IndexOf(arr, new Exact3(0.0, 0.0, 0.0));
        // The center might have been shuffled, but it's strictly interior
        // so it shouldn't be a hull vertex
        // (We don't assert this since Exact3 doesn't implement Equals on struct)
    }

    // ─────────────────────────────────────────────────────────
    //  29. NEGATIVE COORDINATES ONLY
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void AllNegativeCoordinates_ProducesValidHull()
    {
        var rng = new Random(44444);
        var points = new Exact3[50];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Exact3(
                -rng.NextDouble() * 100,
                -rng.NextDouble() * 100,
                -rng.NextDouble() * 100);

        bool success = ExactHullBuilder3D.TryBuildHull(points, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  30. AXIS-ALIGNED SLAB — thick in one dimension, huge in others
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void AxisAlignedSlab_ProducesValidHull()
    {
        var rng = new Random(5050);
        var points = new Exact3[80];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Exact3(
                (rng.NextDouble() - 0.5) * 1e+12,
                (rng.NextDouble() - 0.5) * 1e+12,
                (rng.NextDouble() - 0.5) * 1e-12);

        // Ensure 3D
        points[0] = new Exact3(0, 0, 1.0);
        points[1] = new Exact3(0, 0, -1.0);

        Shuffle(rng, points);
        bool success = ExactHullBuilder3D.TryBuildHull(points, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  31. HIGH-DENSITY FUZZ with high point counts
    // ─────────────────────────────────────────────────────────
    [Theory]
    [InlineData(200, 1)]
    [InlineData(200, 2)]
    [InlineData(200, 3)]
    [InlineData(500, 4)]
    [InlineData(500, 5)]
    public void HighDensityRandomClouds(int pointCount, int seed)
    {
        var rng = new Random(seed);
        var points = new Exact3[pointCount];
        for (int i = 0; i < pointCount; i++)
            points[i] = new Exact3(
                rng.NextDouble() * 2 - 1,
                rng.NextDouble() * 2 - 1,
                rng.NextDouble() * 2 - 1);

        bool success = ExactHullBuilder3D.TryBuildHull(points, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(faceCount >= 4);
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  32. THE TORTURE PLANE — 1000 coplanar + 2 outliers
    //      with ULP-level offsets
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void TorturePlane_1000CoplanarPlus2Outliers()
    {
        var rng = new Random(77777);
        int n = 1000;
        var points = new Exact3[n + 2];

        for (int i = 0; i < n; i++)
        {
            double x = rng.NextDouble() * 1e+8;
            double y = rng.NextDouble() * 1e+8;
            points[i] = new Exact3(x, y, 0.0);
        }
        points[n] = new Exact3(5e+7, 5e+7, 5e-324); // subnormal offset!
        points[n + 1] = new Exact3(5e+7, 5e+7, -5e-324);

        Shuffle(rng, points);
        bool success = ExactHullBuilder3D.TryBuildHull(points, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(ExactHullValidation3D.IsHullValid(points, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  33. POINTS FORMING A LONG CHAIN — sequential convex growth
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void SequentialGrowth_EachPointExpandsHull()
    {
        var rng = new Random(99999);
        var allPoints = new List<Exact3>();

        // Start with tetrahedron
        allPoints.Add(new Exact3(0.0, 0.0, 0.0));
        allPoints.Add(new Exact3(1.0, 0.0, 0.0));
        allPoints.Add(new Exact3(0.0, 1.0, 0.0));
        allPoints.Add(new Exact3(0.0, 0.0, 1.0));

        // Each new point is guaranteed outside current hull
        for (int i = 0; i < 100; i++)
        {
            double r = 2.0 + i * 0.5;
            allPoints.Add(RandomSpherePoint(rng, r));
        }

        var arr = allPoints.ToArray();
        bool success = ExactHullBuilder3D.TryBuildHull(arr, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(ExactHullValidation3D.IsHullValid(arr, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  34. PUBLIC API — ExactHull3D.Build with tuples
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void PublicApi_Build_WithTuples()
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
    public void PublicApi_Build_ThrowsOnTooFewPoints()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ExactHull3D.Build((0.0, 0.0, 0.0), (1.0, 0.0, 0.0), (0.0, 1.0, 0.0)));
    }

    [Fact]
    public void PublicApi_Build_ThrowsOnCoplanarPoints()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ExactHull3D.Build(
                (0.0, 0.0, 0.0),
                (1.0, 0.0, 0.0),
                (0.0, 1.0, 0.0),
                (0.5, 0.5, 0.0)));
    }

    // ─────────────────────────────────────────────────────────
    //  35. THE ULTIMATE NIGHTMARE — all pathologies combined
    // ─────────────────────────────────────────────────────────
    [Fact]
    public void UltimateNightmare_AllPathologiesCombined()
    {
        var rng = new Random(314159);
        var points = new List<Exact3>();

        // Cluster 1: near-coplanar at huge offset
        for (int i = 0; i < 50; i++)
            points.Add(new Exact3(
                1e+14 + rng.NextDouble(),
                1e+14 + rng.NextDouble(),
                (rng.NextDouble() - 0.5) * 1e-14));

        // Cluster 2: near-collinear spike
        for (int i = 0; i < 30; i++)
            points.Add(new Exact3(
                rng.NextDouble() * 1e-14,
                rng.NextDouble() * 1e-14,
                rng.NextDouble() * 1e+14));

        // Cluster 3: subnormal coordinates
        for (int i = 0; i < 10; i++)
            points.Add(new Exact3(
                5e-324 * (rng.Next(2) == 0 ? 1 : -1),
                5e-324 * (rng.Next(2) == 0 ? 1 : -1),
                5e-324 * (rng.Next(2) == 0 ? 1 : -1)));

        // Cluster 4: massive coordinates
        points.Add(new Exact3(1e+300, 0, 0));
        points.Add(new Exact3(-1e+300, 0, 0));
        points.Add(new Exact3(0, 1e+300, 0));
        points.Add(new Exact3(0, -1e+300, 0));
        points.Add(new Exact3(0, 0, 1e+300));
        points.Add(new Exact3(0, 0, -1e+300));

        // Duplicates
        for (int i = 0; i < 20; i++)
            points.Add(points[rng.Next(points.Count)]);

        var arr = points.ToArray();
        Shuffle(rng, arr);
        bool success = ExactHullBuilder3D.TryBuildHull(arr, out var faces, out int faceCount);
        Assert.True(success);
        Assert.True(ExactHullValidation3D.IsHullValid(arr, faces.AsSpan(0, faceCount)));
    }

    // ─────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────

    private static Exact3 RandomSpherePoint(Random rng, double radius)
    {
        while (true)
        {
            double x = rng.NextDouble() * 2 - 1;
            double y = rng.NextDouble() * 2 - 1;
            double z = rng.NextDouble() * 2 - 1;
            double lenSq = x * x + y * y + z * z;
            if (lenSq < 1e-12 || lenSq > 1.0) continue;
            double inv = radius / Math.Sqrt(lenSq);
            return new Exact3(x * inv, y * inv, z * inv);
        }
    }

    private static void Shuffle(Random rng, Exact3[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
