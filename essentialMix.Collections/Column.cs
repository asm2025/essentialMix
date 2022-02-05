using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace essentialMix.Collections;

[Serializable]
[DebuggerDisplay("{Name}")]
public abstract class Column : Header, IComparable<Column>, IEquatable<Column>
{
	private ushort _order;

	protected Column([NotNull] string name, [NotNull] Type type)
		: this(name, type, null) { }

	protected Column([NotNull] string name, [NotNull] Type type, string text)
		: base(name, type, text) { }

	protected Column([NotNull] string name, [NotNull] Type type, bool isFixed) : base(name, type, isFixed)
	{
	}

	protected Column([NotNull] string name, [NotNull] Type type, string text, bool isFixed) : base(name, type, text, isFixed)
	{
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