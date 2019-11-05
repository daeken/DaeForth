using System;
using System.Collections.Generic;
using System.Linq;
using DaeForth;
using MoreLinq;
using PrettyPrinter;

namespace IsoSlice {
	public class QuadTree {
		public class Node {
			public static readonly Node Empty = new Node();
			public static readonly Node Full = new Node();

			public Node[] Children;
		}

		public class Path {
			public readonly Path Parent;
			public readonly Vec2 Min, Max, Center;
			public readonly int Quadrant;
			public readonly Node Node;
			public readonly int Depth;

			public bool IsEmpty => Node == Node.Empty;
			public bool IsFull => Node == Node.Full;

			public IEnumerable<int> CompletePath =>
				Parent == null ? new[] { Quadrant } : new[] { Quadrant }.Concat(Parent.CompletePath);

			public IEnumerable<Path> Leaves =>
				Node.Children == null
					? new[] { this }
					: Enumerable.Range(0, 4).Select(GetChild).Select(x => x.Leaves)
						.Aggregate((a, b) => a.Concat(b));

			public IEnumerable<Path> EmptyLeaves =>
				Node.Children == null
					? (Node == Node.Full ? Enumerable.Empty<Path>() : new[] { this })
					: Enumerable.Range(0, 4).Select(GetChild).Select(x => x.EmptyLeaves)
						.Aggregate((a, b) => a.Concat(b));

			public IEnumerable<Path> FullLeaves =>
				Node.Children == null
					? (Node == Node.Empty ? Enumerable.Empty<Path>() : new[] { this })
					: Enumerable.Range(0, 4).Select(GetChild).Select(x => x.FullLeaves)
						.Aggregate((a, b) => a.Concat(b));

			public IEnumerable<Path> Children => Node.Children == null
				? null
				: new[] { GetChild(0), GetChild(1), GetChild(2), GetChild(3) };

			internal Path(Path parent, Vec2 min, Vec2 max, int quadrant, Node node) {
				Parent = parent;
				Min = min;
				Max = max;
				Center = (max - min) / 2 + min;
				Quadrant = quadrant;
				Node = node;
				Depth = parent?.Depth + 1 ?? 0;
			}

			public Path MaybeGetChild(int quadrant) {
				if(Node.Children == null) return this;
				var halfSize = (Max - Min) / 2;
				var corner = Center - halfSize * Quadrants[quadrant];
				return new Path(this, corner, corner + halfSize, quadrant, Node.Children[quadrant]);
			}

			public Path GetChild(int quadrant) {
				var halfSize = (Max - Min) / 2;
				var corner = Center - halfSize * Quadrants[quadrant];
				return new Path(this, corner, corner + halfSize, quadrant, Node.Children[quadrant]);
			}

			Path Adjacent(int directionBit, bool invert) {
				if(Parent == null) return null;
				var onOpposite = (Quadrant & directionBit) != 0;
				return onOpposite ^ invert
					? Parent.MaybeGetChild(Quadrant ^ directionBit)
					: Parent.Adjacent(directionBit, invert)?.MaybeGetChild(Quadrant ^ directionBit);
			}

			public Path AdjacentLeft() => Adjacent(2, false);
			public Path AdjacentRight() => Adjacent(2, true);
			public Path AdjacentBottom() => Adjacent(1, false);
			public Path AdjacentTop() => Adjacent(1, true);

			IEnumerable<Path> AllAdjacent(int directionBit, bool invert) {
				var adj = Adjacent(directionBit, invert);
				if(adj == null) return Enumerable.Empty<Path>();

				IEnumerable<Path> SubAdjacent(Path node) {
					if(node.Node.Children == null) {
						yield return node;
					} else
						for(var i = 0; i < 4; ++i)
							if(invert ^ ((i & directionBit) != 0))
								foreach(var elem in SubAdjacent(node.GetChild(i))) yield return elem;
				}

				return SubAdjacent(adj);
			}

			public IEnumerable<Path> AllAdjacent() =>
				new Func<Path, IEnumerable<Path>>[] {
					x => x.AllAdjacentLeft(),
					x => x.AllAdjacentRight(),
					x => x.AllAdjacentTop(),
					x => x.AllAdjacentBottom(),
				}.Select(x => x(this)).SelectMany(x => x).Distinct();

			public IEnumerable<Path> AllAdjacentLeft() => AllAdjacent(2, false);
			public IEnumerable<Path> AllAdjacentRight() => AllAdjacent(2, true);
			public IEnumerable<Path> AllAdjacentBottom() => AllAdjacent(1, false);
			public IEnumerable<Path> AllAdjacentTop() => AllAdjacent(1, true);

