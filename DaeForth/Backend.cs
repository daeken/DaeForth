using System;
using System.Collections.Generic;

namespace DaeForth {
	public abstract class Backend {
		public abstract string GenerateCode(IDictionary<string, (string Qualifier, Type Type)> globals,
			Dictionary<(string Name, Type Return, Type[] Arguments), WordContext> words);
	}
}