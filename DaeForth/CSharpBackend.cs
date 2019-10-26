using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PrettyPrinter;

namespace DaeForth {
	public class CSharpBackend : Backend {
		public override string GenerateCode(IDictionary<string, (string Qualifier, Type Type)> globals,
			Dictionary<(string Name, Type Return, Type[] Arguments), WordContext> words) =>
			"using DaeForth;\n" + 
			"using static DaeForth.MathFunctions;\n" + 
			"public class DFShader {\n" +
			(globals.Count == 0
				? ""
				: string.Join('\n',
					  globals.Select(x =>
						  $"\tpublic static {ToType(x.Value.Type)} {ToName(x.Key)};")) +
				  "\n\n") +
			string.Join("\n\n", words.Select(word => {
				var body = "";
				if(word.Value.Locals.Count != 0)
					body += string.Join('\n', word.Value.Locals.Select(x => $"{ToType(x.Value)} {ToName(x.Key)};")) +
					        "\n";
				body += string.Join('\n', word.Value.Body.Select(Transform).Where(x => x != null));
				return $"public static {(word.Key.Return == null ? "void" : ToType(word.Key.Return))} {ToWordName(word.Key)}(" +
				       string.Join(", ", word.Key.Arguments.Select((x, i) => $"{ToType(x)} arg_{i}").Reverse()) +
				       $") {{\n{body.Indent()}\n}}";
			})).Indent() +
			"\n}\n";

		string ToWordName((string Name, Type Return, Type[] Arguments) key) =>
			key.Name == "main"
				? "main"
				: $"{ToName(key.Name)}_{ToType(key.Return)}_{string.Join('_', key.Arguments.Select(ToType))}";

		string Transform(Ir expr) =>
			expr switch {
				Ir.Assignment ass =>
				ass.Value == null ? null : $"{Transform(ass.Lhs)} = {Transform(ass.Value)};",
				Ir.BinaryOperation bop => $"({Transform(bop.Left)}) {bop.Op.ToOperator()} ({Transform(bop.Right)})",
				Ir.UnaryOperation uop => $"{uop.Op.ToOperator()}({Transform(uop.Value)})",
				Ir.Call call => $"{Transform(call.Functor)}({string.Join(", ", call.Arguments.Select(Transform))})",
				Ir.ConstValue<int> icv => icv.Value.ToString(),
				Ir.ConstValue<float> fcv => FormatFloat(fcv),
				Ir.ConstValue<bool> bcv => bcv ? "true" : "false",
				Ir.List list =>
				$"new {ToType(list.Type)}({string.Join(", ", list.Select(Transform))})",
				Ir.Identifier id => ToName(id.Name),
				Ir.MemberAccess ma => $"({Transform(ma.Value)}).{ma.Member}",
				Ir.If _if when _if.B is Ir.List ifList && ifList.Count == 0 =>
				$"if({Transform(_if.Cond)}) {{\n{string.Join('\n', ((Ir.List) _if.A).Select(Transform).Where(x => x != null)).Indent()}\n}}",
				Ir.If _if =>
				$"if({Transform(_if.Cond)}) {{\n{string.Join('\n', ((Ir.List) _if.A).Select(Transform).Where(x => x != null)).Indent()}\n}} else {{\n{string.Join('\n', ((Ir.List) _if.B).Select(Transform).Where(x => x != null)).Indent()}\n}}",
				Ir.For _for =>
				$"for(int {Transform(_for.Iterator)} = 0; {Transform(_for.Iterator)} < (int) ({Transform(_for.Count)}); ++({Transform(_for.Iterator)})) {{\n{string.Join('\n', ((Ir.List) _for.Body).Select(Transform).Where(x => x != null)).Indent()}\n}}",
				Ir.CallWord cw when cw.Type == null =>
				$"{ToWordName(cw.Word)}({string.Join(", ", cw.Arguments.Select(Transform))});",
				Ir.CallWord cw => $"{ToWordName(cw.Word)}({string.Join(", ", cw.Arguments.Select(Transform))})",
				Ir.Break _ => "break;",
				Ir.Continue _ => "continue;",
				Ir.Ternary ter => $"({Transform(ter.Cond)}) ? ({Transform(ter.A)}) : ({Transform(ter.B)})", 
				Ir.Return ret when ret.Value == null => "return;", 
				Ir.Return ret => $"return {Transform(ret.Value)};", 
				_ => throw new NotImplementedException(expr.ToPrettyString())
			};

		string FormatFloat(float value) => value.ToString(CultureInfo.InvariantCulture) + "f";

		string ToType(Type type) {
			if(type == null) return "void";
			if(type == typeof(int)) return "int";
			if(type == typeof(bool)) return "bool";
			if(type == typeof(float)) return "float";
			if(type == typeof(Vec2)) return "Vec2";
			if(type == typeof(Vec3)) return "Vec3";
			if(type == typeof(Vec4)) return "Vec4";
			if(type == typeof(Matrix4x4)) return "Matrix4x4";
			throw new NotImplementedException($"Unknown type {type.ToPrettyString()}");
		}

		string ToName(string name) {
			return name.Replace("-", "_");
		}
	}
}