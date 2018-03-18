using DaeForth;
using static System.Console;

namespace DFC {
	class Program {
		static void Main(string[] args) {
			var compiler = new Compiler();
			compiler.Add(new CommonModule());
			compiler.Add(new ClrModule());
			compiler.Compile("test.dfr", "5 (( some comment here (( and some (( nesting! )) )) )) [ 7 8 9 [ 1 2 3 swap ] swap ] 6 &swap dup call");
		}
	}
}