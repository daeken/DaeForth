using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DaeForth;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Files;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Swan.Logging;

#pragma warning disable 1998

namespace FragmentToy {
	class Program {
		class CompilationResult {
			public string Errors { get; set; }
			public string Code { get; set; }
		}

		static string Filename;

		class CodeController : WebApiController {
			[Route(HttpVerbs.Get, "/code")]
			public async Task<CompilationResult> GetCode() => new CompilationResult { Code = await File.ReadAllTextAsync(Filename) };

			[Route(HttpVerbs.Post, "/compile")]
			public async Task<CompilationResult> PostCompile([FormData] NameValueCollection nvc) {
				var code = nvc["code"];
				await File.WriteAllTextAsync(Filename, code);
				string compiled = null;
				await using var ms = new MemoryStream();
				await using(var tw = new StreamWriter(ms, Encoding.UTF8, leaveOpen: true)) {
					try {
						var compiler = new Compiler { ErrorWriter = tw };
						compiler.Compile(Filename.Split('/').Last(), code);
						if(compiler.Completed)
							compiled = compiler.GenerateCode(new GlslBackend());
					} catch(Exception e) {
						tw.WriteLine(e);
					}
					await tw.FlushAsync();
				}

				return new CompilationResult {
					Errors = Encoding.UTF8.GetString(ms.ToArray()), Code = compiled
				};
			}
		}
		
		static async Task Main(string[] args) {
			Filename = args[0];
			var ws = new WebServer(o => o.WithUrlPrefix("http://*:8888/").WithMode(HttpListenerMode.EmbedIO))
				.WithWebApi("/api", m => m.WithController<CodeController>())
				.WithStaticFolder("/", "static", true, o => o.WithoutContentCaching())
				.WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Error" })))
				;
			ws.StateChanged += (s, e) => $"WebServer New State - {e.NewState}".Info();
			await ws.RunAsync();
		}
	}
}