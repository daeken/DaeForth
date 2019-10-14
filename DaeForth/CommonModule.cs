using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using MoreLinq;
using PrettyPrinter;
using static System.Console;

namespace DaeForth {
	public class CommonModule : DaeforthModule {
		static readonly CommonModule _Module = new CommonModule();
		public override DaeforthModule Module => _Module;

		static readonly Regex IntRegex = new Regex(@"^#(-?[0-9][0-9_]*|0x[0-9a-fA-F][0-9a-fA-F_]*|0b[01][01_]+)$");
		static readonly Regex FloatRegex = new Regex(@"^-?([0-9][0-9_]*\.[0-9_]*|\.[0-9][0-9_]*|[0-9][0-9_]*)$");

		public CommonModule() {
			Ir ParseBlock(Compiler compiler, bool compile = true) {
				var btok = new Ir.List();
				var depth = 0;
				foreach(var elem in compiler.Tokenizer) {
					if(elem.Type == TokenType.Word) {
						if(elem.Value == "{") {
							depth++;
							continue;
						}
						if(elem.Value == "}" && depth-- == 0)
							break;
					}
					btok.Add(elem.Box());
				}
				return compile ? EnsureCompiled(btok) : btok;
			}

			var macroIndex = 0;

			Ir EnsureCompiled(Ir pblock) {
				if(!(pblock is Ir.List ilist)) return pblock;
				
				var index = ++macroIndex;
				var list = ilist.Select(x => ((Ir.ConstValue<Token>) x).Value).ToList();
				var args = new List<(string Name, string Type)>();
				var rets = new List<string>();
				if(list.Count > 2 && list[0].Type == TokenType.Word && list[0].RawValue == "(|") {
					var inArgs = true;
					var i = 1;
					for(; i < list.Count - 1; ++i) {
						var tok = list[i];
						if(tok.RawValue == "|)") break;
						switch(tok.RawValue) {
							case "->":
								if(inArgs) inArgs = false;
								else throw new CompilerException("-> must only appear once in signatures");
								break;
							case { } x when inArgs && x.Contains(':'):
								var parts = x.Split(':');
								if(parts.Length != 2)
									throw new CompilerException(
										": must appear only once in each argument declaration");
								args.Add((parts[0] == "" ? null : parts[0], parts[1] == "" ? null : parts[1]));
								break;
							case { } x when inArgs:
								args.Add((x, null));
								break;
							case { } x when !inArgs:
								rets.Add(x);
								break;
						}
					}

					if(i == list.Count || list[i].RawValue != "|)")
						throw new CompilerException("Missing |) at end of signature");
					if(args.Any(x => x.Name != null) && args.Any(x => x.Name == null))
						throw new CompilerException(
							"All arguments must be either named or unnamed, not a mix of both");
					list = list.Skip(i + 1).ToList();
				}

				if(args.Count == 0 && list.Any(x => x.Type == TokenType.Word && x.Value == "_"))
					args.Add(("_", null));

				var rename = args.Select(x => x.Name).Where(x => x != null).Select(x => (x, $"__macro_{index}_{x}"))
					.ToDictionary();

				if(rename.Count != 0)
					list = list.Select(x => x.Type == TokenType.Word && rename.ContainsKey(x.Value)
						? new Token(x.StartLocation, x.EndLocation, x.Type, x.Prefixes, rename[x.Value], x.RawValue)
						: x).ToList();

				list = args.Select(x => x.Name).Where(x => x != null).Select(x => new Token(Location.Generated,
						Location.Generated, TokenType.Word, new List<string> { "=>" }, rename[x], $"=>{rename[x]}"))
					.Reverse().Concat(list).ToList();
				list = new[] { Token.Generate("~~push-macro-scope") }.Concat(list)
					.Concat(new[] { Token.Generate("~~pop-macro-scope") }).ToList();
				return new Ir.List(list.Select(x => x.Box()));
			}
			
			AddPrefixHandler("`", (compiler, token) => {
				if(token.Value == "{")
					compiler.Push(ParseBlock(compiler, compile: false));
				else
					compiler.InjectToken(token.Box());
			});
			
			AddPrefixHandler("/", (compiler, token) => {
				if(token.Value == "{")
					compiler.Push(ParseBlock(compiler));
				else
					compiler.InjectToken(token.Box());
				compiler.InjectToken("map");
			});
			
			AddPrefixHandler("\\", (compiler, token) => {
				if(token.Value == "{")
					compiler.Push(ParseBlock(compiler));
				else
					compiler.InjectToken(token.Box());
				compiler.InjectToken("reduce");
			});
			
			AddPrefixHandler("~", (compiler, token) => {
				var cond = compiler.TryPop<Ir.ConstValue<bool>>();
				if(cond == null) throw new CompilerException("Conditional compilation requires constant expression");
				var block =
					token.Value == "{"
						? ParseBlock(compiler)
						: token.Box();
				if(!cond.Value) return;
				compiler.InjectToken(block);
				compiler.InjectToken("call");
			});
			
			AddPrefixHandler("=>", (compiler, token) => compiler.MacroLocals[token.Value] = compiler.Pop());
			AddPrefixHandler("=>[", (compiler, token) => throw new NotImplementedException());
			AddPrefixHandler("=[", (compiler, token) => throw new NotImplementedException());
			AddPrefixHandler("=", (compiler, token) => {
				if(token.Value == "=") return false; // ==

				var name = token.Value;
				var value = compiler.Pop();
				compiler.AssignVariable(name, value);
				return true;
			});
			
			AddPrefixHandler("!", (compiler, token) => {
				if(token.Value == "=") return false; // !=
				compiler.InjectToken("dup");
				compiler.InjectToken(token);
				return true;
			});
			
			AddPrefixHandler("*", (compiler, token) => {
				compiler.InjectToken(token);
				compiler.InjectToken("call");
			});

			AddPrefixHandler("@",
				(compiler, token) => compiler.InjectToken(compiler.TypeFromString(token.Value).Box()));
			
			AddWordHandler("uniform", compiler => {
				var type = compiler.TryPop<Ir.ConstValue<Type>>();
				if(type == null) throw new CompilerException("Uniform must be applied to a type");
				compiler.Push(typeof(UniformType<>).MakeGenericType(type).Box());
			});

			AddWordHandler("varying", compiler => {
				var type = compiler.TryPop<Ir.ConstValue<Type>>();
				if(type == null) throw new CompilerException("Varying must be applied to a type");
				compiler.Push(typeof(VaryingType<>).MakeGenericType(type).Box());
			});

			AddWordHandler("output-variable", compiler => {
				var type = compiler.TryPop<Ir.ConstValue<Type>>();
				if(type == null) throw new CompilerException("Output must be applied to a type");
				compiler.Push(typeof(OutputType<>).MakeGenericType(type).Box());
			});

			AddWordHandler("global", compiler => {
				var type = compiler.TryPop<Ir.ConstValue<Type>>();
				if(type == null) throw new CompilerException("Global must be applied to a type");
				compiler.Push(typeof(GlobalType<>).MakeGenericType(type).Box());
			});

			AddWordHandler("((", compiler => {
				var depth = 0;
				foreach(var elem in compiler.Tokenizer) {
					if(elem.Type == TokenType.String)
						continue;
					if(elem.Value == "((")
						depth++;
					else if(elem.Value == "))" && depth-- == 0)
						break;
				}
			});
			
			AddWordHandler("{", compiler => compiler.Push(ParseBlock(compiler)));
			
			AddWordHandler("~~push-macro-scope", compiler => compiler.MacroLocals.PushScope());
			AddWordHandler("~~pop-macro-scope", compiler => compiler.MacroLocals.PopScope());
			
			// Caller for bare blocks on the stack
			AddWordHandler("call", compiler => {
				var btok = compiler.TryPop<Ir.List>();
				if(btok == null) return false;
				compiler.InjectToken("~~push-macro-scope");
				foreach(var val in btok) {
					if(val is Ir.ConstValue<Token> tok)
						compiler.InjectToken(tok.Value);
					else
						Debug.Fail("Non-token in block");
				}
				compiler.InjectToken("~~pop-macro-scope");
				return true;
			});
			
			// Caller for a token on the stack
			AddWordHandler("call", compiler => {
				var tok = compiler.TryPop<Ir.ConstValue<Token>>();
				if(tok == null) return false;
				compiler.InjectToken("~~push-macro-scope");
				compiler.InjectToken(tok.Value);
				compiler.InjectToken("~~pop-macro-scope");
				return true;
			});
			
			AddWordHandler("call-collect", compiler => {
				var block = compiler.Pop();
				compiler.CurrentWord.StmtStack.Push(new List<Ir>());
				compiler.InjectToken(block);
				compiler.InjectToken("call");
				compiler.InjectToken("~~complete-call-collect");
			});

			AddWordHandler("~~complete-call-collect",
				compiler => compiler.Push(new Ir.List(compiler.CurrentWord.StmtStack.Pop())));
			
			AddWordHandler("cif", compiler => {
				var cond = compiler.TryPop<Ir.ConstValue<bool>>();
				if(cond == null) throw new CompilerException("Conditional compilation requires constant expression");
				var else_ = compiler.Pop();
				var if_ = compiler.Pop();
				compiler.InjectToken(cond.Value ? if_ : else_);
				compiler.InjectToken("call");
			});
			
			AddWordHandler("if", compiler => {
				var cond = compiler.Pop();
				var else_ = compiler.Pop();
				var if_ = compiler.Pop();
				compiler.InjectToken(if_);
				compiler.InjectToken("call-collect");
				compiler.InjectToken(else_);
				compiler.InjectToken("call-collect");
				compiler.InjectToken(cond);
				compiler.InjectToken("~~if");
			});
			
			AddWordHandler("~~if", compiler => {
				var cond = compiler.Pop();
				var else_ = compiler.Pop();
				var if_ = compiler.Pop();
				compiler.AddStmt(new Ir.If { Cond = cond, A = if_, B = else_ });
			});

			AddWordHandler("def-macro", compiler => {
				var name = compiler.TryPop<Ir.ConstValue<Token>>()?.Value?.Value ??
				           compiler.TryPop<Ir.ConstValue<string>>()?.Value;
				if(name == null) throw new CompilerException("Macro name must be quoted identifier or string");
				if(compiler.Macros.ContainsKey(name)) compiler.Warn($"Redefining macro '{name}'");
				compiler.Macros[name] = compiler.Pop();
			});
			
			AddWordHandler(":m", compiler => {
				var name = compiler.ConsumeToken().RawValue;
				var elems = new Ir.List();
				foreach(var elem in compiler.Tokenizer) {
					if(elem.Type == TokenType.Word && elem.RawValue == "((") {
						var depth = 0;
						foreach(var selem in compiler.Tokenizer) {
							if(selem.Type == TokenType.String)
								continue;
							if(elem.Value == "((")
								depth++;
							else if(elem.Value == "))" && depth-- == 0)
								break;
						}
					} else if(elem.Type == TokenType.Word && elem.RawValue == ";")
						break;
					else
						elems.Add(elem.Box());
				}
				
				if(compiler.Macros.ContainsKey(name)) compiler.Warn($"Redefining macro '{name}'");
				compiler.Macros[name] = EnsureCompiled(elems);
			});
			
			AddWordHandler("swap", compiler => {
				var (a, b) = compiler.Pop<Ir, Ir>();
				compiler.Push(b, a);
			});
			
			AddWordHandler("dup", compiler => {
				var v = compiler.Pop();
				if(v.IsConstant)
					compiler.Push(v, v);
				else
					throw new NotImplementedException(); // TODO: Implement assignments
			});
			
			AddWordHandler("drop", compiler => compiler.Pop());
			
			AddWordHandler("[", compiler => compiler.PushStack());
			AddWordHandler("]", compiler => compiler.Push(compiler.PopStack().Reverse().ToList()));
			
			AddWordHandler("map", compiler => {
				var block = compiler.Pop();
				var list = compiler.TryPop<Ir.List>();
				if(list == null) { compiler.Push(block); return false; }

				if(list.Count <= 1) {
					compiler.Push(list);
					return true;
				}
				
				compiler.InjectToken("[");
				foreach(var elem in list) {
					compiler.InjectToken(elem);
					compiler.InjectToken(block);
					compiler.InjectToken("call");
				}
				compiler.InjectToken("]");
				
				return true;
			});
			
			AddWordHandler("reduce", compiler => {
				var block = compiler.Pop();
				var list = compiler.TryPop<Ir.List>();
				if(list == null) { compiler.Push(block); return false; }

				if(list.Count == 0) throw new CompilerException("Reduce on empty list");
				if(list.Count == 1) {
					compiler.Push(list[0]);
					return true;
				}
				
				compiler.InjectToken(list[0]);
				for(var i = 1; i < list.Count; ++i) {
					compiler.InjectToken(list[i]);
					compiler.InjectToken(block);
					compiler.InjectToken("call");
				}
				
				return true;
			});
			
			AddWordHandler("flatten", compiler => {
				var list = compiler.TryPop<Ir.List>();
				if(list == null) return false;
				
				compiler.InjectToken("[");
				foreach(var elem in list)
					if(elem is Ir.List sublist)
						foreach(var selem in sublist)
							compiler.InjectToken(selem);
					else
						compiler.InjectToken(elem);
				compiler.InjectToken("]");
				
				return true;
			});
			
			AddWordHandler("flatten-deep", compiler => {
				var list = compiler.TryPop<Ir.List>();
				if(list == null) return false;

				void Flatten(Ir.List slist) {
					foreach(var elem in slist)
						if(elem is Ir.List sublist)
							Flatten(sublist);
						else
							compiler.InjectToken(elem);
				}
				
				compiler.InjectToken("[");
				Flatten(list);
				compiler.InjectToken("]");

				return true;
			});
			
			AddWordHandler("mtimes", compiler => {
				var count = compiler.TryPop<Ir.ConstValue<int>>();
				if(count == null) throw new CompilerException("Count must be a constant integer");
				if(count.Value < 0) throw new CompilerException("Count must be positive");
				var block = compiler.Pop();
				for(var i = 0; i < count; ++i) {
					compiler.InjectToken(i.Box());
					compiler.InjectToken(block);
					compiler.InjectToken("call");
				}
			});
			
			AddWordHandler((compiler, token) => {
				if(IntRegex.IsMatch(token)) {
					token = token.Substring(1).Replace("_", "");
					if(token.StartsWith("0x"))
						compiler.PushValue(Convert.ToInt32(token.Substring(2), 16));
					else if(token.StartsWith("0b"))
						compiler.PushValue(Convert.ToInt32(token.Substring(2), 2));
					else
						compiler.PushValue(int.Parse(token));
				} else if(FloatRegex.IsMatch(token))
					compiler.PushValue(float.Parse(token.Replace("_", "")));
				else
					return false;
				return true;
			});
			
			AddWordHandler("false", compiler => compiler.PushValue(false));
			AddWordHandler("true", compiler => compiler.PushValue(true));
			
			AddWordHandler("print", compiler => compiler.ErrorWriter.WriteLine(compiler.Pop().ToPrettyString()));
			AddWordHandler("assert", compiler => {
				var cond = compiler.TryPop<Ir.ConstValue<bool>>();
				if(cond == null) throw new CompilerException("Assertion on non-const or non-bool value");
				if(!cond.Value) throw new CompilerException("Assertion failed");
			});
		}
	}
}