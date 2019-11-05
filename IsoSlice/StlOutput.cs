using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DaeForth;

namespace IsoSlice {
	public class StlOutput {
		public static void Write(string path, Octree octree) {
			Console.WriteLine("Building faces");
			var verts = BuildFaces(octree);
			verts = verts.Select(x => x * 100).ToList();
			Console.Error.WriteLine($"Writing {verts.Count} vertices");
			using var sw = new StreamWriter(path, false);
			sw.WriteLine("solid marched");
			for(var i = 0; i < verts.Count; i += 3) {
				var a = verts[i];
				var b = verts[i + 1];
				var c = verts[i + 2];
				sw.WriteLine("facet normal 0 0 0");
				sw.WriteLine("outer loop");
				sw.WriteLine($"vertex {a.X} {a.Y} {a.Z}");
				sw.WriteLine($"vertex {b.X} {b.Y} {b.Z}");
				sw.WriteLine($"vertex {c.X} {c.Y} {c.Z}");
				sw.WriteLine("endloop");
				sw.WriteLine("endfacet");
			}
			sw.WriteLine("endsolid marched");
		}
		
		static List<Vec3> BuildFaces(Octree octree) =>
			octree.RootPath.Leaves.Select(leaf => {
				if(leaf.Node == Octree.Node.Empty) return Enumerable.Empty<Vec3>().ToList();
				var tris = new List<Vec3>();

				void AddQuad(Vec3 a, Vec3 b, Vec3 c, Vec3 d) {
					tris.Add(a);
					tris.Add(b);
					tris.Add(c);

					tris.Add(a);
					tris.Add(c);
					tris.Add(d);
				}

				var a = leaf.Min;
				var b = new Vec3(leaf.Max.X, leaf.Min.Y, leaf.Min.Z);
				var c = new Vec3(leaf.Max.X, leaf.Max.Y, leaf.Min.Z);
				var d = new Vec3(leaf.Min.X, leaf.Max.Y, leaf.Min.Z);

				var e = new Vec3(leaf.Min.X, leaf.Min.Y, leaf.Max.Z);
				var f = new Vec3(leaf.Max.X, leaf.Min.Y, leaf.Max.Z);
				var g = leaf.Max;
				var h = new Vec3(leaf.Min.X, leaf.Max.Y, leaf.Max.Z);
				
				if(leaf.AllAdjacentBottom().Any(x => x.Node == Octree.Node.Empty))
					AddQuad(d, c, b, a); // Bottom
				if(leaf.AllAdjacentTop().Any(x => x.Node == Octree.Node.Empty))
					AddQuad(e, f, g, h); // Top

				if(leaf.AllAdjacentLeft().Any(x => x.Node == Octree.Node.Empty))
					AddQuad(a, e, h, d); // Left
				if(leaf.AllAdjacentRight().Any(x => x.Node == Octree.Node.Empty))
					AddQuad(c, g, f, b); // Right

				if(leaf.AllAdjacentBack().Any(x => x.Node == Octree.Node.Empty))
					AddQuad(a, e, f, b); // Back
				if(leaf.AllAdjacentFront().Any(x => x.Node == Octree.Node.Empty))
					AddQuad(c, g, h, d); // Front

				return tris;
			}).SelectMany(x => x).ToList();
	}
}