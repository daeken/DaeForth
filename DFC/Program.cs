using System;
using System.IO;
using DaeForth;

namespace DFC {
	class Program {
		static void Main(string[] args) {
			var compiler = new Compiler();
			compiler.Compile("test.dfr", File.ReadAllText("test.dfr"));
			Console.WriteLine(compiler.GenerateCode(new GlslBackend()));
		}
	}
}