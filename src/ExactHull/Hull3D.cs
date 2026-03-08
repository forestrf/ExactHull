// ExactHull
// (c) Thorben Linneweber
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using ExactHull.ExactGeometry;

namespace ExactHull;

/// <summary>
/// The result of a convex hull computation. Contains the hull vertices as exact points
/// and the triangular faces that reference them by index.
/// </summary>
public sealed class Hull3D
{
    /// <summary>The vertices of the convex hull in exact representation.</summary>
    public Exact3[] Points { get; }

    /// <summary>
    /// The triangular faces of the convex hull. Each face contains three indices (A, B, C)
    /// into <see cref="Points"/>.
    /// </summary>
    public Face[] Faces { get; }

    public Hull3D(Exact3[] points, Face[] faces)
    {
        Points = points ?? throw new ArgumentNullException(nameof(points));
        Faces = faces ?? throw new ArgumentNullException(nameof(faces));
    }
}

/// <summary>
/// Builds exact 3D convex hulls using exact arithmetic.
/// All geometric predicates are evaluated without floating-point rounding.
/// </summary>
public static class ExactHull3D
{
    /// <summary>
    /// Builds a convex hull from points in exact representation.
    /// </summary>
    /// <param name="points">At least 4 non-coplanar points.</param>
    /// <returns>The convex hull.</returns>
    /// <exception cref="InvalidOperationException">Thrown if fewer than 4 points are given or all points are coplanar.</exception>
    public static Hull3D Build(IReadOnlyList<Exact3> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        if (points.Count < 4)
            throw new InvalidOperationException("At least 4 points are required to build a 3D convex hull.");

        var pointArray = new Exact3[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            pointArray[i] = points[i];
        }

        if (!ExactHullBuilder3D.TryBuildHull(pointArray, out Face[] faces, out int faceCount))
            throw new InvalidOperationException("Could not build a 3D convex hull from the given points.");

        if (faceCount != faces.Length)
        {
            var trimmed = new Face[faceCount];
            Array.Copy(faces, trimmed, faceCount);
            faces = trimmed;
        }

        return new Hull3D(pointArray, faces);
    }

    /// <inheritdoc cref="Build(IReadOnlyList{Exact3})"/>
    public static Hull3D Build(params Exact3[] points)
    {
        ArgumentNullException.ThrowIfNull(points);
        return Build((IReadOnlyList<Exact3>)points);
    }

    /// <summary>
    /// Builds a convex hull from points given as <c>(double X, double Y, double Z)</c> tuples.
    /// Each double is converted to an exact dyadic rational before computation.
    /// </summary>
    /// <param name="points">At least 4 non-coplanar points.</param>
    /// <returns>The convex hull.</returns>
    /// <exception cref="InvalidOperationException">Thrown if fewer than 4 points are given or all points are coplanar.</exception>
    public static Hull3D Build(IReadOnlyList<(double X, double Y, double Z)> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        var exactPoints = new Exact3[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            var p = points[i];
            exactPoints[i] = new Exact3(p.X, p.Y, p.Z);
        }

        return Build((IReadOnlyList<Exact3>)exactPoints);
    }

    /// <inheritdoc cref="Build(IReadOnlyList{ValueTuple{double, double, double}})"/>
    public static Hull3D Build(params (double X, double Y, double Z)[] points)
    {
        ArgumentNullException.ThrowIfNull(points);
        return Build((IReadOnlyList<(double X, double Y, double Z)>)points);
    }

    /// <summary>
    /// Builds a convex hull from a list of any point type using a selector to extract coordinates.
    /// </summary>
    /// <typeparam name="T">The point type (e.g. <c>Vector3</c>).</typeparam>
    /// <param name="points">At least 4 non-coplanar points.</param>
    /// <param name="selector">A function that extracts <c>(X, Y, Z)</c> coordinates from each point.</param>
    /// <returns>The convex hull.</returns>
    /// <exception cref="InvalidOperationException">Thrown if fewer than 4 points are given or all points are coplanar.</exception>
    public static Hull3D Build<T>(IReadOnlyList<T> points, Func<T, (double X, double Y, double Z)> selector)
    {
        ArgumentNullException.ThrowIfNull(points);
        ArgumentNullException.ThrowIfNull(selector);

        var tuples = new (double X, double Y, double Z)[points.Count];
        for (int i = 0; i < points.Count; i++)
            tuples[i] = selector(points[i]);

        return Build((IReadOnlyList<(double X, double Y, double Z)>)tuples);
    }

#if !EXCLUDE_BRUTE_FORCE
    public static Hull3D BuildBruteForce(IReadOnlyList<Exact3> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        if (points.Count < 4)
            throw new InvalidOperationException("At least 4 points are required to build a 3D convex hull.");

        var pointArray = new Exact3[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            pointArray[i] = points[i];
        }

        var faceBuffer = new Face[Math.Max(16, pointArray.Length * 4)];

        if (!ExactHullBruteForceBuilder3D.TryBuildHull(pointArray, faceBuffer, out int faceCount))
            throw new InvalidOperationException("Could not build a 3D convex hull from the given points.");

        var faces = new Face[faceCount];
        Array.Copy(faceBuffer, faces, faceCount);

        return new Hull3D(pointArray, faces);
    }

    public static Hull3D BuildBruteForce(params Exact3[] points)
    {
        ArgumentNullException.ThrowIfNull(points);
        return BuildBruteForce((IReadOnlyList<Exact3>)points);
    }

    public static Hull3D BuildBruteForce(IReadOnlyList<(double X, double Y, double Z)> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        var exactPoints = new Exact3[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            var p = points[i];
            exactPoints[i] = new Exact3(p.X, p.Y, p.Z);
        }

        return BuildBruteForce((IReadOnlyList<Exact3>)exactPoints);
    }

    public static Hull3D BuildBruteForce(params (double X, double Y, double Z)[] points)
    {
        ArgumentNullException.ThrowIfNull(points);
        return BuildBruteForce((IReadOnlyList<(double X, double Y, double Z)>)points);
    }
#endif
}
