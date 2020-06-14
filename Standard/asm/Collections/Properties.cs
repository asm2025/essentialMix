using System;
using System.Collections.Generic;
using asm.Extensions;
using asm.Comparers;
using JetBrains.Annotations;

namespace asm.Collections
{
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
			this.RemoveAll(item => !item.IsFixed);
		}

		[NotNull]
		protected override string GetKeyForItem(IProperty item) { return item.Name; }

		public void Reset()
		{
			if (Count == 0) return;

			foreach (IProperty property in this)
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
}