// ExactHull
// (c) Thorben Linneweber
// SPDX-License-Identifier: MIT

using System;

namespace ExactHull.ExactGeometry
{
    /// <summary>
    /// A triangular face of the convex hull, defined by three vertex indices
    /// into <see cref="Hull3D.Points"/>.
    /// </summary>
    public struct Face
    {
        /// <summary>Index of the first vertex.</summary>
        public int A;
        /// <summary>Index of the second vertex.</summary>
        public int B;
        /// <summary>Index of the third vertex.</summary>
        public int C;

        /// <summary>Creates a face from three vertex indices.</summary>
        public Face(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }
    }

    internal readonly struct Edge
    {
        public int A { get; }
        public int B { get; }

        public Edge(int a, int b)
        {
            A = a;
            B = b;
        }
    }

    internal static class ExactHullTopology3D
    {
        public static bool IsFaceVisible(
            ReadOnlySpan<Exact3> points,
            in Face face,
            in Exact3 point)
        {
            return ExactGeometry3D.Orient3D(
                points[face.A],
                points[face.B],
                points[face.C],
                point).Sign() > 0;
        }

        public static int CollectVisibleFaces(
            ReadOnlySpan<Exact3> points,
            ReadOnlySpan<Face> faces,
            in Exact3 point,
            Span<int> visibleFaceIndices)
        {
            int count = 0;

            for (int i = 0; i < faces.Length; i++)
            {
                if (IsFaceVisible(points, faces[i], point))
                {
                    visibleFaceIndices[count++] = i;
                }
            }

            return count;
        }

        public static void CreateInitialTetrahedronFaces(
            ReadOnlySpan<Exact3> points,
            int i0,
            int i1,
            int i2,
            int i3,
            Span<Face> faces)
        {
            if (faces.Length < 4)
                throw new ArgumentException("Need space for 4 faces.", nameof(faces));

            faces[0] = CreateOrientedFace(points, i0, i1, i2, i3);
            faces[1] = CreateOrientedFace(points, i0, i3, i1, i2);
            faces[2] = CreateOrientedFace(points, i0, i2, i3, i1);
            faces[3] = CreateOrientedFace(points, i1, i3, i2, i0);
        }

        public static Face CreateOrientedFace(
            ReadOnlySpan<Exact3> points,
            int a,
            int b,
            int c,
            int opposite)
        {
            Exact orient = ExactGeometry3D.Orient3D(points[a], points[b], points[c], points[opposite]);

            if (orient.IsZero())
                throw new ArgumentException("Face is degenerate or opposite point is coplanar.");

            // We want opposite point on the negative side.
            if (orient.Sign() > 0)
                return new Face(a, c, b);

            return new Face(a, b, c);
        }

        public static int CollectHorizonEdges(
            ReadOnlySpan<Face> faces,
            ReadOnlySpan<int> visibleFaceIndices,
            Span<Edge> horizonEdges)
        {
            int horizonCount = 0;

            for (int i = 0; i < visibleFaceIndices.Length; i++)
            {
                Face face = faces[visibleFaceIndices[i]];

                AddOrCancelEdge(new Edge(face.A, face.B), horizonEdges, ref horizonCount);
                AddOrCancelEdge(new Edge(face.B, face.C), horizonEdges, ref horizonCount);
                AddOrCancelEdge(new Edge(face.C, face.A), horizonEdges, ref horizonCount);
            }

            return horizonCount;
        }

        public static int CreateFacesFromHorizon(
            ReadOnlySpan<Exact3> points,
            ReadOnlySpan<Edge> horizonEdges,
            int pointIndex,
            in Exact3 insidePoint,
            Span<Face> newFaces)
        {
            if (newFaces.Length < horizonEdges.Length)
                throw new ArgumentException("newFaces buffer is too small.", nameof(newFaces));

            int count = 0;

            for (int i = 0; i < horizonEdges.Length; i++)
            {
                Edge edge = horizonEdges[i];
                newFaces[count++] = CreateOrientedFace(points, edge.A, edge.B, pointIndex, insidePoint);
            }

            return count;
        }

        public static Face CreateOrientedFace(
            ReadOnlySpan<Exact3> points,
            int a,
            int b,
            int c,
            in Exact3 insidePoint)
        {
            Exact orient = ExactGeometry3D.Orient3D(points[a], points[b], points[c], insidePoint);

            if (orient.IsZero())
                throw new ArgumentException("insidePoint lies on the candidate face plane.");

            // We want the inside point on the negative side.
            if (orient.Sign() > 0)
                return new Face(a, c, b);

            return new Face(a, b, c);
        }

        public static int ExpandHullByPoint(
            ReadOnlySpan<Exact3> points,
            Span<Face> faces,
            int faceCount,
            int pointIndex,
            in Exact3 insidePoint,
            Span<int> visibleFaceIndices,
            Span<Edge> horizonEdges,
            Span<Face> newFaces)
        {
            if (faceCount < 0 || faceCount > faces.Length)
                throw new ArgumentOutOfRangeException(nameof(faceCount));

            int visibleCount = CollectVisibleFaces(
                points,
                faces[..faceCount],
                points[pointIndex],
                visibleFaceIndices);

            if (visibleCount == 0)
                return faceCount;

            int horizonCount = CollectHorizonEdges(
                faces[..faceCount],
                visibleFaceIndices[..visibleCount],
                horizonEdges);

            int newFaceCount = CreateFacesFromHorizon(
                points,
                horizonEdges[..horizonCount],
                pointIndex,
                insidePoint,
                newFaces);

            int write = 0;

            for (int read = 0; read < faceCount; read++)
            {
                if (!Contains(visibleFaceIndices[..visibleCount], read))
                {
                    faces[write++] = faces[read];
                }
            }

            for (int i = 0; i < newFaceCount; i++)
            {
                faces[write++] = newFaces[i];
            }

            return write;
        }

        private static void AddOrCancelEdge(
            Edge edge,
            Span<Edge> edges,
            ref int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (edges[i].A == edge.B && edges[i].B == edge.A)
                {
                    edges[i] = edges[count - 1];
                    count--;
                    return;
                }
            }

            edges[count] = edge;
            count++;
        }

        private static bool Contains(ReadOnlySpan<int> values, int value)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == value)
                    return true;
            }

            return false;
        }
    }
}
