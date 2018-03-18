using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static System.Console;

namespace DaeForth {
	public class CommonModule : DaeforthModule {
		static readonly CommonModule _Module = new CommonModule();
		public override DaeforthModule Module => _Module;

		public CommonModule() {
			AddPrefixHandler("&", (compiler, token) => {
				compiler.InjectToken("{");
				compiler.InjectToken(token);
				compiler.InjectToken("}");
			});
			
			AddPrefixHandler("/", (compiler, token) => {
				compiler.InjectToken("{");
				compiler.InjectToken(token);
				compiler.InjectToken("}");
				compiler.InjectToken("map");
			});
			
			AddPrefixHandler("\\", (compiler, token) => {
				compiler.InjectToken("{");
				compiler.InjectToken(token);
				compiler.InjectToken("}");
				compiler.InjectToken("reduce");
			});
			
			AddWordHandler("((", compiler => {
				var depth = 0;
				foreach(var elem in compiler.Tokenizer) {
					if(elem.Type == TokenType.String)
						continue;
					if(elem.Value == "((")
						depth++;
					else if(elem.Value == "))" && depth-- == 0)
						break;
				}
			});
			
			AddWordHandler("{", compiler => {
				var btok = new StackBlock();
				var depth = 0;
				foreach(var elem in compiler.Tokenizer) {
					if(elem.Type == TokenType.Word) {
						if(elem.Value == "{") {
							depth++;
							continue;
						}
						if(elem.Value == "}" && depth-- == 0)
							break;
					}
					btok.Add(new StackBoxed<Token>(elem));
				}
				compiler.Push(btok);
			});
			
			// Caller for blocks on the stack
			AddWordHandler("call", compiler => {
				var btok = compiler.TryPop<StackBlock>();
				if(btok == null) return false;
				foreach(var val in btok) {
					if(val is StackBoxed<Token> tok)
						compiler.InjectToken(tok);
					else
						Debug.Fail("Non-token in block");
				}
				return true;
			});
			
			AddWordHandler("swap", compiler => {
				var (a, b) = compiler.Pop<StackValue, StackValue>();
				compiler.Push(b, a);
			});
			
			AddWordHandler("dup", compiler => {
				var v = compiler.Pop();
				compiler.Push(v, v);
			});
			
			AddWordHandler("[", compiler => compiler.PushStack());
			AddWordHandler("]", compiler => compiler.Push(compiler.PopStack()));
		}
	}
}