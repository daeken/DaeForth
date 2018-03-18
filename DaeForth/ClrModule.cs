using System;
using System.Text.RegularExpressions;
using static System.Console;

namespace DaeForth {
	public class ClrModule : DaeforthModule {
		static readonly ClrModule _Module = new ClrModule();
		public override DaeforthModule Module => _Module;
		
		static readonly Regex IntRegex = new Regex(@"^([0-9][0-9_]*|0x[0-9a-fA-F][0-9a-fA-F_]*|0b[01][01_]+)$");
		static readonly Regex FloatRegex = new Regex(@"^([0-9]+\.[0-9]*|\.[0-9]+)$");

		public ClrModule() {
			AddWordHandler((compiler, token) => {
				if(IntRegex.IsMatch(token)) {
					token = token.Replace("_", "");
					if(token.StartsWith("0x"))
						compiler.PushValue(Convert.ToInt32(token.Substring(2), 16));
					else if(token.StartsWith("0b"))
						compiler.PushValue(Convert.ToInt32(token.Substring(2), 2));
					else
						compiler.PushValue(int.Parse(token));
				} else if(FloatRegex.IsMatch(token))
					compiler.PushValue(double.Parse(token));
				else
					return false;

				return true;
			});
		}
	}
}