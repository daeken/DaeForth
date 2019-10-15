using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using PrettyPrinter;

namespace DaeForth {
	public class MacroLocals {
		readonly List<Dictionary<string, Ir>> Scopes = new List<Dictionary<string, Ir>> {
			new Dictionary<string, Ir>()
		};
		Dictionary<string, Ir> Values = new Dictionary<string, Ir>();

		public Ir this[string name] {
			get => Values[name];
			set {
				Values[name] = value;
				foreach(var scope in Scopes) {
					if(!scope.ContainsKey(name)) continue;
					scope[name] = value;
					return;
				}
				Scopes.Last()[name] = value;
			}
		}
		
		public void PushScope() => Scopes.Add(new Dictionary<string, Ir>());

		public void PopScope() {
			Scopes.RemoveAt(Scopes.Count - 1);
			Values = Scopes.SelectMany(x => x).ToDictionary(x => x.Key, x => x.Value);
		}

		public bool TryGetValue(string name, out Ir value) => Values.TryGetValue(name, out value);
	}

	public class CompilerException : Exception {
		public CompilerException(string message) : base(message) {}
	}
	
	class UniformType<T> { }
	class VaryingType<T> { }
	class OutputType<T> { }
	class GlobalType<T> { }

	public interface IStack<T> : IEnumerable<T> {
		int Count { get; }
		T Peek();
		T Pop();
		void Push(T value);
	}

	public class Stack<T> : IStack<T> {
		readonly List<T> Values;

		public Stack() => Values = new List<T>();
		public Stack(params T[] values) => Values = values.ToList();
		public Stack(IEnumerable<T> values) => Values = values.ToList();
		
		public int Count => Values.Count;
		public T Peek() => Values[^1];
		public T Pop() {
			var value = Values[^1];
			Values.RemoveAt(Values.Count - 1);
			return value;
		}
		public void Push(T value) => Values.Add(value);
		public IEnumerator<T> GetEnumerator() => Values.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class SurrogateStack : IStack<Ir> {
		readonly Stack<Ir> Underlying;

		public readonly List<Type> Arguments = new List<Type>();
		
		public SurrogateStack(Stack<Ir> underlying) => Underlying = new Stack<Ir>(underlying);

		public IEnumerator<Ir> GetEnumerator() => Underlying.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public int Count => Underlying.Count;
		public Ir Peek() => new Ir.Identifier($"arg_{Arguments.Count}") { Type = Underlying.Peek().Type };

		public Ir Pop() => Transform(Compiler.CanonicalizeValue(Underlying.Pop()));

		Ir Transform(Ir value) {
			Arguments.Add(value.Type);
			return new Ir.Identifier($"arg_{Arguments.Count - 1}") { Type = value.Type };
		}

		public void Push(Ir value) => throw new NotSupportedException();
	}

	public class WordContext {
		public readonly Dictionary<string, Type> Locals = new Dictionary<string, Type>();
		public readonly Stack<List<Ir>> StmtStack = new Stack<List<Ir>>(new List<Ir>());
		public List<Ir> Body => StmtStack.Peek();

		public SurrogateStack SurrogateStack;
	}
	
	public class Compiler {
		public Tokenizer Tokenizer;
		readonly List<Func<Compiler, Token, bool>> StringHandlers = new List<Func<Compiler, Token, bool>>();
		readonly List<Func<Compiler, string, bool>> WordHandlers = new List<Func<Compiler, string, bool>>();

		readonly List<(string Prefix, Func<Compiler, string, Token, bool> Handler)> PrefixHandlers =
			new List<(string Prefix, Func<Compiler, string, Token, bool> Handler)>();

		public Stack<Ir> Stack = new Stack<Ir>();
		public readonly Stack<Stack<Ir>> StackStack = new Stack<Stack<Ir>>();

		public MacroLocals MacroLocals = new MacroLocals();
		
		public readonly Dictionary<string, Ir> Macros = new Dictionary<string, Ir>();
		public readonly Dictionary<string, Ir> Words = new Dictionary<string, Ir>();

		public readonly Dictionary<string, (string Qualifier, Type Type)> Globals =
			new Dictionary<string, (string Qualifier, Type Type)>();
		
		public readonly WordContext MainContext = new WordContext();
		public WordContext CurrentWord;
		public readonly Stack<WordContext> WordStack = new Stack<WordContext>();

		public Dictionary<(string Name, Type Return, Type[] Arguments), WordContext> CompiledWords;

		public bool Completed;
		
		Token CurrentToken;

		public TextWriter ErrorWriter = Console.Error;

		public Compiler() {
			Add(new CommonModule());
			Add(new ShaderModule());
			Add(new BinaryOpModule());
			Add(new UnaryOpModule());
		}
		
		public void Add(DaeforthModule module) {
			StringHandlers.AddRange(module.StringHandlers);
			WordHandlers.AddRange(module.WordHandlers);
			PrefixHandlers.AddRange(module.PrefixHandlers);
		}

