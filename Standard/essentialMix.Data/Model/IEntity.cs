using System;

namespace essentialMix.Data.Model
{
	public interface IEntity
	{
	}

	public interface IEntity<T> : IEntity
		where T : IComparable<T>, IEquatable<T>
	{
		T Id { get; set; }
	}
}