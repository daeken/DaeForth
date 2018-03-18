using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DaeForth {
	public abstract class StackValue {
	}

	public class StackBoxed<T> : StackValue where T : struct {
		public readonly T Value;
		public StackBoxed(T value) => Value = value;

		public static implicit operator T(StackBoxed<T> box) => box.Value;
		public static implicit operator StackBoxed<T>(T value) => new StackBoxed<T>(value);

		public override string ToString() => Value.ToString();
	}
	
	public class StackList : StackValue, IEnumerable<StackValue> {
		public List<StackValue> Value = new List<StackValue>();
		public int Count => Value.Count;

		public void Add(StackValue value) => Value.Add(value);

		public IEnumerator<StackValue> GetEnumerator() => Value.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => Value.GetEnumerator();

		public void Push(StackValue value) => Value.Add(value);
		public StackValue Peek() => Value.Last();
		public StackValue Pop() {
			var top = Value.Last();
			Value = Value.SkipLast(1).ToList();
			return top;
		}

		public override string ToString() => $"[ {string.Join(" ", Value.Select(x => x.ToString()))} ]";
	}

	public class StackBlock : StackList {
		public override string ToString() => $"{{ {string.Join(" ", Value.Select(x => x.ToString()))} }}";
	}
	
	public class Compiler {
		public Tokenizer Tokenizer;
		readonly List<Func<Compiler, Token, bool>> StringHandlers = new List<Func<Compiler, Token, bool>>();
		readonly List<Func<Compiler, string, bool>> WordHandlers = new List<Func<Compiler, string, bool>>();
		readonly Dictionary<string, Func<Compiler, string, Token, bool>> PrefixHandlers = new Dictionary<string, Func<Compiler, string, Token, bool>>();

		public StackList Stack = new StackList();
		public readonly Stack<StackList> StackStack = new Stack<StackList>();
		
		public Compiler() {
			
		}

		public void Add(DaeforthModule module) {
			StringHandlers.AddRange(module.StringHandlers);
			WordHandlers.AddRange(module.WordHandlers);
			foreach(var (k, v) in module.PrefixHandlers)
				PrefixHandlers[k] = v;
		}

		public void Compile(string filename, string source) {
			Tokenizer = new Tokenizer(filename, source);
			Tokenizer.Prefixes.AddRange(PrefixHandlers.Keys);

			foreach(var token in Tokenizer) {
				var pfxHandled = false;
				if(token.Prefixes.Count != 0) {
					var ptoken = token.PopPrefix();
					foreach(var pfx in token.Prefixes) {
						if(!PrefixHandlers.ContainsKey(pfx) || !PrefixHandlers[pfx](this, pfx, ptoken)) continue;
						pfxHandled = true;
						break;
					}
					if(pfxHandled)
						continue;
					Debug.Assert(token.Prefixes.Count == 0); // We shouldn't have any unhandled prefixes here.
				}

				if(token.Type == TokenType.Word) {
					var wordHandled = false;
					foreach(var wh in WordHandlers) {
						if(!wh(this, token.Value)) continue;
						wordHandled = true;
						break;
					}
					if(!wordHandled)
						throw new Exception($"Unhandled word: {token}");
				} else {
					var stringHandled = false;
					foreach(var sh in StringHandlers) {
						if(!sh(this, token)) continue;
						stringHandled = true;
						break;
					}
					if(!stringHandled)
						throw new Exception($"Unhandled string: {token}");
				}
			}
			
			DumpStack();
		}

		public void DumpStack() {
			Console.WriteLine($"~Stack~");
			foreach(var elem in Stack)
				Console.WriteLine(elem);
		}

		public void PushStack() {
			StackStack.Push(Stack);
			Stack = new StackList();
		}

		public StackList PopStack() {
			var cur = Stack;
			Stack = StackStack.Pop();
			return cur;
		}

		public void PushValue<T>(T value) where T : struct => Stack.Push((StackBoxed<T>) value);
		public void Push(params StackValue[] value) {
			foreach(var val in value)
				Stack.Push(val);
		}
		public T Pop<T>() where T : StackValue {
			Debug.Assert(Stack.Count != 0);
			var val = Stack.Pop();
			Debug.Assert(val is T);
			return (T) val;
		}
		public StackValue Pop() => Pop<StackValue>();
		public T TryPop<T>() where T : StackValue {
			if(Stack.Count != 0 && Stack.Peek() is T) return (T) Stack.Pop();
			return null;
		}
		
		public (T1, T2) Pop<T1, T2>() where T1 : StackValue where T2 : StackValue {
			var _2 = Pop<T2>();
			var _1 = Pop<T1>();
			return (_1, _2);
		}
		public (T1, T2, T3) Pop<T1, T2, T3>() where T1 : StackValue where T2 : StackValue where T3 : StackValue {
			var _3 = Pop<T3>();
			var _2 = Pop<T2>();
			var _1 = Pop<T1>();
			return (_1, _2, _3);
		}
		public (T1, T2, T3, T4) Pop<T1, T2, T3, T4>() where T1 : StackValue where T2 : StackValue where T3 : StackValue where T4 : StackValue {
			var _4 = Pop<T4>();
			var _3 = Pop<T3>();
			var _2 = Pop<T2>();
			var _1 = Pop<T1>();
			return (_1, _2, _3, _4);
		}

		public void InjectToken(Token token) => Tokenizer.Injected.Enqueue(token);
		public void InjectToken(string token) => InjectToken(new Token(Location.Generated, Location.Generated, TokenType.Word, null, token));

		public void Output(Stream ostream) {
			
		}
	}
}