		public void Compile(string filename, string source) {
			CompiledWords = new Dictionary<(string Name, Type Return, Type[] Arguments), WordContext> {
				[("main", null, new Type[0])] = MainContext
			};
			
			Tokenizer = new Tokenizer(filename, source);
			Tokenizer.Prefixes.AddRange(PrefixHandlers.Select(x => x.Prefix));

			CurrentWord = MainContext;

			try {
				foreach(var _token in Tokenizer) {
					var token = CurrentToken = _token;
					if(token.Prefixes.Count != 0) {
						var pftoken = token.PopPrefix();
						var pfx = token.Prefixes.First();
						var ph = PrefixHandlers.FirstOrDefault(x => x.Prefix == pfx);
						if(ph.Handler != null && ph.Handler(this, pfx, pftoken))
							continue;
						token = token.BakePrefixes(token.Prefixes);
					}
					
					switch(token.Type) {
						case TokenType.Word: {
							var wordHandled = false;

							if(Macros.TryGetValue(token.Value, out var macroBody)) {
								InjectToken(macroBody);
								InjectToken("call");
								wordHandled = true;
							}
							
							if(!wordHandled && Words.TryGetValue(token.Value, out var wordBody)) {
								InjectToken(wordBody);
								InjectToken(token.Value.Box());
								InjectToken("~~call-word");
								wordHandled = true;
							}
							
							if(!wordHandled && MacroLocals.TryGetValue(token.Value, out var value)) {
								Push(value);
								wordHandled = true;
							}

							if(!wordHandled && Globals.TryGetValue(token.Value, out var gtype)) {
								Push(new Ir.Identifier(token.Value) { Type = gtype.Type });
								wordHandled = true;
							}

							if(!wordHandled && CurrentWord.Locals.TryGetValue(token.Value, out var type)) {
								Push(new Ir.Identifier(token.Value) { Type = type });
								wordHandled = true;
							}

							if(!wordHandled)
								foreach(var wh in WordHandlers) {
									if(!wh(this, token.Value)) continue;
									wordHandled = true;
									break;
								}

							if(!wordHandled)
								throw new CompilerException($"Unhandled word: {token}");
							break;
						}
						case TokenType.Value:
							Push(((ValueToken) token).Value);
							break;
						case TokenType.String: {
							var stringHandled = false;
							foreach(var sh in StringHandlers) {
								if(!sh(this, token)) continue;
								stringHandled = true;
								break;
							}

							if(!stringHandled)
								throw new CompilerException($"Unhandled string: {token}");
							break;
						}
					}
				}
				
				if(MainContext != CurrentWord) throw new CompilerException("Ended in word other than main");
				if(Stack.Count != 0) throw new CompilerException("Main ended with values on the stack");

				Completed = true;
			} catch(CompilerException ce) {
				ErrorWriter.WriteLine(ce);
				ErrorWriter.WriteLine($"Exception in token {CurrentToken}");
				DumpStack();
			}
		}

		public Type TypeFromString(string type) =>
			type switch {
				"int" => typeof(int), 
				"float" => typeof(float), 
				"vec2" => typeof(Vector2), 
				"vec3" => typeof(Vector3), 
				"vec4" => typeof(Vector4), 
				_ => throw new CompilerException($"Unknown type '{type}'")
			};

		public void AddStmt(Ir stmt) => CurrentWord.Body.Add(stmt);

		int TempI;
		public string TempName => $"tmp_{TempI++}";

		public Ir EnsureCheap(Ir value) {
			if(value.IsCheap) return value;

			var name = TempName;
			AssignVariable(name, value);
			return new Ir.Identifier(name) { Type = value.Type };
		}

		public void AssignVariable(string name, Ir value) {
			value = CanonicalizeValue(value);
			var type = value is Ir.IConstValue icv && icv.Value is Type typespec ? typespec : value.Type;
			if(type.IsConstructedGenericType) {
				var gtd = type.GetGenericTypeDefinition();
				string qualifier;
				if(gtd == typeof(UniformType<>)) qualifier = "uniform";
				else if(gtd == typeof(VaryingType<>)) qualifier = "varying";
				else if(gtd == typeof(OutputType<>)) qualifier = "out";
				else if(gtd == typeof(GlobalType<>)) qualifier = null;
				else throw new CompilerException($"Unknown generic type for variable assignment {type}");
				type = type.GetGenericArguments()[0];
				if(Globals.ContainsKey(name)) throw new CompilerException($"Redeclaration of global variable '{name}'");
				Globals[name] = (qualifier, type);
				return;
			}

			if(name.StartsWith("$")) {
				name = name.Substring(1);
			} else {
				if(Globals.TryGetValue(name, out var gknownType)) {
					if(gknownType.Type != type)
						throw new CompilerException(
							$"Global variable '{name}' has type {gknownType.Type.Name} but a {type.Name} ({value}) is being assigned");
				} else if(CurrentWord.Locals.TryGetValue(name, out var knownType)) {
					if(knownType != type)
						throw new CompilerException(
							$"Variable '{name}' has type {knownType.Name} but a {type.Name} ({value}) is being assigned");
				} else
					CurrentWord.Locals[name] = type;
			}

			AddStmt(new Ir.Assignment {
				Lhs = new Ir.Identifier(name) { Type = type }, Type = type, 
				Value = value.Type == type ? value : null
			});
		}

