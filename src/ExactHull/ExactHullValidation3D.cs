// ExactHull
// (c) Thorben Linneweber
// SPDX-License-Identifier: MIT

using System;

namespace ExactHull.ExactGeometry
{
    public static class ExactHullValidation3D
    {
        public static bool IsHullValid(
            ReadOnlySpan<Exact3> points,
            ReadOnlySpan<Face> faces)
        {
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
