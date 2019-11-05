/*

Implementations of Octree member functions.

Copyright (C) 2011  Tao Ju

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public License
(LGPL) as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DaeForth;

namespace MarchMadness.DC {
    public class Octree {
        static int MATERIAL_AIR = 0;
        static int MATERIAL_SOLID = 1;

        static double QEF_ERROR = 1e-6f;
        static int QEF_SWEEPS = 4;

        #region Readonly Variables

        static readonly Vec3[] CHILD_MIN_OFFSETS = {
            // needs to match the vertMap from Dual Contouring impl
            new Vec3(0, 0, 0),
            new Vec3(0, 0, 1),
            new Vec3(0, 1, 0),
            new Vec3(0, 1, 1),
            new Vec3(1, 0, 0),
            new Vec3(1, 0, 1),
            new Vec3(1, 1, 0),
            new Vec3(1, 1, 1)
        };

        // data from the original DC impl, drives the contouring process

        static readonly int[][] edgevmap = {
            new[] { 2, 4 }, new[] { 1, 5 }, new[] { 2, 6 }, new[] { 3, 7 }, // x-axis 
            new[] { 0, 2 }, new[] { 1, 3 }, new[] { 4, 6 }, new[] { 5, 7 }, // y-axis
            new[] { 0, 1 }, new[] { 2, 3 }, new[] { 4, 5 }, new[] { 6, 7 } // z-axis
        };

        public static readonly int[] edgemask = { 5, 3, 6 };

        public static readonly int[][] vertMap = {
            new[] { 0, 0, 0 },
            new[] { 0, 0, 1 },
            new[] { 0, 1, 0 },
            new[] { 0, 1, 1 },
            new[] { 1, 0, 0 },
            new[] { 1, 0, 1 },
            new[] { 1, 1, 0 },
            new[] { 1, 1, 1 }
        };

        public static readonly int[][] faceMap = {
            new[] { 4, 8, 5, 9 },
            new[] { 6, 10, 7, 11 },
            new[] { 0, 8, 1, 10 },
            new[] { 2, 9, 3, 11 },
            new[] { 0, 4, 2, 6 },
            new[] { 1, 5, 3, 7 }
        };

        static readonly int[][] cellProcFaceMask = {
            new[] { 0, 4, 0 },
            new[] { 1, 5, 0 },
            new[] { 2, 6, 0 },
            new[] { 3, 7, 0 },
            new[] { 0, 2, 1 },
            new[] { 4, 6, 1 },
            new[] { 1, 3, 1 },
            new[] { 5, 7, 1 },
            new[] { 0, 1, 2 },
            new[] { 2, 3, 2 },
            new[] { 4, 5, 2 },
            new[] { 6, 7, 2 }
        };

        static readonly int[][] cellProcEdgeMask = {
            new[] { 0, 1, 2, 3, 0 },
            new[] { 4, 5, 6, 7, 0 },
            new[] { 0, 4, 1, 5, 1 },
            new[] { 2, 6, 3, 7, 1 },
            new[] { 0, 2, 4, 6, 2 },
            new[] { 1, 3, 5, 7, 2 }
        };

        static readonly int[][][] faceProcFaceMask = {
            new[] { new[] { 4, 0, 0 }, new[] { 5, 1, 0 }, new[] { 6, 2, 0 }, new[] { 7, 3, 0 } },
            new[] { new[] { 2, 0, 1 }, new[] { 6, 4, 1 }, new[] { 3, 1, 1 }, new[] { 7, 5, 1 } },
            new[] { new[] { 1, 0, 2 }, new[] { 3, 2, 2 }, new[] { 5, 4, 2 }, new[] { 7, 6, 2 } }
        };

        static readonly int[][][] faceProcEdgeMask = new int[3][][] {
            new[] {
                new[] { 1, 4, 0, 5, 1, 1 }, new[] { 1, 6, 2, 7, 3, 1 }, new[] { 0, 4, 6, 0, 2, 2 },
                new[] { 0, 5, 7, 1, 3, 2 }
            },
            new[] {
                new[] { 0, 2, 3, 0, 1, 0 }, new[] { 0, 6, 7, 4, 5, 0 }, new[] { 1, 2, 0, 6, 4, 2 },
                new[] { 1, 3, 1, 7, 5, 2 }
            },
            new[] {
                new[] { 1, 1, 0, 3, 2, 0 }, new[] { 1, 5, 4, 7, 6, 0 }, new[] { 0, 1, 5, 0, 4, 1 },
                new[] { 0, 3, 7, 2, 6, 1 }
            }
        };

        static readonly int[][][] edgeProcEdgeMask = {
            new[] { new[] { 3, 2, 1, 0, 0 }, new[] { 7, 6, 5, 4, 0 } },
            new[] { new[] { 5, 1, 4, 0, 1 }, new[] { 7, 3, 6, 2, 1 } },
            new[] { new[] { 6, 4, 2, 0, 2 }, new[] { 7, 5, 3, 1, 2 } }
        };

        static readonly int[][] processEdgeMask = {
            new[] { 3, 2, 1, 0 }, new[] { 7, 5, 6, 4 }, new[] { 11, 10, 9, 8 }
        };

        #endregion


        static OctreeNode SimplifyOctree(OctreeNode node, double threshold) {
            if(node == null) {
                return null;
            }

            if(node.Type != OctreeNodeType.Node_Internal) {
                // can't simplify!
                return node;
            }

            var qef = new QefSolver();
            var signs = new int[8] { -1, -1, -1, -1, -1, -1, -1, -1 };
            var midsign = -1;
            var edgeCount = 0;
            var isCollapsible = true;

            for(var i = 0; i < 8; i++) {
                node.children[i] = SimplifyOctree(node.children[i], threshold);

                if(node.children[i] != null) {
                    var child = node.children[i];

                    if(child.Type == OctreeNodeType.Node_Internal) {
                        isCollapsible = false;
                    } else {
                        qef.add(child.drawInfo.qef);

                        midsign = (child.drawInfo.corners >> (7 - i)) & 1;
                        signs[i] = (child.drawInfo.corners >> i) & 1;

                        edgeCount++;
                    }
                }
            }

            if(!isCollapsible) {
                // at least one child is an internal node, can't collapse
                return node;
            }

            Vec3 qefPosition;
            qef.solve(out qefPosition, QEF_ERROR, QEF_SWEEPS, QEF_ERROR);
            var error = qef.getError();
            
            // convert to glm vec3 for ease of use
            var position = new Vec3(qefPosition.X, qefPosition.Y, qefPosition.Z);

            // at this point the masspoint will actually be a sum, so divide to make it the average
            if(error > threshold) {
                // this collapse breaches the threshold
                return node;
            }

            if(position.X < node.min.X || position.X > (node.min.X + node.size) ||
               position.Y < node.min.Y || position.Y > (node.min.Y + node.size) ||
               position.Z < node.min.Z || position.Z > (node.min.Z + node.size)) {
                position = qef.getMassPoint();
            }

            // change the node from an internal node to a 'psuedo leaf' node
            var drawInfo = new OctreeDrawInfo();
            drawInfo.corners = 0;
            drawInfo.index = -1;

            for(var i = 0; i < 8; i++) {
                if(signs[i] == -1) {
                    // Undetermined, use centre sign instead
                    drawInfo.corners |= (midsign << i);
                } else {
                    drawInfo.corners |= (signs[i] << i);
                }
            }

            drawInfo.averageNormal = Vec3.Zero;
            for(var i = 0; i < 8; i++) {
                if(node.children[i] != null) {
                    var child = node.children[i];
                    if(child.Type == OctreeNodeType.Node_Psuedo ||
                       child.Type == OctreeNodeType.Node_Leaf) {
                        drawInfo.averageNormal += child.drawInfo.averageNormal;
                    }
                }
            }

            drawInfo.averageNormal = drawInfo.averageNormal.Normalized;
            drawInfo.position = position;
            drawInfo.qef = qef.getData();

            for(var i = 0; i < 8; i++) {
                DestroyOctree(node.children[i]);
                node.children[i] = null;
            }

            node.Type = OctreeNodeType.Node_Psuedo;
            node.drawInfo = drawInfo;

            return node;
        }

        public static void GenerateVertexIndices(OctreeNode node, List<(Vec3, Vec3)> vertexBuffer) {
            if(node == null) {
                return;
            }

            if(node.Type != OctreeNodeType.Node_Leaf) {
                for(var i = 0; i < 8; i++) {
                    GenerateVertexIndices(node.children[i], vertexBuffer);
                }
            }

            if(node.Type != OctreeNodeType.Node_Internal) {
                node.drawInfo.index = vertexBuffer.Count;

                vertexBuffer.Add((node.drawInfo.position, node.drawInfo.averageNormal));
            }
        }

        public static void ContourProcessEdge(OctreeNode[] node, int dir, List<int> indexBuffer) {
            var minSize = 1000000; // arbitrary big number
            var minIndex = 0;
            var indices = new int[4] { -1, -1, -1, -1 };
            var flip = false;
            var signChange = new bool[4] { false, false, false, false };

            for(var i = 0; i < 4; i++) {
                var edge = processEdgeMask[dir][i];
                var c1 = edgevmap[edge][0];
                var c2 = edgevmap[edge][1];

                var m1 = (node[i].drawInfo.corners >> c1) & 1;
                var m2 = (node[i].drawInfo.corners >> c2) & 1;

                if(node[i].size < minSize) {
                    minSize = node[i].size;
                    minIndex = i;
                    flip = m1 != MATERIAL_AIR;
                }

                indices[i] = node[i].drawInfo.index;

                signChange[i] = m1 != m2;
            }

            if(signChange[minIndex]) {
                if(flip) {
                    indexBuffer.Add(indices[1]);
                    indexBuffer.Add(indices[0]);
                    indexBuffer.Add(indices[2]);
                    indexBuffer.Add(indices[3]);
                } else {
                    indexBuffer.Add(indices[0]);
                    indexBuffer.Add(indices[1]);
                    indexBuffer.Add(indices[3]);
                    indexBuffer.Add(indices[2]);
                }
            }
        }

        public static void ContourEdgeProc(OctreeNode[] node, int dir, List<int> indexBuffer) {
            if(node[0] == null || node[1] == null || node[2] == null || node[3] == null) {
                return;
            }

            if(node[0].Type != OctreeNodeType.Node_Internal &&
               node[1].Type != OctreeNodeType.Node_Internal &&
               node[2].Type != OctreeNodeType.Node_Internal &&
               node[3].Type != OctreeNodeType.Node_Internal) {
                ContourProcessEdge(node, dir, indexBuffer);
            } else {
                for(var i = 0; i < 2; i++) {
                    var edgeNodes = new OctreeNode[4];
                    var c = new int[4] {
                        edgeProcEdgeMask[dir][i][0],
                        edgeProcEdgeMask[dir][i][1],
                        edgeProcEdgeMask[dir][i][2],
                        edgeProcEdgeMask[dir][i][3]
                    };

                    for(var j = 0; j < 4; j++) {
                        if(node[j].Type == OctreeNodeType.Node_Leaf || node[j].Type == OctreeNodeType.Node_Psuedo) {
                            edgeNodes[j] = node[j];
                        } else {
                            edgeNodes[j] = node[j].children[c[j]];
                        }
                    }

                    ContourEdgeProc(edgeNodes, edgeProcEdgeMask[dir][i][4], indexBuffer);
                }
            }
        }

        public static void ContourFaceProc(OctreeNode[] node, int dir, List<int> indexBuffer) {
            if(node[0] == null || node[1] == null) {
                return;
            }

            if(node[0].Type == OctreeNodeType.Node_Internal ||
               node[1].Type == OctreeNodeType.Node_Internal) {
                for(var i = 0; i < 4; i++) {
                    var faceNodes = new OctreeNode[2];
                    var c = new int[2] {
                        faceProcFaceMask[dir][i][0],
                        faceProcFaceMask[dir][i][1]
                    };

                    for(var j = 0; j < 2; j++) {
                        if(node[j].Type != OctreeNodeType.Node_Internal) {
                            faceNodes[j] = node[j];
                        } else {
                            faceNodes[j] = node[j].children[c[j]];
                        }
                    }

                    ContourFaceProc(faceNodes, faceProcFaceMask[dir][i][2], indexBuffer);
                }

                var orders = new int[2][] {
                    new int[4] { 0, 0, 1, 1 },
                    new int[4] { 0, 1, 0, 1 }
                };

                for(var i = 0; i < 4; i++) {
                    var edgeNodes = new OctreeNode[4];
                    var c = new int[4] {
                        faceProcEdgeMask[dir][i][1],
                        faceProcEdgeMask[dir][i][2],
                        faceProcEdgeMask[dir][i][3],
                        faceProcEdgeMask[dir][i][4]
                    };

                    var order = orders[faceProcEdgeMask[dir][i][0]];
                    for(var j = 0; j < 4; j++) {
                        if(node[order[j]].Type == OctreeNodeType.Node_Leaf ||
                           node[order[j]].Type == OctreeNodeType.Node_Psuedo) {
                            edgeNodes[j] = node[order[j]];
                        } else {
                            edgeNodes[j] = node[order[j]].children[c[j]];
                        }
                    }

                    ContourEdgeProc(edgeNodes, faceProcEdgeMask[dir][i][5], indexBuffer);
                }
            }
        }

        public static void ContourCellProc(OctreeNode node, List<int> indexBuffer) {
            if(node?.Type != OctreeNodeType.Node_Internal) return;
            for(var i = 0; i < 8; i++) {
                ContourCellProc(node.children[i], indexBuffer);
            }

            for(var i = 0; i < 12; i++) {
                var faceNodes = new OctreeNode[2];
                int[] c = { cellProcFaceMask[i][0], cellProcFaceMask[i][1] };

                faceNodes[0] = node.children[c[0]];
                faceNodes[1] = node.children[c[1]];

                ContourFaceProc(faceNodes, cellProcFaceMask[i][2], indexBuffer);
            }

            for(var i = 0; i < 6; i++) {
                var edgeNodes = new OctreeNode[4];
                var c = new int[4] {
                    cellProcEdgeMask[i][0],
                    cellProcEdgeMask[i][1],
                    cellProcEdgeMask[i][2],
                    cellProcEdgeMask[i][3]
                };

                for(var j = 0; j < 4; j++) {
                    edgeNodes[j] = node.children[c[j]];
                }

                ContourEdgeProc(edgeNodes, cellProcEdgeMask[i][4], indexBuffer);
            }
        }

        public static Vec3 ApproximateZeroCrossingPosition(Vec3 p0, Vec3 p1) {
            /*// approximate the zero crossing by finding the min value along the edge
            double minValue = 100000f;
            double t = 0f;
            double currentT = 0f;
            const int steps = 100;
            const double increment = 1f / (double) steps;
            while(currentT <= 1.0f) {
                var p = Globals.mix(p0, p1, currentT);
                var density = Math.Abs(Program.Scene(p).Distance);
                if(density < minValue) {
                    minValue = density;
                    t = currentT;
                }

                currentT += increment;
            }

            return Globals.mix(p0, p1, t);*/

            int Sign(double v) => v >= 0 ? 1 : -1;
            Vec3 BinarySearch(Vec3 a, Vec3 b, int level = 15) {
                var mp = (a + b) / 2;
                if(level == 0)
                    return mp;
                var asign = Sign(_Program.Scene(a));
                var bsign = Sign(_Program.Scene(b));
                if(asign == bsign)
                    return mp;
                var mpsign = Sign(_Program.Scene(mp));
                return asign == mpsign ? BinarySearch(mp, b, level - 1) : BinarySearch(a, mp, level - 1);
            }

            return BinarySearch(p0, p1);
        }

        public static Vec3 CalculateSurfaceNormal(Vec3 p) => _Program.SceneNormal(p);

        public static OctreeNode ConstructLeaf(OctreeNode leaf) {
            var leafSize = leaf.max - leaf.min;
            if(leaf == null || leaf.size != 1) {
                return null;
            }

            var corners = 0;
            for(var i = 0; i < 8; i++) {
                var cornerPos = leaf.min + CHILD_MIN_OFFSETS[i] * leafSize;
                var density = _Program.Scene(cornerPos);
                var material = density < 0.0f ? MATERIAL_SOLID : MATERIAL_AIR;
                corners |= (material << i);
            }

            if(corners == 0 || corners == 255) {
                // voxel is full inside or outside the volume
                //delete leaf
                //setting as null isn't required by the GC in C#... but its in the original, so why not!
                leaf = null;
                return null;
            }

            // otherwise the voxel contains the surface, so find the edge intersections
            const int MAX_CROSSINGS = 6;
            var edgeCount = 0;
            var averageNormal = Vec3.Zero;
            var qef = new QefSolver();

            for(var i = 0; i < 12 && edgeCount < MAX_CROSSINGS; i++) {
                var c1 = edgevmap[i][0];
                var c2 = edgevmap[i][1];

                var m1 = (corners >> c1) & 1;
                var m2 = (corners >> c2) & 1;

                if((m1 == MATERIAL_AIR && m2 == MATERIAL_AIR) || (m1 == MATERIAL_SOLID && m2 == MATERIAL_SOLID)) {
                    // no zero crossing on this edge
                    continue;
                }

                var p1 = leaf.min + CHILD_MIN_OFFSETS[c1] * leafSize;
                var p2 = leaf.min + CHILD_MIN_OFFSETS[c2] * leafSize;
                var p = ApproximateZeroCrossingPosition(p1, p2);
                var n = CalculateSurfaceNormal(p);
                qef.add(p.X, p.Y, p.Z, n.X, n.Y, n.Z);

                averageNormal += n;

                edgeCount++;
            }

            var drawInfo = new OctreeDrawInfo {
                averageNormal = (averageNormal / edgeCount).Normalized, 
                corners = corners, 
                index = -1
            };
            qef.solve(out drawInfo.position, QEF_ERROR, QEF_SWEEPS, QEF_ERROR);
            drawInfo.qef = qef.getData();

            var min = leaf.min;
            var max = min + leaf.size;
            if(drawInfo.position.X < min.X || drawInfo.position.X > max.X ||
               drawInfo.position.Y < min.Y || drawInfo.position.Y > max.Y ||
               drawInfo.position.Z < min.Z || drawInfo.position.Z > max.Z) {
                drawInfo.position = qef.getMassPoint();
            }

            leaf.Type = OctreeNodeType.Node_Leaf;
            leaf.drawInfo = drawInfo;

            return leaf;
        }

        public static OctreeNode ConstructOctreeNodes(OctreeNode node, bool first = false) {
            var center = (node.max - node.min) / 2 + node.min;
            var dc = _Program.Scene(center);
            if(dc <= -node.radius || dc > node.radius) // Entirely inside or entirely outside
                return null;

            if(node.size == 1)
                return ConstructLeaf(node);

            var childSize = node.size / 2;
            var cellSize = (node.max - node.min) / 2;

            void GenChild(int i) {
                var child = new OctreeNode(OctreeNodeType.Node_Internal) {
                    size = childSize,
                    min = node.min + CHILD_MIN_OFFSETS[i] * cellSize,
                    radius = cellSize.Length / 2
                };
                child.max = child.min + cellSize;

                node.children[i] = ConstructOctreeNodes(child);
            }

            if(first)
                Parallel.For(0, 8, GenChild);
            else
                for(var i = 0; i < 8; ++i)
                    GenChild(i);
            
            return node.children.FirstOrDefault(x => x != null) != null ? node : null;
        }

        public static OctreeNode BuildOctree(Vec3 min, Vec3 max, int size, double threshold) {
            var root = new OctreeNode(OctreeNodeType.Node_Internal) {
                min = min,
                max = max,
                radius = (max - min).Length / 2,
                size = size
            };

            ConstructOctreeNodes(root, first: true);
            root = SimplifyOctree(root, threshold);

            return root;
        }

        public static void GenerateMeshFromOctree(OctreeNode node, List<(Vec3, Vec3)> vertexBuffer,
            List<int> indexBuffer) {
            
            if(node == null)
                return;

            GenerateVertexIndices(node, vertexBuffer);
            ContourCellProc(node, indexBuffer);

            /*GameObject go = new GameObject("Mesh");
            Mesh mesh;
            MeshFilter filter;
            MeshRenderer meshRenderer;

            meshRenderer = go.AddComponent<MeshRenderer>();
            filter = go.AddComponent<MeshFilter>();

            meshRenderer.sharedMaterial = Resources.Load("Default") as Material;

            Vec3[] vertArray = new Vec3[vertexBuffer.Count];
            Vector2[] uvs = new Vector2[vertexBuffer.Count];
            for(int i = 0; i < vertexBuffer.Count; i++) {
                vertArray[i] = vertexBuffer[i].Xyz;
                uvs[i] = new Vector2(vertexBuffer[i].Xyz.X, vertexBuffer[i].Xyz.Z);
            }

            Vec3[] normsArray = new Vec3[vertexBuffer.Count];
            for(int i = 0; i < vertexBuffer.Count; i++) {
                normsArray[i] = vertexBuffer[i].normal;
            }

            mesh = filter.mesh;
            mesh.vertices = vertArray;
            mesh.uv = uvs;
            mesh.triangles = indexBuffer.ToArray();
            mesh.normals = normsArray;
            mesh.RecalculateBounds();

            for(int i = 0; i < 8; i++) {
                Debug.Log("vert: " + vertArray[i]);
            }

            for(int i = 0; i < 8; i++) {
                Debug.Log("index: " + indexBuffer[i]);
            }*/

        }

        public static void DestroyOctree(OctreeNode node) {
            if(node == null) {
                return;
            }

            for(var i = 0; i < 8; i++) {
                DestroyOctree(node.children[i]);
            }

            node = null;
        }
    }
}