using System;
using System.Collections.Generic;
using essentialMix.Patterns.Sorting;

namespace essentialMix.Web;

[Serializable]
public class RequestParameters<T>
	where T : struct, IComparable
{
	private string _culture = SupportedCultures.DefaultCulture;

	public RequestParameters() 
	{
	}

	public T Id { get; set; }
	public int? Page { get; set; } = 1;
	public int? PageSize { get; set; } = 10;

	public string Culture
	{
		get => _culture;
		set
		{
			if (!SupportedCultures.IsSupportedCulture(value)) return;
			_culture = value;
		}
	}

	public string Filter { get; set; }

	public IList<SortField> Sort { get; set; }
}