using essentialMix.Helpers;
using JetBrains.Annotations;
using Scriban.Parsing;

namespace essentialMix.Template;

public class TemplateOptions
{
	private string _baseDirectory = string.Empty;

	[NotNull]
	public string BaseDirectory
	{
		get => _baseDirectory;
		set => _baseDirectory = PathHelper.Trim(value) ?? string.Empty;
	}

	public LexerOptions? LexerOptions { get; set; }
	public ParserOptions? ParserOptions { get; set; }
}