using System;
using System.Collections.Generic;
using MoreLinq;

namespace DaeForth {
	public class UnaryOpModule : DaeforthModule {
		static readonly UnaryOpModule _Module = new UnaryOpModule();
		public override DaeforthModule Module => _Module;

		public UnaryOpModule() {
			var ops = new Dictionary<string, (UnaryOp Op, Func<dynamic, object> Func)> {
				["not"] = (UnaryOp.LogicalNegate, v => !v), 
				["neg"] = (UnaryOp.Minus, v => -v), 
			};
			
			ops.ForEach(op => AddWordHandler(op.Key, compiler => {
				var v = compiler.Pop<Ir>();
				if(v is Ir.IConstValue icv)
					compiler.PushValue(op.Value.Func(icv.Value));
				else if(v.IsConstant && v is Ir.List list) {
					compiler.InjectToken("[");
					foreach(var elem in list) {
						compiler.InjectToken(elem);
						compiler.InjectToken(op.Key);
					}
					compiler.InjectToken("]");
				} else
					compiler.Push(new Ir.UnaryOperation {
						Type = op.Value.Func(Activator.CreateInstance(Compiler.CanonicalizeValue(v).Type)).GetType(),
						Value = v,
						Op = op.Value.Op
					});
			}));
		}
	}
}