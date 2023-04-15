using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Scriban;
using Scriban.Runtime;

namespace essentialMix.Template;

public class TemplateService : Disposable, ITemplateService
{
	private static readonly ConcurrentDictionary<string, Scriban.Template> _cacheStore = new ConcurrentDictionary<string, Scriban.Template>(StringComparer.OrdinalIgnoreCase);

	private readonly TemplateOptions _options;

	public TemplateService([NotNull] IOptions<TemplateOptions> options)
	{
		_options = options.Value;
	}

	[NotNull]
	public Scriban.Template RegisterOrGet(string fileName) { return RegisterOrGet(fileName, null, TimeSpan.Zero); }
	[NotNull]
	public Scriban.Template RegisterOrGet(string fileName, TimeSpan cacheTimeout) { return RegisterOrGet(fileName, null, cacheTimeout); }
	[NotNull]
	public Scriban.Template RegisterOrGet(string fileName, Encoding encoding) { return RegisterOrGet(fileName, encoding, TimeSpan.Zero); }
	[NotNull]
	public Scriban.Template RegisterOrGet(string fileName, Encoding encoding, TimeSpan cacheTimeout)
	{
		ThrowIfDisposed();
		return _cacheStore.GetOrAdd(fileName, fn =>
		{
			fn = NormalizeFileName(fn, _options.BaseDirectory);
			string data = File.ReadAllText(fn, encoding ?? Encoding.UTF8);
			if (string.IsNullOrWhiteSpace(data)) throw new IOException("File is empty.");
			Scriban.Template tpl = Scriban.Template.Parse(data, fn, _options.ParserOptions, _options.LexerOptions);
			if (tpl.HasErrors) throw new InvalidOperationException(string.Join(Environment.NewLine, tpl.Messages));
			return tpl;
		});
	}

	public void Unregister(string fileName)
	{
		ThrowIfDisposed();
		fileName = NormalizeFileName(fileName, _options.BaseDirectory);
		_cacheStore.TryRemove(fileName, out _);
	}

	public string Render(string template, TemplateContext context)
	{
		ThrowIfDisposed();
		Scriban.Template tpl = Scriban.Template.Parse(template, null, _options.ParserOptions, _options.LexerOptions);
		return Render(tpl, context);
	}

	public string Render(string template) { return Render(template, null, null, null); }
	public string Render(string template, object model) { return Render(template, model, null, null); }
	public string Render(string template, object model, MemberRenamerDelegate memberRenamer) { return Render(template, model, memberRenamer, null); }
	public string Render(string template, object model, MemberFilterDelegate filter) { return Render(template, model, null, filter); }
	public string Render(string template, object model, MemberRenamerDelegate memberRenamer, MemberFilterDelegate filter)
	{
		ThrowIfDisposed();
		Scriban.Template tpl = Scriban.Template.Parse(template, null, _options.ParserOptions, _options.LexerOptions);
		return Render(tpl, model, memberRenamer, filter);
	}

	public ValueTask<string> RenderAsync(string template, TemplateContext context, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		Scriban.Template tpl = Scriban.Template.Parse(template, null, _options.ParserOptions, _options.LexerOptions);
		return RenderAsync(tpl, context);
	}

	public ValueTask<string> RenderAsync(string template, CancellationToken token = default(CancellationToken)) { return RenderAsync(template, null, null, null, token); }
	public ValueTask<string> RenderAsync(string template, object model, CancellationToken token = default(CancellationToken)) { return RenderAsync(template, model, null, null, token); }
	public ValueTask<string> RenderAsync(string template, object model, MemberRenamerDelegate memberRenamer, CancellationToken token = default(CancellationToken)) { return RenderAsync(template, model, memberRenamer, null, token); }
	public ValueTask<string> RenderAsync(string template, object model, MemberFilterDelegate filter, CancellationToken token = default(CancellationToken)) { return RenderAsync(template, model, null, filter, token); }
	public ValueTask<string> RenderAsync(string template, object model, MemberRenamerDelegate memberRenamer, MemberFilterDelegate filter, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		Scriban.Template tpl = Scriban.Template.Parse(template, null, _options.ParserOptions, _options.LexerOptions);
		return RenderAsync(tpl, model, memberRenamer, filter);
	}

