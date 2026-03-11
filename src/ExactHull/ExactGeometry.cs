// ExactHull
// (c) Thorben Linneweber
// SPDX-License-Identifier: MIT

namespace ExactHull.ExactGeometry
{
    internal static class ExactGeometry3D
    {
        public static Exact Dot(Exact3 a, Exact3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static Exact3 Cross(Exact3 a, Exact3 b)
        {
            return new Exact3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);
        }

        public static Exact Orient3D(Exact3 a, Exact3 b, Exact3 c, Exact3 d)
        {
            Exact3 ab = b - a;
            Exact3 ac = c - a;
            Exact3 ad = d - a;

            return Dot(Cross(ab, ac), ad);
        }
    }
}