			public bool Intersects(Vec2 min, Vec2 max) =>
				!(Max.X < min.X || Max.Y < min.Y ||
				  Min.X > max.X || Min.Y > max.Y) ||
				(Min.X >= min.X && Min.Y >= min.Y &&
				 Max.X <= max.X && Max.Y <= max.Y) ||
				(min.X >= Min.X && min.Y >= Min.Y &&
				 max.X <= Max.X && max.Y <= Max.Y);

			public bool Contains(Vec2 point) =>
				Min.X <= point.X && Max.X >= point.X &&
				Min.Y <= point.Y && Max.Y >= point.Y;

			public override int GetHashCode() => HashCode.Combine(Parent == null ? -1 : Parent.GetHashCode(), Quadrant);

			public override bool Equals(object obj) =>
				obj is Path other && Depth == other.Depth && Quadrant == other.Quadrant &&
				CompletePath.SequenceEqual(other.CompletePath);
			public override string ToString() => (Parent == null ? "" : $"{Parent} -> ") + $"{Quadrant} ({Min})";
		}
		
		// These indicate if there is a node adjacent forward on that axis -- INVERSE OF THE POSITION
		// E.g. 111 indicates the bottom-left node, where there are nodes adjacent on X and Y
		public static readonly Vec2[] Quadrants = {
			new Vec2(1, 1), 
			new Vec2(1, 0), 
			new Vec2(0, 1), 
			new Vec2(0, 0)
		};

		public readonly Vec2 Min, Max;
		public readonly Node Root;
		public readonly Path RootPath;
		public readonly Func<Vec3, float> Isosurface;
		public int? _LeafCount;
		public int LeafCount => _LeafCount ??= RootPath.Leaves.Count();

		public QuadTree(Vec2 min, Vec2 max, Node root, Func<Vec3, float> isosurface) {
			Min = min;
			Max = max;
			Root = root;
			RootPath = new Path(null, min, max, -1, root);
			Isosurface = isosurface;
		}

		public static QuadTree FromOctree(Octree octree) {
			Node Convert(Octree.Node octNode) =>
				octNode == Octree.Node.Empty
					? Node.Empty
					: octNode == Octree.Node.Full
						? Node.Full
						: new Node {
							Children = octNode.Children.Enumerate().Where(x => x.Index % 2 == 0)
								.Select(x => Convert(x.Value)).ToArray()
						};
			
			return new QuadTree(octree.Min.xy, octree.Max.xy, Convert(octree.Root), octree.Isosurface);
		}

		public static QuadTree FromOctree(Octree octree, Vec2 zRange) {
			var sliceMin = new Vec3(octree.Min.xy, zRange.X);
			var sliceMax = new Vec3(octree.Max.xy, zRange.Y);
			var heightThreshold = (zRange.Y - zRange.X) / 2 * 0.99f;
			octree = octree.Transform((path, transformChildren) =>
				path.Intersects(sliceMin, sliceMax) ? transformChildren() : Octree.Node.Empty);
			if(!octree.RootPath.FullLeaves.Any())
				return new QuadTree(octree.Min.xy, octree.Max.xy, Node.Empty, octree.Isosurface);
			octree = octree.MakeFullDepthUniform();
			var maxDepth = octree.RootPath.FullLeaves.First().Depth;
			Node Build(Vec2 min, Vec2 max, int depth) {
				if(depth < maxDepth) {
					var halfSize = (max - min) / 2;
					var center = min + halfSize;
					var children = Quadrants.Select(x => {
						var corner = center - halfSize * x;
						return Build(corner, corner + halfSize, depth + 1);
					}).ToArray();
					return children.All(x => x == Node.Empty)
						? Node.Empty
						: children.All(x => x == Node.Full)
							? Node.Full
							: new Node { Children = children };
				}

				var vcenter = (max - min) / 2 + min;
				var height = octree.FindLeaves(path => path.Contains(vcenter)).Where(path => path.IsFull)
					.Select(x => x.Max.Z - x.Min.Z).DefaultIfEmpty(0).Sum();
				return height >= heightThreshold ? Node.Full : Node.Empty;
			}
			return new QuadTree(octree.Min.xy, octree.Max.xy, Build(octree.Min.xy, octree.Max.xy, 0), octree.Isosurface);
		}
		
		public delegate Node QuadTreeTransformer(Path path, Func<Node> transformChildren);

