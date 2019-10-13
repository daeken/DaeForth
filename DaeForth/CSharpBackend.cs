using System;
using System.Collections.Generic;

namespace DaeForth {
	public class CSharpBackend : Backend {
		public override string GenerateCode(IDictionary<string, (string Qualifier, Type Type)> globals,
			IEnumerable<WordContext> words) {
			throw new NotImplementedException();
		}
	}
}