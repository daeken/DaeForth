using System;
using System.Collections.Generic;
using System.Linq;
using DaeForth;
using MoreLinq;

namespace IsoSlice {
	public class Octree {
		public class Node {
			public static readonly Node Empty = new Node();
			public static readonly Node Full = new Node();

			public Node[] Children;
		}

		public class Path {
			public readonly Path Parent;
			public readonly Vec3 Min, Max, Center;
			public readonly int Octant;
			public readonly Node Node;
			public readonly int Depth;
			
			public bool IsEmpty => Node == Node.Empty;
			public bool IsFull => Node == Node.Full;

			public IEnumerable<int> CompletePath =>
				Parent == null ? new[] { Octant } : new[] { Octant }.Concat(Parent.CompletePath);

			public IEnumerable<Path> Leaves =>
				Node.Children == null
					? new[] { this }
					: Enumerable.Range(0, 8).Select(GetChild).Select(x => x.Leaves)
						.Aggregate((a, b) => a.Concat(b));
			
			public IEnumerable<Path> EmptyLeaves =>
				Node.Children == null
					? (Node == Node.Full ? Enumerable.Empty<Path>() : new[] { this })
					: Enumerable.Range(0, 8).Select(GetChild).Select(x => x.EmptyLeaves)
						.Aggregate((a, b) => a.Concat(b));

			public IEnumerable<Path> FullLeaves =>
				Node.Children == null
					? (Node == Node.Empty ? Enumerable.Empty<Path>() : new[] { this })
					: Enumerable.Range(0, 8).Select(GetChild).Select(x => x.FullLeaves)
						.Aggregate((a, b) => a.Concat(b));

			public IEnumerable<Path> Children => Node.Children == null
				? null
				: new[] {
					GetChild(0), GetChild(1), GetChild(2), GetChild(3),
					GetChild(4), GetChild(5), GetChild(6), GetChild(7)
				};

			internal Path(Path parent, Vec3 min, Vec3 max, int octant, Node node) {
				Parent = parent;
				Min = min;
				Max = max;
				Center = (max - min) / 2 + min;
				Octant = octant;
				Node = node;
				Depth = parent?.Depth + 1 ?? 0;
			}

			public Path GetChild(int octant) {
				var halfSize = (Max - Min) / 2;
				var corner = Center - halfSize * Octants[octant];
				return new Path(this, corner, corner + halfSize, octant, Node.Children[octant]);
			}

			public Path MaybeGetChild(int octant) {
				if(Node.Children == null) return this;
				var halfSize = (Max - Min) / 2;
				var corner = Center - halfSize * Octants[octant];
				return new Path(this, corner, corner + halfSize, octant, Node.Children[octant]);
			}

			Path Adjacent(int directionBit, bool invert) {
				if(Parent == null) return null;
				var onOpposite = (Octant & directionBit) != 0;
				return onOpposite ^ invert
					? Parent.MaybeGetChild(Octant ^ directionBit)
					: Parent.Adjacent(directionBit, invert)?.MaybeGetChild(Octant ^ directionBit);
			}

			public Path AdjacentLeft() => Adjacent(4, false);
			public Path AdjacentRight() => Adjacent(4, true);
			public Path AdjacentBack() => Adjacent(2, false);
			public Path AdjacentFront() => Adjacent(2, true);
			public Path AdjacentBottom() => Adjacent(1, false);
			public Path AdjacentTop() => Adjacent(1, true);

			IEnumerable<Path> AllAdjacent(int directionBit, bool invert) {
				var adj = Adjacent(directionBit, invert);
				if(adj == null) return Enumerable.Empty<Path>();

				IEnumerable<Path> SubAdjacent(Path node) {
					if(node.Node.Children == null)
						yield return node;
					else
						for(var i = 0; i < 8; ++i)
							if(invert ^ ((i & directionBit) != 0))
								foreach(var elem in SubAdjacent(node.GetChild(i))) yield return elem;
				}

				return SubAdjacent(adj);
			}