		public static Ir CanonicalizeValue(Ir value) {
			int Size(Ir sval) {
				if(sval is Ir.List slist)
					return slist.Select(Size).Sum();
				var t = sval.Type;
				if(t == typeof(Vector2)) return 2;
				if(t == typeof(Vector3)) return 3;
				if(t == typeof(Vector4)) return 4;
				return 1;
			}
			
			if(!(value is Ir.List list)) return value;
			
			var nlist = new Ir.List(list.Select(CanonicalizeValue));
			switch(Size(nlist)) {
				case 2:
					nlist.Type = typeof(Vector2);
					break;
				case 3:
					nlist.Type = typeof(Vector3);
					break;
				case 4:
					nlist.Type = typeof(Vector4);
					break;
				default:
					throw new CompilerException("Only arrays of size 2-4 can be canonicalized");
			}
			return nlist;
		}

		public void Warn(string message) =>
			ErrorWriter.WriteLine($"Warning near {CurrentToken}: {message}");

		public void DumpStack() {
			ErrorWriter.WriteLine("~Stack~");
			foreach(var elem in Stack)
				ErrorWriter.WriteLine(elem.ToPrettyString());
			ErrorWriter.WriteLine("~End of Stack~");
		}

		public void PushStack() {
			StackStack.Push(Stack);
			Stack = new Stack<Ir>();
		}

		public Stack<Ir> PopStack() {
			var cur = Stack;
			Stack = StackStack.Pop();
			return cur;
		}

		public void PushValue<T>(T value) => Stack.Push(value.Box());
		public void Push(params Ir[] value) {
			foreach(var val in value)
				Stack.Push(val);
		}
		public T Pop<T>() where T : Ir {
			var stack = Stack.Count > 0 || CurrentWord == MainContext ? (IStack<Ir>) Stack : CurrentWord.SurrogateStack;
			if(stack.Count == 0) throw new CompilerException("Stack underflow");
			var val = stack.Pop();
			if(val is T tval) return tval;
			var otype = val.GetType();
			if(otype.IsConstructedGenericType && otype.GetGenericTypeDefinition() == typeof(Ir.ConstValue<>) &&
			   typeof(T).IsConstructedGenericType &&
			   typeof(T).GetGenericTypeDefinition() == typeof(Ir.ConstValue<>) &&
			   typeof(T).GetGenericArguments()[0].IsAssignableFrom(otype.GetGenericArguments()[0]))
				return (T) Activator.CreateInstance(typeof(T), ((Ir.IConstValue) val).Value);
			throw new CompilerException($"Expected {typeof(T).Name} on stack, got {val.GetType().Name}");
		}
		public Ir Pop() => Pop<Ir>();
		public T TryPop<T>() where T : Ir {
			var stack = Stack.Count > 0 || CurrentWord == MainContext ? (IStack<Ir>) Stack : CurrentWord.SurrogateStack;
			if(stack.Count == 0) return null;
			var obj = stack.Peek();
			if(obj is T) return (T) stack.Pop();
			var otype = obj.GetType();
			if(otype.IsConstructedGenericType && otype.GetGenericTypeDefinition() == typeof(Ir.ConstValue<>) &&
			   typeof(T).IsConstructedGenericType &&
			   typeof(T).GetGenericTypeDefinition() == typeof(Ir.ConstValue<>) &&
			   typeof(T).GetGenericArguments()[0].IsAssignableFrom(otype.GetGenericArguments()[0]))
				return (T) Activator.CreateInstance(typeof(T), ((Ir.IConstValue) Stack.Pop()).Value);
			return null;
		}
		
		public (T1, T2) Pop<T1, T2>() where T1 : Ir where T2 : Ir {
			var _2 = Pop<T2>();
			var _1 = Pop<T1>();
			return (_1, _2);
		}
		public (T1, T2, T3) Pop<T1, T2, T3>() where T1 : Ir where T2 : Ir where T3 : Ir {
			var _3 = Pop<T3>();
			var _2 = Pop<T2>();
			var _1 = Pop<T1>();
			return (_1, _2, _3);
		}
		public (T1, T2, T3, T4) Pop<T1, T2, T3, T4>() where T1 : Ir where T2 : Ir where T3 : Ir where T4 : Ir {
			var _4 = Pop<T4>();
			var _3 = Pop<T3>();
			var _2 = Pop<T2>();
			var _1 = Pop<T1>();
			return (_1, _2, _3, _4);
		}

		public void InjectToken(Token token) => Tokenizer.Injected.Enqueue(token);
		public void InjectToken(Ir value) => InjectToken(new ValueToken(value));
		public void InjectToken(string token) => InjectToken(new Token(Location.Generated, Location.Generated, TokenType.Word, null, token));

		public Token ConsumeToken() {
			var enumerator = Tokenizer.GetEnumerator();
			enumerator.MoveNext();
			return enumerator.Current;
		}

		public string GenerateCode(Backend backend) => backend.GenerateCode(Globals, CompiledWords);
	}
}