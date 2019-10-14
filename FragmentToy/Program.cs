using System;
using System.IO;
using System.Text;
using System.Web;
using DaeForth;
using Newtonsoft.Json;
using PrettyPrinter;
using Serac;
using Serac.Static;
#pragma warning disable 1998

namespace FragmentToy {
	class Program {
		class CompilationResult {
			public string Errors { get; set; }
			public string Code { get; set; }
		}
		
		static void Main(string[] args) {
			new WebServer()
				.EnableCompression()
				.EnableStaticCache()
				.StaticFile("/", "./static/index.html")
				.RegisterHandler("/", StaticContent.Serve("./static"))
				.RegisterHandler("/code", async _ => new Response { Body = await File.ReadAllTextAsync(args[0]) })
				.RegisterHandler("/compile", async request => {
					var code = HttpUtility.UrlDecode(Encoding.UTF8.GetString(request.Body).Split('=', 2)[1]);
					await File.WriteAllTextAsync(args[0], code);
					string compiled = null;
					await using var ms = new MemoryStream();
					await using(var tw = new StreamWriter(ms, Encoding.UTF8, leaveOpen: true)) {
						try {
							var compiler = new Compiler { ErrorWriter = tw };
							compiler.Compile("fragmenttoy.dfr", code);
							if(compiler.Completed)
								compiled = compiler.GenerateCode(new GlslBackend());
						} catch(Exception e) {
							tw.WriteLine(e);
						}
						await tw.FlushAsync();
					}

					var cr = new CompilationResult {
						Errors = Encoding.UTF8.GetString(ms.ToArray()), Code = compiled
					};
					return new Response {
						Body = JsonConvert.SerializeObject(cr), 
						ContentType = "application/json"
					};
				})
				.ListenOn(8888)
				.RunForever();
		}
	}
}