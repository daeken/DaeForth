using System;
using System.Collections.Generic;
using MoreLinq;

namespace DaeForth {
	public class BinaryOpModule : DaeforthModule {
		static readonly BinaryOpModule _Module = new BinaryOpModule();
		public override DaeforthModule Module => _Module;

		public BinaryOpModule() {
			var ops = new Dictionary<string, (BinaryOp Op, Func<dynamic, dynamic, object> Func)> {
				["=="] = (BinaryOp.Equal, (a, b) => a == b), 
				["!="] = (BinaryOp.NotEqual, (a, b) => a != b),
				[">="] = (BinaryOp.GreaterThanOrEqual, (a, b) => a >= b), 
				[">"] = (BinaryOp.GreaterThan, (a, b) => a > b),
				["<="] = (BinaryOp.LessThanOrEqual, (a, b) => a <= b), 
				["<"] = (BinaryOp.LessThan, (a, b) => a < b),
				["+"] = (BinaryOp.Add, (a, b) => a + b), 
				["-"] = (BinaryOp.Subtract, (a, b) => a - b), 
				["*"] = (BinaryOp.Multiply, (a, b) => a * b), 
				["/"] = (BinaryOp.Divide, (a, b) => a / b), 
				["%"] = (BinaryOp.Modulus, (a, b) => a % b), 
			};
			
			ops.ForEach(op => AddWordHandler(op.Key, compiler => {
				var (a, b) = compiler.Pop<Ir, Ir>();
				if(a is Ir.IConstValue ica && b is Ir.IConstValue icb)
					compiler.PushValue(op.Value.Func(ica.Value, icb.Value));
				else if(a.IsConstant && b.IsConstant && (a is Ir.List || b is Ir.List)) {
					if(a is Ir.List la && b is Ir.List lb) {
						if(la.Count != lb.Count) throw new CompilerException("Lists have different lengths for binary operation");
						compiler.InjectToken("[");
						for(var i = 0; i < la.Count; ++i) {
							compiler.InjectToken(la[i]);
							compiler.InjectToken(lb[i]);
							compiler.InjectToken(op.Key);
						}
						compiler.InjectToken("]");
					} else if(a is Ir.List las) {
						compiler.InjectToken("[");
						foreach(var elem in las) {
							compiler.InjectToken(elem);
							compiler.InjectToken(b);
							compiler.InjectToken(op.Key);
						}
						compiler.InjectToken("]");
					} else if(b is Ir.List lbs) {
						compiler.InjectToken("[");
						foreach(var elem in lbs) {
							compiler.InjectToken(a);
							compiler.InjectToken(elem);
							compiler.InjectToken(op.Key);
						}
						compiler.InjectToken("]");
					}
				} else
					compiler.Push(new Ir.BinaryOperation {
						Type = op.Value.Func(Activator.CreateInstance(a.Type), Activator.CreateInstance(b.Type))
							.GetType(),
						Left = a, Right = b,
						Op = op.Value.Op
					});
			}));
		}
	}
}