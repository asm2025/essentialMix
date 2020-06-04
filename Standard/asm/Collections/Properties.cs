using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using asm.Extensions;
using asm.Comparers;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class Properties : ObservableKeyedCollection<string, IProperty>, IProperties
	{
		[NonSerialized] 
		private SerializationInfo _siInfo;

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

		protected Properties(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			_siInfo = info;
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			foreach (IProperty property in this)
				info.AddValue(property.Name, property.Value, property.ValueType);
		}

		public override void OnDeserialization(object sender)
		{
			base.OnDeserialization(sender);
			if (_siInfo == null) return;

			foreach (string key in Keys)
			{
				IProperty property = this[key];
				property.Value = _siInfo.GetValue(property.Name, property.ValueType);
			}

			_siInfo = null;
		}

		public override bool Remove(IProperty item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));
			return !item.IsFixed && base.Remove(item);
		}

		public override bool Remove(string key)
		{
			IProperty item = base[key];
			if (item == null || item.IsFixed) return false;
			return base.Remove(key);
		}

		public override void RemoveAt(int index)
		{
			IProperty item = base[index];
			if (item.IsFixed) return;
			base.RemoveAt(index);
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