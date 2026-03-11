// ExactHull
// (c) Thorben Linneweber
// SPDX-License-Identifier: MIT

using System;

namespace ExactHull.ExactGeometry
{
    public static class ExactHullValidation3D
    {
        /// <summary>
        /// Validates that the given faces form a correct convex hull over the point set.
        /// Checks that: at least 4 non-degenerate triangular faces exist; no point lies
        /// strictly above any face; and every directed edge has exactly one reverse-twin,
        /// ensuring the mesh is a closed, orientable 2-manifold.
        /// </summary>
        /// <param name="points">The full point set (hull and interior points).</param>
        /// <param name="faces">The triangular faces to validate.</param>
        /// <returns><c>true</c> if the faces form a valid closed convex hull; otherwise <c>false</c>.</returns>
        public static bool IsHullValid(
            ReadOnlySpan<Exact3> points,
            ReadOnlySpan<Face> faces)
        {
            // A convex polyhedron needs at least 4 triangular faces.
            if (faces.Length < 4)
                return false;

            // Euler's formula for a closed convex polyhedron:
            // V - E + F = 2, and for a triangulated surface E = 3F/2.
            // So F must be even, and every edge must be shared by exactly 2 faces.
            if (faces.Length % 2 != 0)
                return false;

            for (int i = 0; i < faces.Length; i++)
            {
                Face face = faces[i];

                if (!IsValidVertexIndex(face.A, points.Length) ||
                    !IsValidVertexIndex(face.B, points.Length) ||
                    !IsValidVertexIndex(face.C, points.Length))
                {
                    return false;
                }

                if (face.A == face.B || face.A == face.C || face.B == face.C)
                    return false;

                Exact3 a = points[face.A];
                Exact3 b = points[face.B];
                Exact3 c = points[face.C];

                Exact3 ab = b - a;
                Exact3 ac = c - a;
                Exact3 cross = ExactGeometry3D.Cross(ab, ac);

                if (IsZero(cross))
                    return false;

                for (int p = 0; p < points.Length; p++)
                {
                    Exact orient = ExactGeometry3D.Orient3D(a, b, c, points[p]);

                    if (orient.Sign() > 0)
                        return false;
                }
            }

            // Verify closed manifold: every directed edge (A→B) of a face must
            // have exactly one matching reverse edge (B→A) in another face.
            // This ensures the mesh is closed and non-degenerate.
            int edgeCount = faces.Length * 3;
            int[] edgeFrom = new int[edgeCount];
            int[] edgeTo = new int[edgeCount];

            for (int i = 0; i < faces.Length; i++)
            {
                edgeFrom[i * 3 + 0] = faces[i].A; edgeTo[i * 3 + 0] = faces[i].B;
                edgeFrom[i * 3 + 1] = faces[i].B; edgeTo[i * 3 + 1] = faces[i].C;
                edgeFrom[i * 3 + 2] = faces[i].C; edgeTo[i * 3 + 2] = faces[i].A;
            }

            for (int i = 0; i < edgeCount; i++)
            {
                int matchCount = 0;
                for (int j = 0; j < edgeCount; j++)
                {
                    if (j == i) continue;
                    if (edgeFrom[j] == edgeTo[i] && edgeTo[j] == edgeFrom[i])
                        matchCount++;
                }

                if (matchCount != 1)
                    return false;
            }

            return true;
        }

        private static bool IsValidVertexIndex(int index, int pointCount)
        {
            return index >= 0 && index < pointCount;
        }

        private static bool IsZero(in Exact3 v)
        {
            return v.X.IsZero() && v.Y.IsZero() && v.Z.IsZero();
        }
    }
}
