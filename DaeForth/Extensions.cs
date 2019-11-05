using System;
using System.Collections.Generic;
using System.Linq;

namespace DaeForth {
	public static class Extensions {
		internal static Ir Box<T>(this T value) =>
			(Ir) Activator.CreateInstance(typeof(Ir.ConstValue<>).MakeGenericType(value.GetType()), value);
		
		internal static string Indent(this string code, int level = 1) =>
			string.Join("\n", code.Split('\n').Select(x => new string('\t', level) + x));

		internal static string ToOperator(this BinaryOp op) =>
			op switch {
				BinaryOp.Add => "+",
				BinaryOp.Subtract => "-",
				BinaryOp.Multiply => "*",
				BinaryOp.Divide => "/",
				BinaryOp.Modulus => "%",
				BinaryOp.Equal => "==",
				BinaryOp.NotEqual => "!=",
				BinaryOp.LessThanOrEqual => "<=",
				BinaryOp.LessThan => "<",
				BinaryOp.GreaterThan => ">",
				BinaryOp.GreaterThanOrEqual => ">=", 
				BinaryOp.LogicalAnd => "&&", 
				BinaryOp.LogicalOr => "||", 
				_ => throw new NotImplementedException()
			};

		internal static string ToOperator(this UnaryOp op) =>
			op switch {
				UnaryOp.LogicalNegate => "!",
				UnaryOp.BitwiseNegate => "~",
				UnaryOp.Minus => "-",
				_ => throw new NotImplementedException()
			};

		public static IEnumerable<(int Index, T Value)> Enumerate<T>(this IEnumerable<T> enumerable) =>
			enumerable.Select((x, i) => (i, x));
	}
}