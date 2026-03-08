// ExactHull
// (c) Thorben Linneweber
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;

namespace ExactHull.ExactGeometry
{
    internal static class ExactHullBuilder3D
    {
        public static bool TryBuildHull(
            ReadOnlySpan<Exact3> points,
            out Face[] faces,
            out int faceCount)
        {
            faces = Array.Empty<Face>();
            faceCount = 0;

            if (points.Length < 4)
                return false;

            if (!TryFindInitialTetrahedron(points, out int i0, out int i1, out int i2, out int i3))
                return false;

            Exact3 insidePoint = ComputeCentroid(points[i0], points[i1], points[i2], points[i3]);

            // Build initial tetrahedron with adjacency.
            Span<Face> initial = stackalloc Face[4];
            ExactHullTopology3D.CreateInitialTetrahedronFaces(points, i0, i1, i2, i3, initial);

            var face0 = new HullFace(initial[0].A, initial[0].B, initial[0].C);
            var face1 = new HullFace(initial[1].A, initial[1].B, initial[1].C);
            var face2 = new HullFace(initial[2].A, initial[2].B, initial[2].C);
            var face3 = new HullFace(initial[3].A, initial[3].B, initial[3].C);

            HullFace[] tetra = { face0, face1, face2, face3 };
            LinkTetrahedron(tetra);

            var activeFaces = new List<HullFace>(16) { face0, face1, face2, face3 };
            var pendingFaces = new Stack<HullFace>(16);

            // Initial point assignment.
            for (int pointIndex = 0; pointIndex < points.Length; pointIndex++)
            {
                if (pointIndex == i0 || pointIndex == i1 || pointIndex == i2 || pointIndex == i3)
                    continue;

                AssignPointToFace(points, activeFaces, pointIndex);
            }

            // Seed the pending stack.
            for (int i = 0; i < activeFaces.Count; i++)
            {
                if (activeFaces[i].OutsidePoints.Count > 0)
                    pendingFaces.Push(activeFaces[i]);
            }

            var visibleFaces = new List<HullFace>(16);
            var horizonEntries = new List<HorizonEntry>(32);
            var bfsQueue = new Queue<HullFace>(16);
            var newFaces = new List<HullFace>(32);
            var reassignmentCandidates = new List<int>(64);

            while (pendingFaces.Count > 0)
            {
                HullFace seedFace = pendingFaces.Pop();
                if (seedFace.Deleted || seedFace.OutsidePoints.Count == 0)
                    continue;

                int pointIndex = PopFarthestPoint(points, seedFace);

                // BFS from seedFace to find visible faces and horizon edges.
                visibleFaces.Clear();
                horizonEntries.Clear();

                CollectVisibleAndHorizon(points, seedFace, points[pointIndex], bfsQueue,
                    visibleFaces, horizonEntries);

                if (visibleFaces.Count == 0)
                    continue;

                // Gather reassignment candidates from visible faces.
                reassignmentCandidates.Clear();
                for (int i = 0; i < visibleFaces.Count; i++)
                {
                    HullFace vf = visibleFaces[i];
                    for (int j = 0; j < vf.OutsidePoints.Count; j++)
                    {
                        int candidate = vf.OutsidePoints[j];
                        if (candidate != pointIndex)
                            reassignmentCandidates.Add(candidate);
                    }

                    vf.OutsidePoints.Clear();
                    vf.Deleted = true;
                }

                // Create new faces from horizon edges.
                newFaces.Clear();
                for (int i = 0; i < horizonEntries.Count; i++)
                {
                    HorizonEntry he = horizonEntries[i];

                    Face oriented = ExactHullTopology3D.CreateOrientedFace(
                        points, he.EdgeA, he.EdgeB, pointIndex, insidePoint);

                    var nf = new HullFace(oriented.A, oriented.B, oriented.C);

                    // Link to surviving neighbor across the horizon edge.
                    int nfEdge = nf.FindEdgeIndexUnordered(he.EdgeA, he.EdgeB);
                    nf.SetNeighbor(nfEdge, he.Survivor);
                    he.Survivor.SetNeighbor(he.SurvivorEdgeIndex, nf);

                    newFaces.Add(nf);
                }

                // Link new faces to each other around the apex fan.
                LinkApexFan(newFaces, pointIndex);

                // Add to active set.
                for (int i = 0; i < newFaces.Count; i++)
                    activeFaces.Add(newFaces[i]);

                // Reassign orphaned points only to new faces.
                for (int i = 0; i < reassignmentCandidates.Count; i++)
                    AssignPointToFace(points, newFaces, reassignmentCandidates[i]);

                // Push new faces that received outside points.
                for (int i = 0; i < newFaces.Count; i++)
                {
                    if (newFaces[i].OutsidePoints.Count > 0)
                        pendingFaces.Push(newFaces[i]);
                }
            }

            CompactFaces(activeFaces, out faces, out faceCount);
            return true;
        }

