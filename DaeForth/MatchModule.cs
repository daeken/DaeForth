using System.Linq;
using PrettyPrinter;

namespace DaeForth {
	public class MatchModule : DaeforthModule {
		static readonly MatchModule _Module = new MatchModule();
		public override DaeforthModule Module => _Module;

		public MatchModule() {
			AddWordHandler("()", compiler => compiler.Push(Unit.Value));
			
			// TODO: COMPLETE
			AddWordHandler("match", async compiler => {
				var value = compiler.Pop();
				var patterns = compiler.TryPop<Ir.List>();
				if(patterns == null) throw new CompilerException("Match requires a list of patterns");

				var vtype = value.Type;
				int size;
				if(value is Ir.List vlist)
					size = vlist.Count;
				else if(vtype == typeof(Vec2)) size = 2;
				else if(vtype == typeof(Vec3)) size = 3;
				else if(vtype == typeof(Vec4)) size = 4;
				else size = -1;

				var runtime = false;
				Ir currentBlock = null;

				foreach(var (pattern, handler) in patterns.Where((_, i) => i % 2 == 0)
					.Zip(patterns.Where((_, i) => i % 2 == 1)).Concat(patterns.Count % 2 == 0
						? new (Ir, Ir)[0]
						: new[] { ((Ir) Unit.Value, patterns.Last()) }).Reverse()) {
					if(!(pattern is Ir.List plist)) plist = new Ir.List(new[] { pattern });

					if(plist.Count == 0) continue; // Empty list is always a miss

					Ir.List constraint = null;
					if(plist.Last() is Ir.List pblock && pblock[0] is Ir.ConstValue<Token> pbtok &&
					   pbtok.Value.Type == TokenType.Word && pbtok.Value.RawValue == "when")
						constraint = new Ir.List(pblock.Skip(1));
					
					if(constraint != null) plist = new Ir.List(plist.SkipLast(1));
					if(plist.Count == 0) plist = new Ir.List(new[] { Unit.Value });

					var matched = false;

					foreach(var option in plist)
						switch(option) {
							case Ir.IConstValue icv when icv.Value.GetType().IsAssignableFrom(vtype):
								if(runtime || !value.IsConstant) {
									runtime = true;
									compiler.InjectToken(handler);
									compiler.InjectToken("call-collect");
									await compiler.RunToHere();
									var block = compiler.Pop();
									currentBlock = new Ir.If {
										Cond = new Ir.BinaryOperation {
											Op = BinaryOp.Equal, 
											Left = Compiler.CanonicalizeValue(option), 
											Right = Compiler.CanonicalizeValue(value)
										}, 
										A = block, 
										B = currentBlock
									};
								}
								break;
						}

					if(matched) break;
				}
			});
		}
	}
}