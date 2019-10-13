using System.IO;
using DaeForth;
using static System.Console;

namespace DFC {
	class Program {
		static void Main(string[] args) {
			var compiler = new Compiler();
			compiler.Add(new CommonModule());
			compiler.Add(new ShaderModule());
			compiler.Add(new BinaryOpModule());
			compiler.Compile("test.dfr", File.ReadAllText("test.dfr"));
		}
	}
}