// ExactHull
// (c) Thorben Linneweber
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;

namespace ExactHull.ExactGeometry
{
    internal sealed class HullFace
    {
        public int A;
        public int B;
        public int C;

        public bool Deleted;
        public bool Visited;

        public HullFace? Neighbor0;
        public HullFace? Neighbor1;
        public HullFace? Neighbor2;

        public readonly List<int> OutsidePoints = new();

        public HullFace(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }

        public HullFace? GetNeighbor(int edgeIndex)
        {
            return edgeIndex switch
            {
                0 => Neighbor0,
                1 => Neighbor1,
                2 => Neighbor2,
                _ => null
            };
        }

        public void SetNeighbor(int edgeIndex, HullFace? neighbor)
        {
            switch (edgeIndex)
            {
                case 0: Neighbor0 = neighbor; break;
                case 1: Neighbor1 = neighbor; break;
                case 2: Neighbor2 = neighbor; break;
            }
        }

        public (int From, int To) GetEdgeVertices(int edgeIndex)
        {
            return edgeIndex switch
            {
                0 => (A, B),
                1 => (B, C),
                2 => (C, A),
                _ => throw new ArgumentOutOfRangeException(nameof(edgeIndex))
            };
        }

        public int FindEdgeIndex(int v1, int v2)
        {
            if (A == v1 && B == v2) return 0;
            if (B == v1 && C == v2) return 1;
            if (C == v1 && A == v2) return 2;
            return -1;
        }

        public int FindEdgeIndexUnordered(int v1, int v2)
        {
            if ((A == v1 && B == v2) || (A == v2 && B == v1)) return 0;
            if ((B == v1 && C == v2) || (B == v2 && C == v1)) return 1;
            if ((C == v1 && A == v2) || (C == v2 && A == v1)) return 2;
            return -1;
        }

        public Face ToFace()
        {
            return new Face(A, B, C);
        }
    }
}
