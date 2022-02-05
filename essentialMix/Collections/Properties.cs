using System;
using System.Collections.Generic;
using essentialMix.Extensions;
using essentialMix.Comparers;
using JetBrains.Annotations;

namespace essentialMix.Collections;

[Serializable]
public class Properties : ObservableKeyedCollectionBase<string, IProperty>, IProperties
{
	public Properties()
		: base(StringComparer.OrdinalIgnoreCase)
	{
	}

	public Properties(IEqualityComparer<string> comparer) : base(comparer)
	{
	}

	public Properties([NotNull] IEnumerable<IProperty> collection) : base(collection)
	{
	}

	public Properties([NotNull] IEnumerable<IProperty> collection, IEqualityComparer<string> comparer) : base(collection, comparer)
	{
	}

	/// <inheritdoc />
	protected override void RemoveItem(int index)
	{
		IProperty item = base[index];
		if (item.IsFixed) return;
		base.RemoveItem(index);
	}

	protected override void ClearItems()
	{
		if (Count == 0) return;

		for (int i = Items.Count - 1; i >= 0; i--)
		{
			if (Items[i].IsFixed) continue;
			RemoveAt(i);
		}
	}

	[NotNull]
	protected override string GetKeyForItem(IProperty item) { return item.Name; }

	public void Reset()
	{
		if (Count == 0) return;

		foreach (IProperty property in Items)
		{
			if (property.IsReadOnly) continue;
			property.Reset();
		}
	}

	public virtual object Clone() { return this.CloneMemberwise(); }

	public virtual int CompareTo(object obj) { return ReferenceComparer.Default.Compare(this, obj); }

	public virtual int CompareTo(IProperties other) { return PropertiesComparer.Default.Compare(this, other); }

	public virtual bool Equals(IProperties other) { return PropertiesComparer.Default.Equals(this, other); }
}