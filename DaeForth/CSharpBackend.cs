using System;
using System.Collections.Generic;

namespace DaeForth {
	public class CSharpBackend : Backend {
		public override string GenerateCode(IDictionary<string, (string Qualifier, Type Type)> globals,
			Dictionary<(string Name, Type Return, Type[] Arguments), WordContext> words) {
			throw new NotImplementedException();
		}
	}
}