			public IEnumerable<Path> AllAdjacent() =>
				new Func<Path, IEnumerable<Path>>[] {
					x => x.AllAdjacentLeft(),
					x => x.AllAdjacentRight(),
					x => x.AllAdjacentBack(),
					x => x.AllAdjacentFront(),
					x => x.AllAdjacentBottom(),
					x => x.AllAdjacentTop()
				}.Select(x => x(this)).SelectMany(x => x).Distinct();

			public IEnumerable<Path> AllAdjacentLeft() => AllAdjacent(4, false);
			public IEnumerable<Path> AllAdjacentRight() => AllAdjacent(4, true);
			public IEnumerable<Path> AllAdjacentBack() => AllAdjacent(2, false);
			public IEnumerable<Path> AllAdjacentFront() => AllAdjacent(2, true);
			public IEnumerable<Path> AllAdjacentBottom() => AllAdjacent(1, false);
			public IEnumerable<Path> AllAdjacentTop() => AllAdjacent(1, true);

			public bool Intersects(Vec3 min, Vec3 max) =>
				!(Max.X < min.X || Max.Y < min.Y || Max.Z < min.Z ||
				  Min.X > max.X || Min.Y > max.Y || Min.Z > max.Z) ||
				(Min.X >= min.X && Min.Y >= min.Y && Min.Z >= min.Z &&
				 Max.X <= max.X && Max.Y <= max.Y && Max.Z <= max.Z) ||
				(min.X >= Min.X && min.Y >= Min.Y && min.Z >= Min.Z &&
				 max.X <= Max.X && max.Y <= Max.Y && max.Z <= Max.Z);
			
			public bool Contains(Vec3 point) =>
				Min.X <= point.X && Max.X >= point.X &&
				Min.Y <= point.Y && Max.Y >= point.Y &&
				Min.Z <= point.Z && Max.Z >= point.Z;

			public bool Contains(Vec2 point) =>
				Min.X <= point.X && Max.X >= point.X &&
				Min.Y <= point.Y && Max.Y >= point.Y;

			public override int GetHashCode() => HashCode.Combine(Parent == null ? -1 : Parent.GetHashCode(), Octant);

			public override bool Equals(object obj) =>
				obj is Path other && Depth == other.Depth && Octant == other.Octant &&
				CompletePath.SequenceEqual(other.CompletePath);
			public override string ToString() => (Parent == null ? "" : $"{Parent} -> ") + $"{Octant} ({Min})";
		}
		
		// These indicate if there is a node adjacent forward on that axis -- INVERSE OF THE POSITION
		// E.g. 111 indicates the bottom-back-left node, where there are nodes adjacent on X Y and Z
		public static readonly Vec3[] Octants = {
			new Vec3(1, 1, 1), 
			new Vec3(1, 1, 0), 
			new Vec3(1, 0, 1), 
			new Vec3(1, 0, 0), 
			new Vec3(0, 1, 1), 
			new Vec3(0, 1, 0), 
			new Vec3(0, 0, 1), 
			new Vec3(0, 0, 0)
		};

		public readonly Vec3 Min, Max;
		public readonly Node Root;
		public readonly Path RootPath;
		public readonly Func<Vec3, float> Isosurface;

		public int? _LeafCount;
		public int LeafCount => _LeafCount ??= RootPath.Leaves.Count();
		
		public static Octree Build(Func<Vec3, float> map, Vec3 mins, Vec3 maxs, int maxDepth) {
			Node BuildOctree(Vec3 min, Vec3 max, int depth) {
				var halfSize = (max - min) / 2;
				var center = min + halfSize;
				var cval = map(center);
				if(cval > 0.00001f && cval > halfSize.Length * 2)
					return Node.Empty;
				if(depth == maxDepth) return Node.Full;

				var children = Octants.Select(x => {
					var corner = center - halfSize * x;
					return BuildOctree(corner, corner + halfSize, depth + 1);
				}).ToArray();

				return children.All(x => x == Node.Empty)
					? Node.Empty
					: children.All(x => x == Node.Full)
						? Node.Full
						: new Node { Children = children };
			}

			return new Octree(mins, maxs, BuildOctree(mins, maxs, 0), map).Prune();
		}

