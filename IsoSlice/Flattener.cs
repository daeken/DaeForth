using System;
using System.Collections.Generic;
using System.Linq;
using DaeForth;
using MoreLinq;
using PrettyPrinter;

namespace IsoSlice {
	public class ClosedPolygon {
		public readonly IReadOnlyList<Vec2> Points;
		public readonly IReadOnlyList<IReadOnlyList<Vec2>> Holes;
		public ClosedPolygon(IEnumerable<Vec2> points, IEnumerable<IEnumerable<Vec2>> holes) {
			Points = points.ToList();
			Holes = holes.Select(x => x.ToList()).ToList();
		}
	}

	public class Flattener {
		static IEnumerable<QuadTree> FindAllIslands(QuadTree quadTree) {
			while(quadTree.Root != QuadTree.Node.Empty) {
				var beenQueued = new HashSet<QuadTree.Path>();
				var first = quadTree.RootPath.FullLeaves.First();
				var processing = new Queue<QuadTree.Path>(new[] { first });
				var island = new HashSet<QuadTree.Path>();
				while(processing.TryDequeue(out var path)) {
					if(path.Node != QuadTree.Node.Full) continue;
					island.Add(path);
					foreach(var x in path.AllAdjacent().Where(x => !beenQueued.Contains(x))) {
						beenQueued.Add(x);
						processing.Enqueue(x);
					}
				}

				yield return quadTree.TransformLeaves(path =>
					path.Node == QuadTree.Node.Full && island.Contains(path)
						? QuadTree.Node.Full
						: QuadTree.Node.Empty);
				quadTree = quadTree.TransformLeaves(path =>
					path.Node == QuadTree.Node.Full && !island.Contains(path)
						? QuadTree.Node.Full
						: QuadTree.Node.Empty);
			}
		}
		
		public static IEnumerable<IReadOnlyList<ClosedPolygon>>
			Flatten(Octree octree, float layerHeight, Vec2 zRange) =>
			Enumerable.Range(0, (int) MathF.Ceiling((zRange.Y - zRange.X) / layerHeight)).AsParallel().AsOrdered()
				.Select(layerIdx => {
					var zBase = zRange.X + layerIdx * layerHeight;
					var quadTree = QuadTree.FromOctree(octree, new Vec2(zBase, zBase + layerHeight));
					return FindAllIslands(quadTree).Select(x => IslandToPolygon(zBase + layerHeight / 2, x)).ToList();
				}).AsSequential();

		static ClosedPolygon IslandToPolygon(float z, QuadTree island) {
			island = island.ExpandIntoVoid().ExpandIntoOppositeVoid();

			IEnumerable<Vec2> ToPoints(QuadTree quadTree) {
				quadTree = quadTree.ExpandIntoVoid().ExpandIntoOppositeVoid();
				var segments = quadTree.RootPath.FullLeaves.Select(x =>
					x.AllAdjacentBottom().Where(y => y.Node == QuadTree.Node.Empty).Select(y =>
							(new Vec2(MathF.Max(x.Min.X, y.Min.X), x.Min.Y),
								new Vec2(MathF.Min(x.Max.X, y.Max.X), x.Min.Y)))
						.Concat(x.AllAdjacentTop().Where(y => y.Node == QuadTree.Node.Empty).Select(y =>
							(new Vec2(MathF.Max(x.Min.X, y.Min.X), x.Max.Y),
								new Vec2(MathF.Min(x.Max.X, y.Max.X), x.Max.Y))))
						.Concat(x.AllAdjacentLeft().Where(y => y.Node == QuadTree.Node.Empty).Select(y =>
							(new Vec2(x.Min.X, MathF.Max(x.Min.Y, y.Min.Y)),
								new Vec2(x.Min.X, MathF.Min(x.Max.Y, y.Max.Y)))))
						.Concat(x.AllAdjacentRight().Where(y => y.Node == QuadTree.Node.Empty).Select(y =>
							(new Vec2(x.Max.X, MathF.Max(x.Min.Y, y.Min.Y)),
								new Vec2(x.Max.X, MathF.Min(x.Max.Y, y.Max.Y)))))).SelectMany(x => x).ToList();

				var list = new LinkedList<Vec2>(new[] { segments[0].Item1 });
				while(segments.Count != 0)
					for(var i = 0; i < segments.Count; ++i) {
						var (a, b) = segments[i];
						if((list.Last.Value - a).LengthSquared < 0.000001f) list.AddLast(b);
						else if((list.Last.Value - b).LengthSquared < 0.000001f) list.AddLast(a);
						else if((list.First.Value - a).LengthSquared < 0.000001f) list.AddFirst(b);
						else if((list.First.Value - b).LengthSquared < 0.000001f) list.AddFirst(a);
						else continue;
						segments.RemoveAt(i);
						break;
					}
				return list.ToList();
			}

			var emptyExternal = island.ExternallyEmpty();
			var holesOnly = island.TransformLeaves(x =>
				x.Node == QuadTree.Node.Full || (x.Node == QuadTree.Node.Empty && emptyExternal.Contains(x))
					? QuadTree.Node.Empty
					: QuadTree.Node.Full);
			var noHoles = island.TransformLeaves(x =>
				x.Node == QuadTree.Node.Empty && !emptyExternal.Contains(x)
					? QuadTree.Node.Full
					: x.Node);

			return new ClosedPolygon(ToPoints(noHoles), FindAllIslands(holesOnly).Select(ToPoints));
		}
	}
}