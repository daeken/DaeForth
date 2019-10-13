using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using PrettyPrinter;

namespace DaeForth {
	public class GlslBackend : Backend {
		public override string GenerateCode(IEnumerable<WordContext> words) =>
			string.Join("\n\n", words.Select(word => {
				var body = "";
				if(word.Locals.Count != 0)
					body += string.Join('\n', word.Locals.Select(x => $"{ToType(x.Value)} {ToName(x.Key)};")) + "\n";
				body += string.Join('\n', word.Body.Select(Transform).Where(x => x != null));
				return $"void {word.Name}() {{\n{body.Indent()}\n}}";
			}));

		string Transform(Ir expr) =>
			expr switch {
				Ir.Assignment ass =>
					ass.Value == null ? null : $"{Transform(ass.Identifier)} = {Transform(ass.Value)};",
				Ir.BinaryOperation bop =>
					$"({Transform(bop.Left)}) {bop.Op.ToOperator()} ({Transform(bop.Right)})", 
				Ir.ConstValue<int> icv => icv.Value.ToString(), 
				Ir.ConstValue<float> fcv => FormatFloat(fcv), 
				Ir.List list => $"vec{list.Count}({string.Join(", ", list.Select(Transform))})", 
				Ir.Identifier id => ToName(id.Name), 
				_ => throw new NotImplementedException(expr.ToPrettyString())
			};

		string FormatFloat(float value) {
			var str = value.ToString(CultureInfo.InvariantCulture);
			if(str.Contains('.')) return str;
			return str + ".";
		}
		
		string ToType(Type type) {
			if(type == typeof(int)) return "int";
			if(type == typeof(float)) return "float";
			if(type == typeof(Vector2)) return "vec2";
			if(type == typeof(Vector3)) return "vec3";
			if(type == typeof(Vector4)) return "vec4";
			throw new NotImplementedException($"Unknown type {type}");
		}

		string ToName(string name) {
			return name.Replace("-", "_");
		}
	}
}