using System;
using System.Collections.Generic;
using System.Linq;
using DaeForth;
using PrettyPrinter;

namespace MarchMadness {
	public class OctreeLayerStacking {
		class Octree {
			public static readonly Octree Empty = new Octree();
			public static readonly Octree Full = new Octree();
			
			public Octree[] Children;
		}

		public static List<Vec3> Build(Func<Vec3, float> map, Vec3 mins, Vec3 maxs, int maxDepth) {
			var octants = new[] {
				new Vec3(1, 1, 1), 
				new Vec3(1, 1, 0), 
				new Vec3(1, 0, 1), 
				new Vec3(1, 0, 0), 
					
				new Vec3(0, 1, 1), 
				new Vec3(0, 1, 0), 
				new Vec3(0, 0, 1), 
				new Vec3(0, 0, 0)
			};

			var singleStep = (maxs - mins) / MathF.Pow(2, maxDepth + 1);
			
			Octree BuildOctree(Vec3 min, Vec3 max, int depth) {
				var halfSize = (max - min) / 2;
				var center = min + halfSize;
				var cval = map(center);
				if(cval > 0.001f && cval > MathF.Max(halfSize.X, MathF.Max(halfSize.Y, halfSize.Z)) * 2)
					return Octree.Empty;
				if(depth == maxDepth) return Octree.Full;
				
				Octree SubBuild(Vec3 x) {
					var corner = center - halfSize * x;
					return BuildOctree(corner, corner + halfSize, depth + 1);
				}
				var children = depth < 2
					? octants.AsParallel().AsOrdered().Select(SubBuild).ToArray()
					: octants.Select(SubBuild).ToArray();

				if(children.All(x => x == Octree.Empty)) return Octree.Empty;
				if(children.All(x => x == Octree.Full)) return Octree.Full;
				return new Octree { Children = children };
			}
			
			Console.WriteLine("Building octree...");
			var octree = BuildOctree(mins, maxs, 0);

			var emptyList = new List<Vec3>();
			List<Vec3> BuildFaces(Vec3 min, Vec3 max, Octree node, int depth) {
				if(node == Octree.Empty) return emptyList;
				var halfSize = (max - min) / 2;
				var center = min + halfSize;
				if(node != Octree.Full) {
					List<Vec3> SubBuild(Vec3 x, int i) {
						var corner = center - halfSize * x;
						return BuildFaces(corner, corner + halfSize, node.Children[i], depth + 1);
					}

					return depth < 2
						? octants.AsParallel().Select(SubBuild).AsSequential().SelectMany(x => x).ToList()
						: octants.Select(SubBuild).SelectMany(x => x).ToList();
				}

				var tris = new List<Vec3>();
				void AddQuad(Vec3 a, Vec3 b, Vec3 c, Vec3 d, Vec3 en) {
					Vec3 GetNormal(Vec3 a, Vec3 b, Vec3 c) => (b - a).Cross(c - a).Normalized;

					var quad = (A: a, B: b, C: c, D: d);
					var na = GetNormal(quad.A, quad.B, quad.C);
					var nb = GetNormal(quad.A, quad.C, quad.D);
					if(na.Dot(nb) < 0.999f || na.Dot(en) < 0.999f)
						throw new Exception($"Normals {na.ToPrettyString()} {nb.ToPrettyString()} -- {en.ToPrettyString()}");
					
					tris.Add(quad.A);
					tris.Add(quad.B);
					tris.Add(quad.C);
					
					tris.Add(quad.A);
					tris.Add(quad.C);
					tris.Add(quad.D);
				}

				bool AdjacentEmpty(Vec3 dir) {
					var pos = center + (halfSize + singleStep) * dir;
					bool IsFull(Vec3 min, Vec3 max, Octree node) {
						if(min.X > pos.X || max.X < pos.X || 
						   min.Y > pos.Y || max.Y < pos.Y || 
						   min.Z > pos.Z || max.Z < pos.Z)
							return false;
						if(node == Octree.Empty) return false;
						if(node == Octree.Full) return true;
						var halfSize = (max - min) / 2;
						var center = min + halfSize;
						return octants.Select((x, i) => (x, i)).Any(x => {
							var corner = center - halfSize * x.x;
							return IsFull(corner, corner + halfSize, node.Children[x.i]);
						});
					}
					return !IsFull(mins, maxs, octree);
				}
				
				var a = min;
				var b = new Vec3(max.X, min.Y, min.Z);
				var c = new Vec3(max.X, max.Y, min.Z);
				var d = new Vec3(min.X, max.Y, min.Z);

				var e = new Vec3(min.X, min.Y, max.Z);
				var f = new Vec3(max.X, min.Y, max.Z);
				var g = max;
				var h = new Vec3(min.X, max.Y, max.Z);
				
				if(AdjacentEmpty(new Vec3(0, 0, -1)))
					AddQuad(d, c, b, a, new Vec3(0, 0, -1)); // Bottom
				if(AdjacentEmpty(new Vec3(0, 0, 1)))
					AddQuad(e, f, g, h, new Vec3(0, 0,  1)); // Top
				
				if(AdjacentEmpty(new Vec3(-1, 0, 0)))
					AddQuad(a, e, h, d, new Vec3(-1, 0, 0)); // Left
				if(AdjacentEmpty(new Vec3(1, 0, 0)))
					AddQuad(c, g, f, b, new Vec3( 1, 0, 1)); // Right

				if(AdjacentEmpty(new Vec3(0, -1, 0)))
					AddQuad(a, e, f, b, new Vec3(0, -1, 0)); // Back
				if(AdjacentEmpty(new Vec3(0, 1, 0)))
					AddQuad(c, g, h, d, new Vec3(0,  1, 0)); // Front

				return tris;
			}
			
			Console.WriteLine("Building faces");
			return BuildFaces(mins, maxs, octree, 0);
		}
	}
}