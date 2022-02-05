using System;
using Newtonsoft.Json;

namespace essentialMix.Newtonsoft.Converters;

public class NumberConverter : JsonConverter
{
	/// <inheritdoc />
	public NumberConverter()
	{
	}

	/// <inheritdoc />
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(string);
	}

	/// <inheritdoc />
	public override bool CanRead => false;

	/// <inheritdoc />
	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, global::Newtonsoft.Json.JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public override void WriteJson(JsonWriter writer, object value, global::Newtonsoft.Json.JsonSerializer jsonSerializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}

		string s = value.ToString();

		if (s.Length == 0)
		{
			writer.WriteNull();
			return;
		}

		bool allDigits = true;
		bool hasDot = false;
		int i = 0;
		if (s[i] == '-') i++;

		for (;allDigits && i < s.Length; i++)
		{
			char c = s[i];
			if (char.IsDigit(c)) continue;

			if (c == '.' && !hasDot)
			{
				hasDot = true;
				continue;
			}

			allDigits = false;
		}

		if (allDigits)
		{
			if (long.TryParse(s, out long l))
				writer.WriteValue(l);
			else if (ulong.TryParse(s, out ulong ul))
				writer.WriteValue(ul);
			else if (double.TryParse(s, out double d))
				writer.WriteValue(d);
			else if (decimal.TryParse(s, out decimal dc))
				writer.WriteValue(dc);
		}
		else
		{
			writer.WriteValue(s);
		}
	}
}