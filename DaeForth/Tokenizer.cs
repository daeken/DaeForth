using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PrettyPrinter;
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
		List<string> _Prefixes;
		public List<string> Prefixes => _Prefixes ??= FindPrefixes();
		public string Value, RawValue;

		public Token(Location start, Location end, TokenType type, List<string> prefixes, string value, string rawValue = null) {
			StartLocation = start;
			EndLocation = end;
			Type = type;
			_Prefixes = prefixes;
			Value = value;
			RawValue = rawValue ?? value;
		}

		List<string> FindPrefixes() {
			var prefixes = new List<string>();
			var i = 0;
			if(RawValue == null) return prefixes;
			var allPrefixes = Compiler.Instance.PrefixHandlers.Select(x => x.Prefix)
				.Concat(Compiler.Instance.Prefixes.Keys).OrderByDescending(x => x.Length)
				.ToList();
			while(i < RawValue.Length) {
				var found = false;
				foreach(var pfx in allPrefixes) {
					if(i + pfx.Length >= RawValue.Length || RawValue.Substring(i, pfx.Length) != pfx) continue;
					prefixes.Add(pfx);
					found = true;
					i += pfx.Length;
					break;
				}
				if(!found)
					break;
			}

			if(prefixes.Count > 0 && i == RawValue.Length) {
				i -= prefixes.Last().Length;
				prefixes = prefixes.Take(prefixes.Count - 1).ToList();
			}

			if(i != 0) Value = RawValue.Substring(i);

			return prefixes;
		}

		public Token PopPrefix() =>
			new Token(StartLocation, EndLocation, Type, Prefixes.Skip(1).ToList(), Value,
				RawValue.Substring(Prefixes[0].Length));

		public Token PrependPrefix(string prefix) =>
			new Token(StartLocation, EndLocation, Type, new[] { prefix }.Concat(Prefixes).ToList(), Value,
				prefix + RawValue);
		
		public Token BakePrefixes(List<string> prefixes) =>
			new Token(StartLocation, EndLocation, Type, new List<string>(), string.Join("", prefixes) + Value, RawValue);
		
		public static Token Generate(string token) =>
			new Token(Location.Generated, Location.Generated, TokenType.Word, null, token, token);

		public override string ToString() => $"'{Value}' == '{RawValue}'{(Prefixes.Count != 0 ? $" [ {string.Join(" ", Prefixes)} ]" : "")} @ {StartLocation} - {EndLocation}";

		public override bool Equals(object obj) {
			if(obj is Token otoken) {
				WriteLine($"Token equality... ? '{RawValue}' '{otoken.RawValue}'");
				return otoken.RawValue == RawValue;
			}

			return false;
		}

		public static bool operator ==(Token a, Token b) => a.RawValue == b.RawValue;
		public static bool operator !=(Token a, Token b) => a.RawValue != b.RawValue;

		public override int GetHashCode() => RawValue.GetHashCode();
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

				var start = GetLocation(i);

				if(Code[i] == '"') {
					throw new NotImplementedException();
				} else {
					var tv = "";
					while(i < Code.Length && !IsWhitespace(Code[i]))
						tv += Code[i++];
					yield return new Token(start, GetLocation(i - 1), TokenType.Word, null, tv, tv);
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