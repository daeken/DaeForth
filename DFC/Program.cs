using System;
using System.IO;
using DaeForth;

namespace DFC {
	class Program {
		static void Main(string[] args) {
			var compiler = new Compiler();
			compiler.Compile(args[0].Split('/')[^1], File.ReadAllText(args[0]));
			//Console.WriteLine(compiler.GenerateCode(new GlslBackend()));
			Console.WriteLine(compiler.GenerateCode(new CSharpBackend()));
		}
	}
}