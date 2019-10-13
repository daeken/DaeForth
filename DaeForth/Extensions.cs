using System;

namespace DaeForth {
	public static class Extensions {
		public static Ir Box<T>(this T value) =>
			(Ir) Activator.CreateInstance(typeof(Ir.ConstValue<>).MakeGenericType(value.GetType()), value);
	}
}