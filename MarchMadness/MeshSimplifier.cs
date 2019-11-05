using System;
using System.Collections.Generic;
using System.Linq;
using DaeForth;
using MeshDecimator;
using MeshDecimator.Math;
using MoreLinq;
using PrettyPrinter;

namespace MarchMadness {
	public class MeshSimplifier {
		public static List<Vec3> Simplify(List<Vec3> incoming, int? target = null, Func<Vec3, float> map = null) {
			var ms = new MeshSimplifier(incoming, target);
			if(map != null)
				ms.Tighten(map);
			return ms.Emit();
		}

		class Triangle {
			internal int A, B, C;
			
			internal Triangle(int a, int b, int c) {
				A = a;
				B = b;
				C = c;
			}

			internal List<int> Triangulate() => new List<int> { A, B, C };
		}

		List<Vec3> Vertices = new List<Vec3>();
		List<Triangle> Triangles = new List<Triangle>();

		MeshSimplifier(List<Vec3> verts, int? target) {
			Console.WriteLine("Converting to indexed form");
			ConvertToIndexed(verts);

			var nmesh = new Mesh(Vertices.Select(x => new Vector3d(x.X, x.Y, x.Z)).ToArray(),
				Triangles.Select(x => x.Triangulate()).SelectMany(x => x).ToArray());
			nmesh.RecalculateNormals();
			nmesh.RecalculateTangents();
			Console.WriteLine("Decimating mesh");
			nmesh = target == null
				? MeshDecimation.DecimateMeshLossless(nmesh)
				: MeshDecimation.DecimateMesh(nmesh, target.Value);
			Vertices = nmesh.Vertices.Select(x => new Vec3((float) x.x, (float) x.y, (float) x.z)).ToList();
			var inds = nmesh.Indices;
			Triangles = new List<Triangle>();
			for(var i = 0; i < inds.Length; i += 3)
				Triangles.Add(new Triangle(inds[i], inds[i + 1], inds[i + 2]));
		}

		void Tighten(Func<Vec3, float> map) {
			Vec3 Normal(Vec3 a, Vec3 b, Vec3 c) => (b - a).Cross(c - a).Normalized;
			Vec3 Centroid(Vec3 a, Vec3 b, Vec3 c) => (a + b + c) / 3;
			Vec3 NormalAt(Vec3 pos) {
				var value = map(pos);
				return (new Vec3(
					        map(pos + new Vec3(0.0001f, 0, 0)),
					        map(pos + new Vec3(0, 0.0001f, 0)),
					        map(pos + new Vec3(0, 0, 0.0001f))
				        ) - value).Normalized;
			}
			
			Console.WriteLine("Tightening mesh to isosurface");
			var avgError = float.PositiveInfinity;
			for(var i = 0; i < 64; ++i) {
				Console.WriteLine($"Tightening iteration {++i} -- {avgError}");
				var errors = Triangles.Select(tri => (tri,
						new[] {
							Centroid(Vertices[tri.A], Vertices[tri.B], Vertices[tri.C]), Vertices[tri.A],
							Vertices[tri.B], Vertices[tri.C]
						}.Select(y => MathF.Abs(map(y))).Sum()))
					.OrderByDescending(x => x.Item2);
				var changed = false;
				var running = 0f;
				foreach(var (tri, berror) in errors) {
					running += berror;
					var ta = Vertices[tri.A];
					var tb = Vertices[tri.B];
					var tc = Vertices[tri.C];
					var centroid = Centroid(ta, tb, tc);
					var targetCentroid = centroid - NormalAt(centroid) * map(centroid);
					var cerror = (targetCentroid - centroid).Length;
					var targetNormal = NormalAt(targetCentroid);
					var faceNormal = Normal(ta, tb, tc);
					var nerror = 1 - faceNormal.Dot(targetNormal);
					if(cerror < 0.0001f && nerror < 0.001f) continue;

					var targetA = ta - NormalAt(ta) * map(ta);
					var targetB = tb - NormalAt(tb) * map(tb);
					var targetC = tc - NormalAt(tc) * map(tc);
					var moveA = (ta - targetA).Length;
					var moveB = (tb - targetB).Length;
					var moveC = (tc - targetC).Length;

					if(moveA < 0.0001f && moveB < 0.0001f && moveC < 0.0001f) continue;

					var nifA = Normal(targetA, tb, tc);
					var nifB = Normal(ta, targetB, tc);
					var nifC = Normal(ta, tb, targetC);

					var cifA = Centroid(targetA, tb, tc);
					var cifB = Centroid(ta, targetB, tc);
					var cifC = Centroid(ta, tb, targetC);

					var nrerrorA = nerror / (1 - nifA.Dot(targetNormal));
					var nrerrorB = nerror / (1 - nifB.Dot(targetNormal));
					var nrerrorC = nerror / (1 - nifC.Dot(targetNormal));

					if(nrerrorA >= 1 && nrerrorB >= 1 && nrerrorC >= 1) continue;

					var crerrorA = cerror / (targetCentroid - cifA).Length;
					var crerrorB = cerror / (targetCentroid - cifB).Length;
					var crerrorC = cerror / (targetCentroid - cifC).Length;

					changed = true;
					if(nrerrorA < nrerrorB && nrerrorA < nrerrorC && crerrorA < crerrorB && crerrorA < crerrorC)
						Vertices[tri.A] = targetA;
					else if(nrerrorB < nrerrorA && nrerrorB < nrerrorC && crerrorB < crerrorA && crerrorB < crerrorC)
						Vertices[tri.B] = targetB;
					else if(nrerrorC < nrerrorA && nrerrorC < nrerrorB && crerrorC < crerrorA && crerrorC < crerrorB)
						Vertices[tri.C] = targetC;
					else if(nrerrorA < nrerrorB && nrerrorA < nrerrorC)
						Vertices[tri.A] = targetA;
					else if(nrerrorB < nrerrorA && nrerrorB < nrerrorC)
						Vertices[tri.B] = targetB;
					else if(nrerrorC < nrerrorA && nrerrorC < nrerrorB)
						Vertices[tri.C] = targetC;
					else
						changed = false;
				}

				var cavg = running / Triangles.Count;
				if(cavg > avgError) break;
				avgError = cavg;
				if(!changed) break;
			}
		}

		void ConvertToIndexed(List<Vec3> iv) {
			var indexMap = new Dictionary<(float, float, float), int>();
			var numVerts = 0;

			int ToIndex(Vec3 point) {
				var key = (MathF.Round(point.X, 5), MathF.Round(point.Y, 5), MathF.Round(point.Z, 5));
				if(indexMap.TryGetValue(key, out var index)) return index;
				index = indexMap[key] = numVerts++;
				Vertices.Add(point);
				return index;
			}

			for(var i = 0; i < iv.Count; i += 3) {
				var a = ToIndex(iv[i]);
				var b = ToIndex(iv[i + 1]);
				var c = ToIndex(iv[i + 2]);
				//var normal = (iv[i + 1] - iv[i + 0]).Cross(iv[i + 2] - iv[i + 0]).Normalized;
				var poly = new Triangle(a, b, c);
				Triangles.Add(poly);
			}
		}

		List<Vec3> Emit() => Triangles.AsParallel().Select(x => x.Triangulate()).AsSequential().SelectMany(x => x)
			.Select(x => Vertices[x]).ToList();
	}
}