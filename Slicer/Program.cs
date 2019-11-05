using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DaeForth;
using IsoSlice;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MoreLinq;
using PrettyPrinter;

namespace Slicer {
	class Program {
		static void Main(string[] args) {
			var references = AppDomain.CurrentDomain.GetAssemblies()
				.Select(x => MetadataReference.CreateFromFile(x.Location)).ToList();
			var compiler = new Compiler();
			compiler.Compile(args[0].Split('/')[^1], File.ReadAllText(args[0]));
			var code = compiler.GenerateCode(new CSharpBackend());
			Console.Error.WriteLine(code);
			var cst = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(LanguageVersion.Latest));
			var optimize = true;
			var options = new CSharpCompilationOptions(
				OutputKind.DynamicallyLinkedLibrary,
				optimizationLevel: optimize ? OptimizationLevel.Release : OptimizationLevel.Debug,
				allowUnsafe: true);

			var lc = CSharpCompilation.Create("SlicerCompilation", options: options,
				references: references);
			var compilation = lc.AddSyntaxTrees(cst);
			using var ms = new MemoryStream();
			var er = compilation.Emit(ms);
			foreach(var diag in er.Diagnostics)
				Console.Error.WriteLine(diag);
			if(!er.Success) return;
			
			var asm = AppDomain.CurrentDomain.Load(ms.ToArray());
			var type = asm.GetType("DFShader");
			var mi = type.GetMethod("map_scene_float_Vec3");
			var tmap = (Func<Vec3, float>) mi.CreateDelegate(typeof(Func<Vec3, float>));
			var map = (Func<Vec3, float>) (p => tmap(p.xzy));

			var tmin = new Vec3(-2.5f);
			var tmax = new Vec3(2.5f);

			const int initialDepth = 7;

			Console.WriteLine("Building octree");
			var octree = Octree.Build(map, tmin, tmax, initialDepth);
			Console.WriteLine("Removing internal voids");
			octree = octree.RemoveInternalVoids();
			//Console.WriteLine("Refining surface");
			//octree = octree.RefineSurface(refinedDepth);
			//Console.WriteLine("Removing nonsurface nodes");
			//octree = octree.StripNonsurface();

			foreach(var (i, layer) in Flattener.Flatten(octree, (tmax.Z - tmin.Z) / MathF.Pow(2, initialDepth), new Vec2(-2.5f, 2.5f)).Enumerate()) {
				using var fp = File.OpenWrite($"layer{i:D4}.png");
				Png.Encode(DrawLayer(tmin.xy, tmax.xy, initialDepth + 2, layer), fp);
			}

			//StlOutput.Write(args[1], octree);
		}

		static Image DrawLayer(Vec2 min, Vec2 max, int depth, IReadOnlyList<ClosedPolygon> polygons) {
			var dim = (int) MathF.Pow(2, depth + 1);
			var data = new byte[dim * dim];
			var res = (max - min) / dim;
			
			polygons.ForEach(poly => {
				foreach(var points in new[] { poly.Points }.Concat(poly.Holes))
					foreach(var (a, b) in points.Zip(points.Skip(1).Concat(new[] { points[0] }))) {
						var ap = (a - min) / res;
						var bp = (b - min) / res;

						var diff = bp - ap;
						var steep = MathF.Abs(diff.Y) > MathF.Abs(diff.X);
						if(steep) (ap, bp) = (ap.yx, bp.yx);
						if(ap.X > bp.X) (ap, bp) = (bp, ap);

						diff = bp - ap;
						var gradient = diff.X == 0.0f ? 1f : diff.Y / diff.X;
						var intersectY = MathF.Round(ap.Y);
						
						int Clamp(int v, int min, int max) => Math.Max(min, Math.Min(max, v));
						void SetPixel(float x, float y, float v) =>
							data[Clamp((int) MathF.Round(y), 0, dim - 1) * dim + Clamp((int) MathF.Round(x), 0, dim - 1)] = (byte) MathF.Round(v * 255);

						if(steep)
							for(var x = ap.X; x <= bp.X; ++x) {
								var fp = intersectY - MathF.Floor(intersectY);
								SetPixel(intersectY, x, 1 - fp);
								SetPixel(intersectY - 1, x, fp);
								intersectY += gradient;
							}
						else
							for(var x = ap.X; x <= bp.X; ++x) {
								var fp = intersectY - MathF.Floor(intersectY);
								SetPixel(x, intersectY, 1 - fp);
								SetPixel(x, intersectY - 1, fp);
								intersectY += gradient;
							}
					}
			});
			
			return new Image(ColorMode.Greyscale, (dim, dim), data);
		}
	}
}