		public Octree(Vec3 min, Vec3 max, Node root, Func<Vec3, float> isosurface) {
			Min = min;
			Max = max;
			Root = root;
			RootPath = new Path(null, min, max, -1, root);
			Isosurface = isosurface;
		}
		
		public static Octree FromQuadTree(QuadTree quadTree, Vec2 zRange) {
			Node Convert(QuadTree.Node quadNode) =>
				quadNode == QuadTree.Node.Empty
					? Node.Empty
					: quadNode == QuadTree.Node.Full
						? Node.Full
						: new Node {
							Children = quadNode.Children.Select(x => new[] { Convert(x), Node.Empty })
								.SelectMany(x => x).ToArray()
						};

			return new Octree(new Vec3(quadTree.Min, zRange.X), new Vec3(quadTree.Max, zRange.Y),
				Convert(quadTree.Root), quadTree.Isosurface);
		}
		
		public delegate Node OctreeTransformer(Path path, Func<Node> transformChildren);

		public Octree Transform(OctreeTransformer transformer, bool recursive = false) {
			Node SubTransform(Path path) {
				Node TransformChildren() {
					var cur = path.Node;
					if(cur == Node.Empty || cur == Node.Full) return cur;
					var halfSize = (path.Max - path.Min) / 2;
					var children = cur.Children.Select((node, i) => {
						var corner = path.Center - halfSize * Octants[i];
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
					? SubTransform(new Path(path.Parent, path.Min, path.Max, path.Octant, outNode))
					: outNode;
			}
			
			return new Octree(Min, Max, SubTransform(new Path(null, Min, Max, -1, Root)), Isosurface).Prune();
		}

		public Octree TransformLeaves(Func<Path, Node> transformer, bool recursive = false) {
			Node SubTransform(Path path) {
				if(path.Node.Children == null) {
					var outNode = transformer(path);
					return recursive && outNode != path.Node
						? SubTransform(new Path(path.Parent, path.Min, path.Max, path.Octant, outNode))
						: outNode;
				}
				
				var cur = path.Node;
				if(cur == Node.Empty || cur == Node.Full) return cur;
				var halfSize = (path.Max - path.Min) / 2;
				var children = cur.Children.Select((node, i) => {
					var corner = path.Center - halfSize * Octants[i];
					return SubTransform(new Path(path, corner, corner + halfSize, i, node));
				}).ToArray();
				return children.All(x => x == Node.Empty)
					? Node.Empty
					: children.All(x => x == Node.Full)
						? Node.Full
						: new Node { Children = children };
			}
			
			return new Octree(Min, Max, SubTransform(new Path(null, Min, Max, -1, Root)), Isosurface).Prune();
		}

		public IEnumerable<Path> FindLeaves(Func<Path, bool> pathPredicate) {
			IEnumerable<Path> SubFind(Path path) {
				return !pathPredicate(path)
					? Enumerable.Empty<Path>()
					: path.Node.Children == null
						? new[] { path }
						: Enumerable.Range(0, 8).Select(i => SubFind(path.GetChild(i))).SelectMany(x => x);
			}
			return SubFind(RootPath);
		}

		public Octree Prune() {
			if(Root.Children == null) return this;
			var emptyCount = Root.Children.Count(x => x == Node.Empty);
			if(emptyCount == 8) return new Octree(Min, Max, Node.Empty, Isosurface);
			if(emptyCount < 7) return this;
			var (octant, node) = Root.Children.Enumerate().First(x => x.Value != Node.Empty);
			var halfSize = (Max - Min) / 2;
			var corner = RootPath.Center - halfSize * Octants[octant];
			return new Octree(corner, corner + halfSize, node, Isosurface).Prune();
		}

		public Octree ExpandIntoVoid() =>
			new Octree(Min - (Max - Min), Max, new Node {
				Children = new[] {
					Node.Empty, Node.Empty, Node.Empty, Node.Empty,
					Node.Empty, Node.Empty, Node.Empty, Root
				}
			}, Isosurface);

		HashSet<Path> ExternallyEmpty() {
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

		public Octree RemoveInternalVoids() {
			var tempTree = ExpandIntoVoid();
			Console.WriteLine("Finding external empty cells");
			var externalEmpty = tempTree.ExternallyEmpty();
			if(externalEmpty.Count == tempTree.RootPath.EmptyLeaves.Count()) return this;
			Console.WriteLine("Removing voids");
			return tempTree.TransformLeaves(path =>
				path.Node == Node.Empty && externalEmpty.Contains(path) ? Node.Empty : Node.Full);
		}

		public HashSet<Path> FindSurface() {
			var externalEmpty = ExternallyEmpty();
			var beenQueued = new HashSet<Path>(externalEmpty);
			var processing = new Queue<Path>(externalEmpty);
			var surface = new HashSet<Path>();
			while(processing.TryDequeue(out var path)) {
				if(path.Node == Node.Empty)
					path.AllAdjacent().Where(x => !beenQueued.Contains(x)).ForEach(x => {
						beenQueued.Add(x);
						processing.Enqueue(x);
					});
				else
					path.Leaves.Where(x => x.Node == Node.Full).ForEach(x => surface.Add(x));
			}
			return surface;
		}

		public Octree RefineSurface(int maxDepth) {
			var tempTree = ExpandIntoVoid();
			var surface = tempTree.FindSurface();
			var ntree = tempTree.Transform((path, transformChildren) =>
				path.Node == Node.Full && surface.Contains(path)
					? path.Depth >= maxDepth
						? Node.Full
						: Build(Isosurface, path.Min, path.Max, maxDepth - path.Depth).Root
					: transformChildren());
			Console.WriteLine($"Refined... {ntree.LeafCount} {tempTree.LeafCount} {LeafCount}??");
			return ntree.LeafCount == LeafCount
				? ntree
				: ntree.RefineSurface(maxDepth);
		}

		public Octree StripNonsurface() {
			var tempTree = ExpandIntoVoid();
			Console.WriteLine("Finding surface");
			var surface = tempTree.FindSurface();
			Console.WriteLine("Removing nonsurface");
			return tempTree.TransformLeaves(path =>
				path.Node == Node.Full && surface.Contains(path)
					? Node.Full
					: Node.Empty
			);
		}

		public Octree MakeFullDepthUniform() {
			var maxDepth = RootPath.FullLeaves.Select(x => x.Depth).Max();

			Node Uniform(Node node, int depth) {
				if(depth == maxDepth || node == Node.Empty) return node;
				if(node == Node.Full)
					return Uniform(
						new Node {
							Children = new[] {
								Node.Full, Node.Full, Node.Full, Node.Full, Node.Full, Node.Full, Node.Full, Node.Full
							}
						},
						depth + 1);
				return new Node { Children = node.Children.Select(x => Uniform(x, depth + 1)).ToArray() };
			}
			return new Octree(Min, Max, Uniform(Root, 0), Isosurface);
		}

		public Octree CollapseZ() {
			Node SubCollapse(Node node) {
				if(node == Node.Empty || node == Node.Full) return node;

				var children = new[] {
					Node.Empty, Node.Empty, Node.Empty, Node.Empty, 
					Node.Empty, Node.Empty, Node.Empty, Node.Empty
				};
				for(var i = 0; i < 8; i += 2) {
					var bottom = SubCollapse(node.Children[i]);
					var top = SubCollapse(node.Children[i + 1]);
					children[i] = bottom == top
						? bottom
						: bottom == Node.Full || top == Node.Full
							? Node.Full
							: SubCollapse(new Node {
								Children = Enumerable.Range(0, 8)
									.Select(i =>
										i % 2 == 0
											? bottom.Children?[i] ?? Node.Empty
											: top.Children?[i - 1] ?? Node.Empty).ToArray()
							});
				}
				return children.All(x => x == Node.Empty)
					? Node.Empty
					: children.Enumerate().All(x => x.Index % 2 == 1 || x.Value == Node.Full)
						? Node.Full
						: new Node { Children = children };
			}
			return new Octree(Min, Max + new Vec3(0, 0, Max.Z - Min.Z), SubCollapse(Root), Isosurface);
		}
	}
}