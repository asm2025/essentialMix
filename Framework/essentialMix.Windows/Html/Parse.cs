using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Windows.Html
{
	/// <summary>
	/// Main class for parsing HTML text
	/// </summary>
	/// <remarks>
	/// The code in written quit �dirty� because the speed is the most important thing.
	/// This is mostly seen in code duplication. In classical parser one can use a function
	/// like IsSpace which would return true if next character is a space. I have found
	/// out that such a function is a time killer in .NET 2.0 CF environment so instead
	/// of such a function I use big if statement.
	/// Variable _source_with_guards holds the Source text with added CHR(0) at the end of
	/// text. For example �This is HTML� becomes �This is HTML000�, where 0 means CHR(0).
	/// With this I can read beyond the end of text without the need for checking the
	/// length of text.
	/// Variable _source_len holds the text length so no repeated calls for Length
	/// function are made.
	/// idx is current index
	/// Class has only one public (static) function (ParseAll) which parse the whole HTML
	/// text into  a list of elements (parts)
	/// </remarks>
	public class Parse
	{
		private string _source;
		private string _sourceWithGuards;
		private int _sourceLen;
		private int _idx;

		protected string Source
		{
			get => _source;
			set
			{
				_source = value;
				_sourceWithGuards = _source + (char)0 + (char)0 + (char)0;
				_sourceLen = _source.Length;
			}
		}

		private void EatWhiteSpace()
		{
			while (_idx < _sourceLen)
			{
				char ch = _sourceWithGuards[_idx];
				if (ch != ' ' && ch != '\t' && ch != '\n' && ch != '\r') return; // optimization, do not use isWhiteSpace
				++_idx;
			}
		}

		[NotNull]
		private string ParseAttributeName()
		{
			EatWhiteSpace();

			// get attribute name
			int start = _idx;
			while (_idx < _sourceLen)
			{
				char ch = _sourceWithGuards[_idx];
				if (ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r' || ch == '=' || ch == '>') // optimization, do not use isWhiteSpace
					break;

				_idx++;
			}

			string name = _sourceWithGuards.Substring(start, _idx - start);
			EatWhiteSpace();
			return name;
		}

		[NotNull]
		private string ParseAttributeValue()
		{
			if (_sourceWithGuards[_idx] != '=') return string.Empty;

			_idx++;
			EatWhiteSpace();

			string value;
			char ch = _sourceWithGuards[_idx];

			if (ch == '\'' || ch == '\"')
			{
				char valueDelimiter = ch;
				_idx++;
				int start = _idx;
				while (_sourceWithGuards[_idx] != valueDelimiter) ++_idx;
				value = _sourceWithGuards.Substring(start, _idx - start);
				_idx++;
			}
			else
			{
				int start = _idx;
				while (_idx < _sourceLen && ch != ' ' && ch != '\t' && ch != '\n' && ch != '\r' && _sourceWithGuards[_idx] != '>') ++_idx;

				value = _sourceWithGuards.Substring(start, _idx - start);
			}
			EatWhiteSpace();

			return value;
		}

		[NotNull]
		private Part ParseTag()
		{
			// Is it a comment or process instruction
			if (_sourceWithGuards[_idx] == '!')
			{
				if (_sourceWithGuards[_idx + 1] == '-' && _sourceWithGuards[_idx + 2] == '-')
				{
					_idx += 3;
					int start = _idx;
					while (_idx < _sourceLen)
					{
						if (_sourceWithGuards[_idx] == '-' &&
							_sourceWithGuards[_idx + 1] == '-' &&
							_sourceWithGuards[_idx + 2] == '>') break;
						++_idx;
					}
					string value = _sourceWithGuards.Substring(start, _idx - start);
					if (_idx < _sourceLen) _idx += 3;
					return new Comment(value);
				}
				else
				{
					_idx += 1;
					int start = _idx;
					while (_idx < _sourceLen && _sourceWithGuards[_idx] != '>') ++_idx;

					int end = _idx;
					if (_idx < _sourceLen)
					{
						end = _idx - 1;
						++_idx;
					}
					return new ProcessInstruction(_sourceWithGuards.Substring(start, end - start));
				}
			}

			// Find the tag name
			int start1 = _idx;
			while (_idx < _sourceLen)
			{
				char ch = _sourceWithGuards[_idx];
				if (ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r' || ch == '>') // optimization, do not use isWhiteSpace
					break;
				++_idx;
			}
			string name = _sourceWithGuards.Substring(start1, _idx - start1);

			EatWhiteSpace();

			// Get the attributes
			AttributeList attrList = new AttributeList();
			while (_sourceWithGuards[_idx] != '>')
			{
				string parseName = ParseAttributeName();

				if (_sourceWithGuards[_idx] == '>')
				{
					attrList.Add(new Attribute(parseName, string.Empty));
					break;
				}

				string parseValue = ParseAttributeValue();
				attrList.Add(new Attribute(parseName, parseValue));
			}
			++_idx;

			return new Tag(name, attrList);
		}

		private bool IsEof() { return _idx >= _sourceLen; }

		[NotNull]
		private Part ParseNext()
		{
			int start = _idx;

			if (_sourceWithGuards[_idx] == '<')
			{
				char ch = char.ToUpper(_sourceWithGuards[_idx + 1]);
				if (ch is >= 'A' and <= 'Z' || ch == '!' || ch == '/')
				{
					++_idx;
					return ParseTag();
				}

				++_idx;
			}

			while (_idx < _sourceLen)
			{
				if (_sourceWithGuards[_idx] == '<') break;
				_idx++;
			}

			string value = _sourceWithGuards.Substring(start, _idx - start);
			return new Text(value);
		}

		[NotNull]
		public static List<Part> ParseAll(string htmlString)
		{
			List<Part> retValue = new List<Part>();
			Parse parse = new Parse {Source = htmlString};
			while (!parse.IsEof()) retValue.Add(parse.ParseNext());

			return retValue;
		}
	}
}