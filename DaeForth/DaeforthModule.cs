using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#pragma warning disable 1998

namespace DaeForth {
	public abstract class DaeforthModule {
		public abstract DaeforthModule Module { get; }

		public readonly List<Func<Compiler, Token, bool>> StringHandlers = new List<Func<Compiler, Token, bool>>();
		public readonly List<Func<Compiler, string, bool>> WordHandlers = new List<Func<Compiler, string, bool>>();
		public readonly List<(string Prefix, Func<Compiler, string, Token, bool> Handler)> PrefixHandlers =
			new List<(string Prefix, Func<Compiler, string, Token, bool> Handler)>();

		protected void AddStringHandler(Func<Compiler, Token, bool> func) =>
			StringHandlers.Add(func);

		protected void AddWordHandler(Func<Compiler, string, bool> func) =>
			WordHandlers.Add(func);

		protected void AddWordHandler(string word, Func<Compiler, bool> func) =>
			WordHandlers.Add((compiler, token) => token == word && func(compiler));

		protected void AddWordHandler(string word, Func<Compiler, Task> func) =>
			AddWordHandler(word, (Action<Compiler>) (async compiler => {
				try {
					await func(compiler);
				} catch(Exception e) {
					compiler.Bailout(e);
				}
			}));

		protected void AddWordHandler(string word, Action<Compiler> func) =>
			WordHandlers.Add((compiler, token) => {
				if(token != word) return false;
				func(compiler);
				return true;
			});

		protected void AddPrefixHandler(string pfx, Func<Compiler, Token, bool> func) =>
			PrefixHandlers.Add((pfx, (compiler, ppfx, token) => func(compiler, token)));

		protected void AddPrefixHandler(string pfx, Action<Compiler, Token> func) =>
			PrefixHandlers.Add((pfx, (compiler, ppfx, token) => {
				func(compiler, token);
				return true;
			}));
	}
}