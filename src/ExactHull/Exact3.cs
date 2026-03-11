// ExactHull
// (c) Thorben Linneweber
// SPDX-License-Identifier: MIT

using System;

namespace ExactHull.ExactGeometry
{
    /// <summary>
    /// A 3D point with exact dyadic rational coordinates.
    /// </summary>
    public readonly struct Exact3
    {
        /// <summary>The exact X coordinate.</summary>
        public Exact X { get; }
        /// <summary>The exact Y coordinate.</summary>
        public Exact Y { get; }
        /// <summary>The exact Z coordinate.</summary>
        public Exact Z { get; }

        /// <summary>The origin (0, 0, 0).</summary>
        public static Exact3 Zero => new(Exact.Zero, Exact.Zero, Exact.Zero);

        /// <summary>Creates a point from exact coordinates.</summary>
        public Exact3(Exact x, Exact y, Exact z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Creates a point from double coordinates. Each value is converted
        /// to an exact dyadic rational.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if any coordinate is NaN or Infinity.</exception>
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
