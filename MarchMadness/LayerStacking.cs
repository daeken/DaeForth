using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DaeForth;
using PrettyPrinter;

namespace MarchMadness {
	public class LayerStacking {
		public static List<Vec3> Build(Func<Vec3, float> map, Vec3 mins, Vec3 maxs, (int X, int Y, int Z) resolution) {
			var diff = maxs - mins;
			var step = diff / new Vec3(resolution.X, resolution.Y, resolution.Z);
			var halfStep = step / 2;
			var range = MathF.Max(halfStep.X, MathF.Max(halfStep.Y, halfStep.Z));
			
			void PaintLayer(bool[,] layer, int z) =>
				Parallel.ForEach(Enumerable.Range(0, resolution.X), x => {
					for(var y = 0; y < resolution.Y; ++y)
						layer[x + 1, y + 1] = map(mins + halfStep + step * new Vec3(x, y, z)) <= range;
				});

			var quads = new List<(Vec3 A, Vec3 B, Vec3 C, Vec3 D)>();

			var player = new bool[resolution.X + 2, resolution.Y + 2];
			for(var z = 1; z <= resolution.Z + 1; ++z) {
				Console.WriteLine(z);
				var clayer = new bool[resolution.X + 2, resolution.Y + 2];

				void AddQuad((int X, int Y, int Z) a, (int X, int Y, int Z) b, (int X, int Y, int Z) c,
					(int X, int Y, int Z) d, Vec3 en) {
					Vec3 GetPos((int X, int Y, int Z) v) => mins + halfStep + step * new Vec3(v.X, v.Y, v.Z);

					Vec3 GetNormal(Vec3 a, Vec3 b, Vec3 c) => (b - a).Cross(c - a).Normalized;

					var quad = (A: GetPos(a), B: GetPos(b), C: GetPos(c), D: GetPos(d));
					var na = GetNormal(quad.A, quad.B, quad.C);
					var nb = GetNormal(quad.A, quad.C, quad.D);
					if(na.Dot(nb) < 0.999f || na.Dot(en) < 0.999f)
						throw new Exception($"Normals {na.ToPrettyString()} {nb.ToPrettyString()} -- {en.ToPrettyString()}");
					quads.Add(quad);
				}
				
				if(z <= resolution.Z + 1) PaintLayer(clayer, z);
				
				for(var x = 1; x <= resolution.X; ++x)
					for(var y = 1; y <= resolution.Y; ++y) {
						var cur = clayer[x, y];
						var prev = player[x, y];
						if(!cur && !prev) // Both empty
							continue;
						if(!cur) // Current cell empty, previous cell full - add floor
							AddQuad(
								(x    , y    , z - 1), 
								(x + 1, y    , z - 1), 
								(x + 1, y + 1, z - 1), 
								(x    , y + 1, z - 1), 
								new Vec3(0, 0, 1)
							);
						if(!prev) // Current cell full, previous cell empty - add floor
							AddQuad(
								(x    , y + 1, z    ), 
								(x + 1, y + 1, z    ), 
								(x + 1, y    , z    ), 
								(x    , y    , z    ), 
								new Vec3(0, 0, -1)
							);
						if(!player[x - 1, y]) // Empty cell to the left
							AddQuad(
								(x    , y + 1, z    ), 
								(x    , y + 1, z - 1), 
								(x    , y    , z - 1), 
								(x    , y    , z    ), 
								new Vec3(-1, 0, 0)
							);
						if(!player[x + 1, y]) // Empty cell to the right
							AddQuad(
								(x + 1, y    , z    ), 
								(x + 1, y    , z - 1), 
								(x + 1, y + 1, z - 1), 
								(x + 1, y + 1, z    ), 
								new Vec3(1, 0, 0)
							);
						if(!player[x, y - 1]) // Empty cell to the back
							AddQuad(
								(x    , y    , z    ), 
								(x + 1, y    , z    ), 
								(x + 1, y    , z - 1), 
								(x    , y    , z - 1), 
								new Vec3(0, -1, 0)
							);
						if(!player[x, y + 1]) // Empty cell to the front
							AddQuad(
								(x    , y + 1, z - 1), 
								(x + 1, y + 1, z - 1), 
								(x + 1, y + 1, z    ), 
								(x    , y + 1, z    ), 
								new Vec3(0, 1, 0)
							);
					}

				player = clayer;
			}

			return quads.Select(x => new[] {
				x.A, x.B, x.C, 
				x.A, x.C, x.D
			}).SelectMany(x => x).ToList();
		}
	}
}