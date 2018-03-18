using System;
using System.Collections.Generic;

namespace DaeForth {
	public abstract class DaeforthModule {
		public abstract DaeforthModule Module { get; }

		public readonly List<Func<Compiler, Token, bool>> StringHandlers = new List<Func<Compiler, Token, bool>>();
		public readonly List<Func<Compiler, string, bool>> WordHandlers = new List<Func<Compiler, string, bool>>();
		public readonly Dictionary<string, Func<Compiler, string, Token, bool>> PrefixHandlers = new Dictionary<string, Func<Compiler, string, Token, bool>>();

		protected void AddStringHandler(Func<Compiler, Token, bool> func) =>
			StringHandlers.Add(func);

		protected void AddWordHandler(Func<Compiler, string, bool> func) =>
			WordHandlers.Add(func);

		protected void AddWordHandler(string word, Func<Compiler, bool> func) =>
			WordHandlers.Add((compiler, token) => token == word && func(compiler));

		protected void AddWordHandler(string word, Action<Compiler> func) =>
			WordHandlers.Add((compiler, token) => {
				if(token != word) return false;
				func(compiler);
				return true;
			});

		protected void AddPrefixHandler(string pfx, Func<Compiler, Token, bool> func) =>
			PrefixHandlers[pfx] = (compiler, ppfx, token) => func(compiler, token);

		protected void AddPrefixHandler(string pfx, Action<Compiler, Token> func) =>
			PrefixHandlers[pfx] = (compiler, ppfx, token) => {
				func(compiler, token);
				return true;
			};
	}
}