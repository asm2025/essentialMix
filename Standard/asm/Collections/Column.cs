using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	[DebuggerDisplay("{Name}")]
	public abstract class Column : Header, IComparable<Column>, IEquatable<Column>
	{
		private ushort _order;

		protected Column(string name, Type type)
			: this(name, type, null) { }

		protected Column(string name, Type type, string text)
			: base(name, type, text) { }

		protected Column(string name, Type type, bool isFixed) : base(name, type, isFixed)
		{
		}

		protected Column([NotNull] string name, [NotNull] Type type, string text, bool isFixed) : base(name, type, text, isFixed)
		{
		}

		protected Column([NotNull] SerializationInfo info, StreamingContext context)
			: base(info, context) { _order = info.GetUInt16("Order"); }

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Order", _order);
		}

		public ushort Order
		{
			get => _order;
			set
			{
				if (_order == value) return;
				_order = value;
				OnPropertyChanged();
			}
		}

		public virtual int CompareTo(Column other) { return ColumnComparer.Default.Compare(this, other); }

		public virtual bool Equals(Column other) { return ColumnComparer.Default.Equals(this, other); }
	}
}