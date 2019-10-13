using System.Collections.Generic;

namespace DaeForth {
	public abstract class Backend {
		public abstract string GenerateCode(IEnumerable<WordContext> words);
	}
}