	public string RenderFile(string fileName, TemplateContext context)
	{
		ThrowIfDisposed();
		Scriban.Template tpl = RegisterOrGet(fileName);
		return Render(tpl, context);
	}

	public string RenderFile(string fileName) { return RenderFile(fileName, null, null, null); }
	public string RenderFile(string fileName, object model) { return RenderFile(fileName, model, null, null); }
	public string RenderFile(string fileName, object model, MemberRenamerDelegate memberRenamer) { return RenderFile(fileName, model, memberRenamer, null); }
	public string RenderFile(string fileName, object model, MemberFilterDelegate filter) { return RenderFile(fileName, model, null, filter); }
	public string RenderFile(string fileName, object model, MemberRenamerDelegate memberRenamer, MemberFilterDelegate filter)
	{
		ThrowIfDisposed();
		Scriban.Template tpl = RegisterOrGet(fileName);
		return Render(tpl, model, memberRenamer, filter);
	}

	public async ValueTask<string> RenderFileAsync(string fileName, TemplateContext context, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		Scriban.Template tpl = RegisterOrGet(fileName);
		return await RenderAsync(tpl, context);
	}

	public ValueTask<string> RenderFileAsync(string fileName, CancellationToken token = default(CancellationToken)) { return RenderFileAsync(fileName, null, null, null, token); }
	public ValueTask<string> RenderFileAsync(string fileName, object model, CancellationToken token = default(CancellationToken)) { return RenderFileAsync(fileName, model, null, null, token); }
	public ValueTask<string> RenderFileAsync(string fileName, object model, MemberRenamerDelegate memberRenamer, CancellationToken token = default(CancellationToken)) { return RenderFileAsync(fileName, model, memberRenamer, null, token); }
	public ValueTask<string> RenderFileAsync(string fileName, object model, MemberFilterDelegate filter, CancellationToken token = default(CancellationToken)) { return RenderFileAsync(fileName, model, null, filter, token); }
	public async ValueTask<string> RenderFileAsync(string fileName, object model, MemberRenamerDelegate memberRenamer, MemberFilterDelegate filter, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		Scriban.Template tpl = RegisterOrGet(fileName);
		return await RenderAsync(tpl, model, memberRenamer, filter);
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	private static string Render([NotNull] Scriban.Template tpl, [NotNull] TemplateContext context)
	{
		string result;

		try
		{
			result = tpl.Render(context);
		}
		catch (InvalidOperationException ex)
		{
			if (tpl.HasErrors) throw new InvalidOperationException(string.Join(Environment.NewLine, tpl.Messages), ex);
			throw;
		}

		return result;
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	private static string Render([NotNull] Scriban.Template tpl, object model, MemberRenamerDelegate memberRenamer, MemberFilterDelegate filter)
	{
		string result;

		try
		{
			result = tpl.Render(model, memberRenamer, filter);
		}
		catch (InvalidOperationException ex)
		{
			if (tpl.HasErrors) throw new InvalidOperationException(string.Join(Environment.NewLine, tpl.Messages), ex);
			throw;
		}

		return result;
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	private static ValueTask<string> RenderAsync([NotNull] Scriban.Template tpl, [NotNull] TemplateContext context)
	{
		try
		{
			return tpl.RenderAsync(context);
		}
		catch (InvalidOperationException ex)
		{
			if (tpl.HasErrors) throw new InvalidOperationException(string.Join(Environment.NewLine, tpl.Messages), ex);
			throw;
		}
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	private static ValueTask<string> RenderAsync([NotNull] Scriban.Template tpl, object model, MemberRenamerDelegate memberRenamer, MemberFilterDelegate filter)
	{
		try
		{
			return tpl.RenderAsync(model, memberRenamer, filter);
		}
		catch (InvalidOperationException ex)
		{
			if (tpl.HasErrors) throw new InvalidOperationException(string.Join(Environment.NewLine, tpl.Messages), ex);
			throw;
		}
	}

	[NotNull]
	private static string NormalizeFileName([NotNull] string fileName, string baseDirectory)
	{
		fileName = PathHelper.Trim(fileName);
		if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
		if (!PathHelper.IsPathQualified(fileName) && !string.IsNullOrEmpty(baseDirectory)) fileName = Path.Combine(baseDirectory, fileName);
		return fileName;
	}
}