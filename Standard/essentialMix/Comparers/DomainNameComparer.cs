using System;
using System.Collections.Generic;
using System.Threading;
using essentialMix.Extensions;
using Other.Nager.PublicSuffix;

namespace essentialMix.Comparers;

// https://github.com/nager/Nager.PublicSuffix
// see ./Nager/IMPORTANT.txt for notes about this code
public sealed class DomainNameComparer : GenericComparer<string>
{
	public new static DomainNameComparer Default { get; } = new DomainNameComparer();

	private static readonly Lazy<DomainParser> __parser = new Lazy<DomainParser>(() =>
	{
		FileCacheProvider cacheProvider = new FileCacheProvider();
		WebTldRuleProvider tldRuleProvider = new WebTldRuleProvider(cacheProvider: cacheProvider);

		if (!tldRuleProvider.CacheProvider.IsCacheValid())
		{
			try
			{
				tldRuleProvider.BuildAsync().Execute();
			}
			catch
			{
				return null;
			}
		}
		return new DomainParser(tldRuleProvider);
	}, LazyThreadSafetyMode.PublicationOnly);

	/// <inheritdoc />
	public DomainNameComparer()
		: this(null, null)
	{
	}

	/// <inheritdoc />
	public DomainNameComparer(IComparer<string> comparer)
		: this(comparer, null)
	{
	}

	/// <inheritdoc />
	public DomainNameComparer(IComparer<string> comparer, IEqualityComparer<string> equalityComparer)
		: base(comparer ?? StringComparer.OrdinalIgnoreCase, equalityComparer ?? StringComparer.OrdinalIgnoreCase)
	{

	}

	public override int Compare(string x, string y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (ReferenceEquals(x, null)) return 1;
		if (ReferenceEquals(y, null)) return -1;
		DomainName xd = __parser.Value.Get(x);
		DomainName yd = __parser.Value.Get(y);
		return string.Compare(xd.Hostname, yd.Hostname, StringComparison.OrdinalIgnoreCase);
	}

	public override bool Equals(string x, string y) { return Compare(x, y) == 0; }
}