		public QuadTree Transform(QuadTreeTransformer transformer, bool recursive = false) {
			Node SubTransform(Path path) {
				Node TransformChildren() {
					var cur = path.Node;
					if(cur == Node.Empty || cur == Node.Full) return cur;
					var halfSize = (path.Max - path.Min) / 2;
					var children = cur.Children.Select((node, i) => {
						var corner = path.Center - halfSize * Quadrants[i];
						return SubTransform(new Path(path, corner, corner + halfSize, i, node));
					}).ToArray();
					return children.All(x => x == Node.Empty)
						? Node.Empty
						: children.All(x => x == Node.Full)
							? Node.Full
							: new Node { Children = children };
				}

				var outNode = transformer(path, TransformChildren);
				return recursive && outNode != path.Node
					? SubTransform(new Path(path.Parent, path.Min, path.Max, path.Quadrant, outNode))
					: outNode;
			}
			
			return new QuadTree(Min, Max, SubTransform(new Path(null, Min, Max, -1, Root)), Isosurface).Prune();
		}

		public QuadTree TransformLeaves(Func<Path, Node> transformer, bool recursive = false) {
			Node SubTransform(Path path) {
				if(path.Node.Children == null) {
					var outNode = transformer(path);
					return recursive && outNode != path.Node
						? SubTransform(new Path(path.Parent, path.Min, path.Max, path.Quadrant, outNode))
						: outNode;
				}
				
				var cur = path.Node;
				if(cur == Node.Empty || cur == Node.Full) return cur;
				var halfSize = (path.Max - path.Min) / 2;
				var children = cur.Children.Select((node, i) => {
					var corner = path.Center - halfSize * Quadrants[i];
					return SubTransform(new Path(path, corner, corner + halfSize, i, node));
				}).ToArray();
				return children.All(x => x == Node.Empty)
					? Node.Empty
					: children.All(x => x == Node.Full)
						? Node.Full
						: new Node { Children = children };
			}
			
			return new QuadTree(Min, Max, SubTransform(new Path(null, Min, Max, -1, Root)), Isosurface).Prune();
		}

		public QuadTree Prune() {
			if(Root.Children == null) return this;
			var emptyCount = Root.Children.Count(x => x == Node.Empty);
			if(emptyCount == 4) return new QuadTree(Min, Max, Node.Empty, Isosurface);
			if(emptyCount < 3) return this;
			var (quadrant, node) = Root.Children.Enumerate().First(x => x.Value != Node.Empty);
			var halfSize = (Max - Min) / 2;
			var corner = RootPath.Center - halfSize * Quadrants[quadrant];
			return new QuadTree(corner, corner + halfSize, node, Isosurface).Prune();
		}

		public QuadTree ExpandIntoVoid() =>
			new QuadTree(Min - (Max - Min), Max,
				new Node { Children = new[] { Node.Empty, Node.Empty, Node.Empty, Root } }, Isosurface);

		public QuadTree ExpandIntoOppositeVoid() =>
			new QuadTree(Min, Max + (Max - Min),
				new Node { Children = new[] { Root, Node.Empty, Node.Empty, Node.Empty } }, Isosurface);

		public HashSet<Path> ExternallyEmpty() {
			var beenQueued = new HashSet<Path>();
			var processing = new Queue<Path>(new[] { RootPath.GetChild(0) });
			var externalEmpty = new HashSet<Path>();
			while(processing.TryDequeue(out var path)) {
				externalEmpty.Add(path);
				path.AllAdjacent().ForEach(x => {
					if(!x.IsEmpty || beenQueued.Contains(x)) return;
					beenQueued.Add(x);
					processing.Enqueue(x);
				});
			}

			return externalEmpty;
		}

		public QuadTree RemoveInternalVoids() {
			var tempTree = ExpandIntoVoid();
			Console.WriteLine("Finding external empty cells");
			var externalEmpty = tempTree.ExternallyEmpty();
			if(externalEmpty.Count == tempTree.RootPath.EmptyLeaves.Count()) return this;
			return tempTree.TransformLeaves(path =>
				path.IsEmpty && externalEmpty.Contains(path) ? Node.Empty : Node.Full);
		}

		public HashSet<Path> FindSurface() =>
			new HashSet<Path>(ExternallyEmpty().Select(x => x.AllAdjacent().Where(y => y.IsEmpty)).SelectMany(x => x)
				.Distinct());

		public QuadTree StripNonsurface() {
			var tempTree = ExpandIntoVoid();
			Console.WriteLine("Finding surface");
			var surface = tempTree.FindSurface();
			Console.WriteLine("Removing nonsurface");
			return tempTree.TransformLeaves(path =>
				path.IsFull && surface.Contains(path)
					? Node.Full
					: Node.Empty
			);
		}
	}
}