        private static void CollectVisibleAndHorizon(
            ReadOnlySpan<Exact3> points,
            HullFace seedFace,
            in Exact3 point,
            Queue<HullFace> bfsQueue,
            List<HullFace> visibleFaces,
            List<HorizonEntry> horizonEntries)
        {
            bfsQueue.Clear();
            seedFace.Visited = true;
            bfsQueue.Enqueue(seedFace);

            while (bfsQueue.Count > 0)
            {
                HullFace face = bfsQueue.Dequeue();
                visibleFaces.Add(face);

                for (int ei = 0; ei < 3; ei++)
                {
                    HullFace? neighbor = face.GetNeighbor(ei);
                    if (neighbor is null || neighbor.Deleted || neighbor.Visited)
                        continue;

                    Exact orient = ExactGeometry3D.Orient3D(
                        points[neighbor.A],
                        points[neighbor.B],
                        points[neighbor.C],
                        point);

                    if (orient.Sign() > 0)
                    {
                        neighbor.Visited = true;
                        bfsQueue.Enqueue(neighbor);
                    }
                    else
                    {
                        // Neighbor is not visible — this is a horizon edge.
                        var (edgeFrom, edgeTo) = face.GetEdgeVertices(ei);
                        int survivorEdge = neighbor.FindEdgeIndex(edgeTo, edgeFrom);

                        horizonEntries.Add(new HorizonEntry
                        {
                            EdgeA = edgeFrom,
                            EdgeB = edgeTo,
                            Survivor = neighbor,
                            SurvivorEdgeIndex = survivorEdge
                        });
                    }
                }
            }

            // Reset visited flags.
            for (int i = 0; i < visibleFaces.Count; i++)
                visibleFaces[i].Visited = false;
        }

        private static void LinkApexFan(List<HullFace> newFaces, int apex)
        {
            // Each new face has two edges touching the apex vertex.
            // Two new faces are neighbors if they share an apex edge,
            // which means they share the apex and one horizon vertex.
            // Build a map: horizonVertex -> (face, edgeIndex) for apex edges.

            // Use a simple list-based lookup since the fan is typically small.
            // For each apex edge, the "other" vertex (not apex) identifies the pair.

            Span<int> pendingVertex = stackalloc int[newFaces.Count * 2];
            Span<int> pendingFaceIdx = stackalloc int[newFaces.Count * 2];
            Span<int> pendingEdgeIdx = stackalloc int[newFaces.Count * 2];
            int pendingCount = 0;

            for (int fi = 0; fi < newFaces.Count; fi++)
            {
                HullFace face = newFaces[fi];

                for (int ei = 0; ei < 3; ei++)
                {
                    if (face.GetNeighbor(ei) != null)
                        continue;

                    var (v1, v2) = face.GetEdgeVertices(ei);
                    int horizonVertex = v1 == apex ? v2 : v1;

                    bool found = false;
                    for (int pi = 0; pi < pendingCount; pi++)
                    {
                        if (pendingVertex[pi] == horizonVertex)
                        {
                            HullFace other = newFaces[pendingFaceIdx[pi]];
                            face.SetNeighbor(ei, other);
                            other.SetNeighbor(pendingEdgeIdx[pi], face);

                            pendingVertex[pi] = pendingVertex[pendingCount - 1];
                            pendingFaceIdx[pi] = pendingFaceIdx[pendingCount - 1];
                            pendingEdgeIdx[pi] = pendingEdgeIdx[pendingCount - 1];
                            pendingCount--;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        pendingVertex[pendingCount] = horizonVertex;
                        pendingFaceIdx[pendingCount] = fi;
                        pendingEdgeIdx[pendingCount] = ei;
                        pendingCount++;
                    }
                }
            }
        }

        private static void LinkTetrahedron(HullFace[] faces)
        {
            // Link all 6 shared edges of the 4 tetrahedron faces.
            for (int i = 0; i < 4; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    TryLink(faces[i], faces[j]);
                }
            }
        }

