// ExactHull
// (c) Thorben Linneweber
// SPDX-License-Identifier: MIT

#if INCLUDE_BRUTE_FORCE
using System;

namespace ExactHull.ExactGeometry
{
    public static class ExactHullBruteForceBuilder3D
    {
        public static bool TryBuildHull(
            ReadOnlySpan<Exact3> points,
            Span<Face> faces,
            out int faceCount)
        {
            faceCount = 0;

            if (points.Length < 4)
                return false;

            if (!TryFindInitialTetrahedron(points, out int i0, out int i1, out int i2, out int i3))
                return false;

            if (faces.Length < points.Length * 2)
                throw new ArgumentException("faces buffer is too small.", nameof(faces));

            ExactHullTopology3D.CreateInitialTetrahedronFaces(points, i0, i1, i2, i3, faces);
            faceCount = 4;

            Exact3 insidePoint = ComputeCentroid(points[i0], points[i1], points[i2], points[i3]);

            Span<int> visibleFaceIndices = stackalloc int[faces.Length];
            Span<Edge> horizonEdges = stackalloc Edge[faces.Length * 2];
            Span<Face> newFaces = stackalloc Face[faces.Length * 2];

            for (int pointIndex = 0; pointIndex < points.Length; pointIndex++)
            {
                if (pointIndex == i0 || pointIndex == i1 || pointIndex == i2 || pointIndex == i3)
                    continue;

                faceCount = ExactHullTopology3D.ExpandHullByPoint(
                    points,
                    faces,
                    faceCount,
                    pointIndex,
                    insidePoint,
                    visibleFaceIndices,
                    horizonEdges,
                    newFaces);
            }

            return true;
        }

        public static bool TryFindInitialTetrahedron(
            ReadOnlySpan<Exact3> points,
            out int i0,
            out int i1,
            out int i2,
            out int i3)
        {
            i0 = i1 = i2 = i3 = -1;

            if (points.Length < 4)
                return false;

            // 1. Find two distinct points.
            i0 = 0;

            for (int i = 1; i < points.Length; i++)
            {
                if (!AreEqual(points[i0], points[i]))
                {
                    i1 = i;
                    break;
                }
            }

            if (i1 < 0)
                return false;

            // 2. Find a third point that is not collinear with i0, i1.
            Exact3 a = points[i0];
            Exact3 b = points[i1];
            Exact3 ab = b - a;

            for (int i = 0; i < points.Length; i++)
            {
                if (i == i0 || i == i1)
                    continue;

                Exact3 ac = points[i] - a;
                Exact3 cross = ExactGeometry3D.Cross(ab, ac);

                if (!IsZero(cross))
                {
                    i2 = i;
                    break;
                }
            }

            if (i2 < 0)
                return false;

            // 3. Find a fourth point that is not coplanar with i0, i1, i2.
            Exact3 c = points[i2];

            for (int i = 0; i < points.Length; i++)
            {
                if (i == i0 || i == i1 || i == i2)
                    continue;

                Exact orientation = ExactGeometry3D.Orient3D(a, b, c, points[i]);

                if (!orientation.IsZero())
                {
                    i3 = i;
                    return true;
                }
            }

            i0 = i1 = i2 = i3 = -1;
            return false;
        }

        private static Exact3 ComputeCentroid(in Exact3 a, in Exact3 b, in Exact3 c, in Exact3 d)
        {
            Exact quarter = Exact.One * Exact.FromDouble(0.25);

            Exact x = (a.X + b.X + c.X + d.X) * quarter;
            Exact y = (a.Y + b.Y + c.Y + d.Y) * quarter;
            Exact z = (a.Z + b.Z + c.Z + d.Z) * quarter;

            return new Exact3(x, y, z);
        }

        private static bool AreEqual(in Exact3 a, in Exact3 b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        private static bool IsZero(in Exact3 v)
        {
            return v.X.IsZero() && v.Y.IsZero() && v.Z.IsZero();
        }
    }
}
#endif
