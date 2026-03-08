// ExactHull
// (c) Thorben Linneweber
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using ExactHull.ExactGeometry;

namespace ExactHull;

public sealed class Hull3D
{
    public Exact3[] Points { get; }
    public Face[] Faces { get; }

    public Hull3D(Exact3[] points, Face[] faces)
    {
        Points = points ?? throw new ArgumentNullException(nameof(points));
        Faces = faces ?? throw new ArgumentNullException(nameof(faces));
    }
}

public static class ExactHull3D
{
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

    public static Hull3D Build(params Exact3[] points)
    {
        ArgumentNullException.ThrowIfNull(points);
        return Build((IReadOnlyList<Exact3>)points);
    }

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

    public static Hull3D Build(params (double X, double Y, double Z)[] points)
    {
        ArgumentNullException.ThrowIfNull(points);
        return Build((IReadOnlyList<(double X, double Y, double Z)>)points);
    }
    

#if INCLUDE_BRUTE_FORCE
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
