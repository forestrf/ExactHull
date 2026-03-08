// ExactHull
// (c) Thorben Linneweber
// SPDX-License-Identifier: MIT

using System;

namespace ExactHull.ExactGeometry
{
    public readonly struct Exact3
    {
        public Exact X { get; }
        public Exact Y { get; }
        public Exact Z { get; }

        public static Exact3 Zero => new(Exact.Zero, Exact.Zero, Exact.Zero);

        public Exact3(Exact x, Exact y, Exact z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Exact3(double x, double y, double z)
        {
            X = Exact.FromDouble(x);
            Y = Exact.FromDouble(y);
            Z = Exact.FromDouble(z);
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        public static Exact3 operator +(Exact3 a, Exact3 b)
            => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Exact3 operator -(Exact3 a, Exact3 b)
            => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Exact3 operator -(Exact3 v)
            => new(-v.X, -v.Y, -v.Z);
    }
}
