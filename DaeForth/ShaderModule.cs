using System;
using System.Collections.Generic;

namespace DaeForth {
	public class ShaderModule : DaeforthModule {
		static readonly CommonModule _Module = new CommonModule();
		public override DaeforthModule Module => _Module;

		public ShaderModule() {
			var intrinsics = new Dictionary<string, Func<Compiler, bool>> {
				
			};
			
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
		}
	}
}