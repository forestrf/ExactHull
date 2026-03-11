// ExactHull
// (c) Thorben Linneweber
// SPDX-License-Identifier: MIT

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace ExactHull.ExactGeometry
{
    /// <summary>
    /// Exact dyadic rational:
    /// Value = Mantissa * 2^Exponent
    ///
    /// This can represent every double exactly.
    /// It is closed under +, -, *.
    /// </summary>
    public readonly struct Exact : IEquatable<Exact>, IComparable<Exact>
    {
        /// <summary>The mantissa of the dyadic rational. Value = Mantissa × 2^Exponent.</summary>
        public BigInteger Mantissa { get; }
        /// <summary>The binary exponent. Value = Mantissa × 2^Exponent.</summary>
        public int Exponent { get; }

        /// <summary>The additive identity (0).</summary>
        public static Exact Zero => new(BigInteger.Zero, 0);
        /// <summary>The multiplicative identity (1).</summary>
        public static Exact One => new(BigInteger.One, 0);

        /// <summary>Creates an exact value from a mantissa and binary exponent.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Exact(BigInteger mantissa, int exponent)
        {
            Normalize(mantissa, exponent, out mantissa, out exponent);
            Mantissa = mantissa;
            Exponent = exponent;
        }

        /// <summary>Creates an exact value from an integer.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Exact(int value) : this(new BigInteger(value), 0) { }

        /// <summary>Creates an exact value from a long.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Exact(long value) : this(new BigInteger(value), 0) { }

        /// <summary>
        /// Attempts to convert this exact value back to a double.
        /// Returns <c>false</c> if the value overflows or is not representable.
        /// </summary>
        public bool TryToDouble(out double value)
        {
            if (Mantissa.IsZero)
            {
                value = 0.0;
                return true;
            }

            try
            {
                value = (double)Mantissa * Math.Pow(2.0, Exponent);

                if (double.IsInfinity(value) || double.IsNaN(value))
                {
                    value = 0.0;
                    return false;
                }

                return true;
            }
            catch
            {
                value = 0.0;
                return false;
            }
        }
        
        public override string ToString()
        {
            if (Mantissa.IsZero) return "0";

            double approx;
            bool hasApprox = TryToDouble(out approx);

            if (Exponent == 0)
            {
                return hasApprox
                    ? $"{Mantissa} (~ {approx:R})"
                    : Mantissa.ToString();
            }

            return hasApprox
                ? $"{Mantissa} * 2^{Exponent} (~ {approx:R})"
                : $"{Mantissa} * 2^{Exponent}";
        }

        /// <summary>
        /// Converts an IEEE-754 double to its exact dyadic rational representation.
        /// Every finite double can be represented exactly.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is NaN or Infinity.</exception>
        public static Exact FromDouble(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("NaN and Infinity are not supported.", nameof(value));

            long bits = BitConverter.DoubleToInt64Bits(value);

            bool negative = (bits & (1L << 63)) != 0;
            int rawExponent = (int)((bits >> 52) & 0x7FFL);
            long rawFraction = bits & 0x000F_FFFF_FFFF_FFFFL;

            if (rawExponent == 0 && rawFraction == 0)
                return Zero;

            BigInteger mantissa;
            int exponent;

            if (rawExponent == 0)
            {
                // Subnormal:
                // value = fraction * 2^(-1074)
                mantissa = rawFraction;
                exponent = -1074;
            }
            else
            {
                // Normal:
                // value = (2^52 + fraction) * 2^(rawExponent - 1023 - 52)
                mantissa = (1L << 52) | rawFraction;
                exponent = rawExponent - 1023 - 52;
            }

            if (negative)
                mantissa = BigInteger.Negate(mantissa);

            return new Exact(mantissa, exponent);
        }

        /// <summary>Returns −1, 0, or 1 indicating the sign of this value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Sign()
        {
            return Mantissa.Sign;
        }

        /// <summary>Returns <c>true</c> if this value is exactly zero.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsZero()
        {
            return Mantissa.IsZero;
        }

        /// <summary>Returns the absolute value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Exact Abs()
        {
            return Mantissa.Sign >= 0 ? this : new Exact(BigInteger.Abs(Mantissa), Exponent);
        }

        /// <inheritdoc />
        public int CompareTo(Exact other)
        {
            if (Mantissa.Sign != other.Mantissa.Sign)
                return Mantissa.Sign.CompareTo(other.Mantissa.Sign);

            if (Mantissa.IsZero)
                return 0;

            if (Exponent == other.Exponent)
                return Mantissa.CompareTo(other.Mantissa);

            int minExponent = Math.Min(Exponent, other.Exponent);
            BigInteger left = ShiftByPowerOfTwo(Mantissa, Exponent - minExponent);
            BigInteger right = ShiftByPowerOfTwo(other.Mantissa, other.Exponent - minExponent);
            return left.CompareTo(right);
        }

        public bool Equals(Exact other)
        {
            return Mantissa.Equals(other.Mantissa) && Exponent == other.Exponent;
        }

        public override bool Equals(object? obj)
        {
            return obj is Exact other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Mantissa, Exponent);
        }

        /// <summary>
        /// Converts this exact value to a double (lossy). Intended for debugging and inspection.
        /// </summary>
        public double ToDouble()
        {
            return (double)Mantissa * Math.Pow(2.0, Exponent);
        }

        public static Exact operator +(Exact a, Exact b)
        {
            if (a.Mantissa.IsZero) return b;
            if (b.Mantissa.IsZero) return a;

            int minExponent = Math.Min(a.Exponent, b.Exponent);

            BigInteger am = ShiftByPowerOfTwo(a.Mantissa, a.Exponent - minExponent);
            BigInteger bm = ShiftByPowerOfTwo(b.Mantissa, b.Exponent - minExponent);

            return new Exact(am + bm, minExponent);
        }

        public static Exact operator -(Exact a, Exact b)
        {
            if (b.Mantissa.IsZero) return a;
            if (a.Mantissa.IsZero) return new Exact(BigInteger.Negate(b.Mantissa), b.Exponent);

            int minExponent = Math.Min(a.Exponent, b.Exponent);

            BigInteger am = ShiftByPowerOfTwo(a.Mantissa, a.Exponent - minExponent);
            BigInteger bm = ShiftByPowerOfTwo(b.Mantissa, b.Exponent - minExponent);

            return new Exact(am - bm, minExponent);
        }

        public static Exact operator -(Exact value)
        {
            return new Exact(BigInteger.Negate(value.Mantissa), value.Exponent);
        }

        public static Exact operator *(Exact a, Exact b)
        {
            if (a.Mantissa.IsZero || b.Mantissa.IsZero)
                return Zero;

            return new Exact(a.Mantissa * b.Mantissa, checked(a.Exponent + b.Exponent));
        }

        public static bool operator ==(Exact left, Exact right) => left.Equals(right);
        public static bool operator !=(Exact left, Exact right) => !left.Equals(right);
        public static bool operator <(Exact left, Exact right) => left.CompareTo(right) < 0;
        public static bool operator >(Exact left, Exact right) => left.CompareTo(right) > 0;
        public static bool operator <=(Exact left, Exact right) => left.CompareTo(right) <= 0;
        public static bool operator >=(Exact left, Exact right) => left.CompareTo(right) >= 0;

        public static implicit operator Exact(int value) => new(value);
        public static implicit operator Exact(long value) => new(value);

        private static void Normalize(
            BigInteger mantissa,
            int exponent,
            out BigInteger normalizedMantissa,
            out int normalizedExponent)
        {
            if (mantissa.IsZero)
            {
                normalizedMantissa = BigInteger.Zero;
                normalizedExponent = 0;
                return;
            }

            int trailingZeros = CountTrailingBinaryZeros(mantissa);
            if (trailingZeros > 0)
            {
                mantissa >>= trailingZeros;
                exponent = checked(exponent + trailingZeros);
            }

            normalizedMantissa = mantissa;
            normalizedExponent = exponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BigInteger ShiftByPowerOfTwo(BigInteger value, int shift)
        {
            if (shift < 0)
                throw new ArgumentOutOfRangeException(nameof(shift), "Shift must be non-negative.");
            return value << shift;
        }

        private static int CountTrailingBinaryZeros(BigInteger value)
        {
            if (value.IsZero) return 0;

            value = BigInteger.Abs(value);

            int count = 0;
            while ((value & BigInteger.One).IsZero)
            {
                value >>= 1;
                count++;
            }

            return count;
        }
    }
}
