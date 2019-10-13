using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static System.Console;

namespace DaeForth {
	public struct Location {
		public string Filename;
		public int Line, Column, Offset;

		public override string ToString() => $"{Filename}:{Line+1}:{Column+1}";

		public static readonly Location Generated = new Location { Filename = "~generated~", Line = -2, Column = -2, Offset = -1 };
	}

	public enum TokenType {
		String, 
		Word, 
		Value
	}
	
	public class Token {
		public readonly Location StartLocation, EndLocation;
		public readonly TokenType Type;
		public readonly List<string> Prefixes;
		public readonly string Value, RawValue;

		public Token(Location start, Location end, TokenType type, List<string> prefixes, string value, string rawValue = null) {
			StartLocation = start;
			EndLocation = end;
			Type = type;
			Prefixes = prefixes ?? new List<string>();
			Value = value;
			RawValue = rawValue ?? value;
		}

		public Token PopPrefix() =>
			new Token(StartLocation, EndLocation, Type, Prefixes.Skip(1).ToList(), Value, RawValue);
		
		public Token BakePrefixes(List<string> prefixes) =>
			new Token(StartLocation, EndLocation, Type, null, string.Join("", prefixes) + Value, RawValue);
		
		public static Token Generate(string token) =>
			new Token(Location.Generated, Location.Generated, TokenType.Word, null, token, token);

		public override string ToString() => $"'{RawValue}'{(Prefixes.Count != 0 ? $" [ {string.Join(" ", Prefixes)} ]" : "")} @ {StartLocation} - {EndLocation}";
	}

	public class ValueToken : Token {
		public new readonly Ir Value;

		public ValueToken(Ir value) : base(Location.Generated, Location.Generated, TokenType.Value, null, null) =>
			Value = value;

		public override string ToString() => $"ValueToken {Value}";
	}

	public class Tokenizer : IEnumerable<Token> {
		readonly string Filename, Code;
		
		readonly List<(int Offset, int Line)> LineStarts;
		public readonly List<string> Prefixes = new List<string>();

		public Queue<Token> Injected = new Queue<Token>();
		public readonly Stack<Queue<Token>> AllInjected = new Stack<Queue<Token>>();

		readonly IEnumerator<Token> Enumerator;
		
		public Tokenizer(string filename, string code) {
			Filename = filename;
			Code = code;
			
			LineStarts = new List<(int Offset, int Line)> { (0, 0) };
			var line = 1;
			for(var i = 0; i < code.Length; ++i)
				if(code[i] == '\n')
					LineStarts.Add((i + 1, line++));
			LineStarts.Reverse();

			Enumerator = GetCoreEnumerator();
		}

		Location GetLocation(int offset) {
			var (lo, ln) = LineStarts.First(x => x.Offset <= offset);
			return new Location { Filename=Filename, Line=ln, Column=offset-lo, Offset=offset };
		}
		
		IEnumerator<Token> GetCoreEnumerator() {
			bool IsWhitespace(char ch) =>
				ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r';

			void HandleInjected() {
				if(Injected.Count == 0) return;
				AllInjected.Push(Injected);
				Injected = new Queue<Token>();
			}
			
			var i = 0;
			var ps = 0;
			while(i < Code.Length) {
				while(true) {
					if(AllInjected.Count == 0) break;
					var iq = AllInjected.Peek();
					if(iq.Count == 0) {
						AllInjected.Pop();
						continue;
					}
					yield return iq.Dequeue();
					HandleInjected();
				}

				if(IsWhitespace(Code[i])) {
					i++;
					continue;
				}

				if(ps != Prefixes.Count) {
					Prefixes.Sort((a, b) => b.Length.CompareTo(a.Length));
					ps = Prefixes.Count;
				}

				var start = GetLocation(i);

				var prefixes = new List<string>();
				while(i < Code.Length) {
					var found = false;
					foreach(var pfx in Prefixes) {
						if(i + pfx.Length >= Code.Length || Code.Substring(i, pfx.Length) != pfx) continue;
						prefixes.Add(pfx);
						found = true;
						i += pfx.Length;
					}
					if(!found)
						break;
				}

				if(prefixes.Count > 0 && (i == Code.Length || IsWhitespace(Code[i]))) {
					i -= prefixes.Last().Length;
					prefixes = prefixes.Take(prefixes.Count - 1).ToList();
				}

				if(Code[i] == '"') {
					throw new NotImplementedException();
				} else {
					var tv = "";
					while(i < Code.Length && !IsWhitespace(Code[i]))
						tv += Code[i++];
					yield return new Token(start, GetLocation(i - 1), TokenType.Word, prefixes, tv, string.Join("", prefixes) + tv);
				}
				
				HandleInjected();
			}
			
			while(true) {
				if(AllInjected.Count == 0) break;
				var iq = AllInjected.Peek();
				if(iq.Count == 0) {
					AllInjected.Pop();
					continue;
				}
				yield return iq.Dequeue();
				HandleInjected();
			}
		}

		public IEnumerator<Token> GetEnumerator() => Enumerator;
		IEnumerator IEnumerable.GetEnumerator() => Enumerator;
	}
}