        private static void TryLink(HullFace a, HullFace b)
        {
            // Find a shared edge (appears in opposite winding).
            int[] aVerts = { a.A, a.B, a.C };
            int[] bVerts = { b.A, b.B, b.C };

            for (int ai = 0; ai < 3; ai++)
            {
                int aFrom = aVerts[ai];
                int aTo = aVerts[(ai + 1) % 3];

                int bEdge = b.FindEdgeIndex(aTo, aFrom);
                if (bEdge >= 0)
                {
                    a.SetNeighbor(ai, b);
                    b.SetNeighbor(bEdge, a);
                    return;
                }
            }
        }

        private static int PopFarthestPoint(ReadOnlySpan<Exact3> points, HullFace face)
        {
            int bestIndex = 0;
            int bestPoint = face.OutsidePoints[0];
            Exact bestDistance = SignedDistance(points, face, bestPoint);

            for (int i = 1; i < face.OutsidePoints.Count; i++)
            {
                int pointIndex = face.OutsidePoints[i];
                Exact distance = SignedDistance(points, face, pointIndex);

                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    bestPoint = pointIndex;
                    bestIndex = i;
                }
            }

            // Swap-remove for O(1).
            int last = face.OutsidePoints.Count - 1;
            face.OutsidePoints[bestIndex] = face.OutsidePoints[last];
            face.OutsidePoints.RemoveAt(last);

            return bestPoint;
        }

        private static Exact SignedDistance(ReadOnlySpan<Exact3> points, HullFace face, int pointIndex)
        {
            return ExactGeometry3D.Orient3D(
                points[face.A],
                points[face.B],
                points[face.C],
                points[pointIndex]);
        }

        private static void AssignPointToFace(
            ReadOnlySpan<Exact3> points,
            List<HullFace> faces,
            int pointIndex)
        {
            HullFace? bestFace = null;
            Exact bestDistance = Exact.Zero;

            for (int i = 0; i < faces.Count; i++)
            {
                HullFace face = faces[i];
                if (face.Deleted)
                    continue;

                Exact distance = SignedDistance(points, face, pointIndex);
                if (distance.Sign() > 0)
                {
                    if (bestFace is null || distance > bestDistance)
                    {
                        bestFace = face;
                        bestDistance = distance;
                    }
                }
            }

            bestFace?.OutsidePoints.Add(pointIndex);
        }

        internal static bool TryFindInitialTetrahedron(
            ReadOnlySpan<Exact3> points,
            out int i0,
            out int i1,
            out int i2,
            out int i3)
        {
            i0 = i1 = i2 = i3 = -1;

            if (points.Length < 4)
                return false;

            i0 = 0;

            for (int i = 1; i < points.Length; i++)
            {
                if (!(points[i0].X == points[i].X && points[i0].Y == points[i].Y && points[i0].Z == points[i].Z))
                {
                    i1 = i;
                    break;
                }
            }

            if (i1 < 0)
                return false;

            Exact3 a = points[i0];
            Exact3 b = points[i1];
            Exact3 ab = b - a;

            for (int i = 0; i < points.Length; i++)
            {
                if (i == i0 || i == i1)
                    continue;

                Exact3 ac = points[i] - a;
                Exact3 cross = ExactGeometry3D.Cross(ab, ac);

                if (!(cross.X.IsZero() && cross.Y.IsZero() && cross.Z.IsZero()))
                {
                    i2 = i;
                    break;
                }
            }

            if (i2 < 0)
                return false;

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
            Exact quarter = Exact.FromDouble(0.25);

            return new Exact3(
                (a.X + b.X + c.X + d.X) * quarter,
                (a.Y + b.Y + c.Y + d.Y) * quarter,
                (a.Z + b.Z + c.Z + d.Z) * quarter);
        }

        private static void CompactFaces(
            List<HullFace> hullFaces,
            out Face[] faces,
            out int faceCount)
        {
            int count = 0;

            for (int i = 0; i < hullFaces.Count; i++)
            {
                if (!hullFaces[i].Deleted)
                    count++;
            }

            faces = new Face[count];
            faceCount = count;

            int write = 0;
            for (int i = 0; i < hullFaces.Count; i++)
            {
                if (!hullFaces[i].Deleted)
                {
                    faces[write++] = hullFaces[i].ToFace();
                }
            }
        }

        private struct HorizonEntry
        {
            public int EdgeA;
            public int EdgeB;
            public HullFace Survivor;
            public int SurvivorEdgeIndex;
        }
    }
}
