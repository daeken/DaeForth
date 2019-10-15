using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;
using MoreLinq;

namespace DaeForth {
	public class ShaderModule : DaeforthModule {
		static readonly CommonModule _Module = new CommonModule();
		public override DaeforthModule Module => _Module;

		public ShaderModule() {
			var intrinsics = ("abs acos acosh asin asinh atan atanh ceil cos cosh " +
			                  "degrees dFdx dFdy dFdxCoarse dFdyCoarse dFdxFine dFdyFine " +
			                  "exp exp2 floor fract fwidth fwidthCoarse fwidthFine interpolateAtCentroid " + 
			                  "inversesqrt log log2 normalize radians round roundEven sign sin sinh sqrt " + 
			                  "tan tanh trunc").Split(" ");

			intrinsics.ForEach(name => AddWordHandler(name, compiler => {
				var value = compiler.Pop();
				compiler.Push(new Ir.Call {
					Functor = new Ir.Identifier(name), Arguments = new Ir.List(new[] { Compiler.CanonicalizeValue(value) }), Type = value.Type
				});
			}));

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
						2 => typeof(Vector2),
						3 => typeof(Vector3),
						4 => typeof(Vector4),
						_ => throw new NotSupportedException()
					};
				
				if(CatchFloatRegex.IsMatch(token.RawValue)) return false;
				var pattern = token.Value;
				var vec = compiler.Pop();
				if(vec.IsConstant) throw new NotImplementedException();
				else {
					if(pattern.Contains('.'))
						vec = compiler.EnsureCheap(vec);
					foreach(var spattern in pattern.Split('.'))
						compiler.Push(new Ir.MemberAccess(vec, spattern) { Type = GenVecType(spattern.Length) });
				}
				return true;
			});
		}
	}
}