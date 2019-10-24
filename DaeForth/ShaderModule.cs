using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MoreLinq;

namespace DaeForth {
	public class ShaderModule : DaeforthModule {
		static readonly CommonModule _Module = new CommonModule();
		public override DaeforthModule Module => _Module;

		public ShaderModule() {
			var intrinsic_1 = ("abs acos acosh asin asinh atan atanh ceil cos cosh " +
			                   "degrees dFdx dFdy dFdxCoarse dFdyCoarse dFdxFine dFdyFine " +
			                   "exp exp2 floor fract fwidth fwidthCoarse fwidthFine interpolateAtCentroid " + 
			                   "inversesqrt log log2 normalize radians round roundEven sign sin sinh sqrt " + 
			                   "tan tanh transpose trunc").Split(" ");

			intrinsic_1.ForEach(name => AddWordHandler(name, compiler => {
				var value = Compiler.CanonicalizeValue(compiler.Pop());
				compiler.Push(new Ir.Call {
					Functor = new Ir.Identifier(name), Arguments = new Ir.List(new[] { value }), Type = value.Type
				});
			}));

			var intrinsic_2 = ("min max cross mod").Split(" ");

			intrinsic_2.ForEach(name => AddWordHandler(name, compiler => {
				var b = Compiler.CanonicalizeValue(compiler.Pop());
				var a = Compiler.CanonicalizeValue(compiler.Pop());
				compiler.Push(new Ir.Call {
					Functor = new Ir.Identifier(name), Arguments = new Ir.List(new[] { a, b }), 
					Type = a.Type == typeof(float) ? b.Type : a.Type
				});
			}));

			var intrinsic_3 = ("mix clamp").Split(" ");

			intrinsic_3.ForEach(name => AddWordHandler(name, compiler => {
				var c = Compiler.CanonicalizeValue(compiler.Pop());
				var b = Compiler.CanonicalizeValue(compiler.Pop());
				var a = Compiler.CanonicalizeValue(compiler.Pop());
				compiler.Push(new Ir.Call {
					Functor = new Ir.Identifier(name), Arguments = new Ir.List(new[] { a, b, c }), 
					Type = a.Type
				});
			}));

			AddWordHandler("dot", compiler => {
				var b = Compiler.CanonicalizeValue(compiler.Pop());
				var a = Compiler.CanonicalizeValue(compiler.Pop());
				compiler.Push(new Ir.Call {
					Functor = new Ir.Identifier("dot"), Arguments = new Ir.List(new[] { a, b }), 
					Type = typeof(float)
				});
			});

			AddWordHandler("length",
				compiler => compiler.Push(new Ir.Call {
					Functor = new Ir.Identifier("length"), Arguments = new Ir.List(new[] { Compiler.CanonicalizeValue(compiler.Pop()) }),
					Type = typeof(float)
				}));
			
			var CatchFloatRegex = new Regex(@"\.[0-9]+$");

			AddPrefixHandler(".", (compiler, token) => {
				Type GenVecType(int size) =>
					size switch {
						1 => typeof(float),
						2 => typeof(Vec2),
						3 => typeof(Vec3),
						4 => typeof(Vec4),
						_ => throw new NotSupportedException()
					};
				
				var letterMap = new Dictionary<char, int> {
					['x'] = 0, 
					['y'] = 1, 
					['z'] = 2, 
					['w'] = 3,
					
					['r'] = 0, 
					['g'] = 1, 
					['b'] = 2, 
					['a'] = 3 
				};
				
				if(CatchFloatRegex.IsMatch(token.RawValue)) return false;
				var pattern = token.Value;
				var vec = compiler.Pop();
				if(vec.IsConstant && vec is Ir.List list)
					foreach(var spattern in pattern.Split('.'))
						compiler.Push(spattern.Length == 1
							? list[letterMap[spattern[0]]]
							: new Ir.List(spattern.Select(x => list[letterMap[x]])));
				else {
					if(pattern.Contains('.'))
						vec = compiler.EnsureCheap(vec);
					foreach(var spattern in pattern.Split('.'))
						compiler.Push(new Ir.MemberAccess(vec, spattern) { Type = GenVecType(spattern.Length) });
				}
				return true;
			});
			
			AddWordHandler("matrix", compiler => {
				var lists = compiler.TryPop<Ir.List>();
				if(lists == null) return false;
				compiler.Push(new Ir.Call {
					Functor = new Ir.Identifier($"mat{lists.Count}"),
					Arguments = new Ir.List(lists.Select(Compiler.CanonicalizeValue)), Type = typeof(Matrix4x4)
				});
				return true;
			});
		}
	}
}