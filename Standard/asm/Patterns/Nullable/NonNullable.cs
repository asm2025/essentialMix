using System;
using JetBrains.Annotations;

namespace asm.Patterns.Nullable
{
	/// <summary>
	/// Encapsulates a reference compatible with the type parameter. The reference
	/// is guaranteed to be non-null unless the value has been created with the
	/// parameterless constructor (e.g. as the default value of a field or array).
	/// Implicit conversions are available to and from the type parameter. The
	/// conversion to the non-nullable type will throw ArgumentNullException
	/// when presented with a null reference. The conversion from the non-nullable
	/// type will throw NullReferenceException if it contains a null reference.
	/// This type is a value type (to avoid taking any extra space) and as the CLR
	/// unfortunately has no knowledge of it, it will be boxed as any other value
	/// type. The conversions are also available through the Value property and the
	/// parameterized constructor.
	/// </summary>
	/// <typeparam name="T">Type of non-nullable reference to encapsulate</typeparam>
	public struct NonNullable<T> : IEquatable<NonNullable<T>>
		where T : class
	{
		private readonly T _value;

		/// <summary>
		/// Creates a non-nullable value encapsulating the specified reference.
		/// </summary>
		public NonNullable([NotNull] T value)
		{
			_value = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <summary>
		/// Implicit conversion from the specified reference.
		/// </summary>
		public static explicit operator NonNullable<T>([NotNull] T value) { return new NonNullable<T>(value); }

		/// <summary>
		/// Implicit conversion to the type parameter from the encapsulated value.
		/// </summary>
		[NotNull]
		public static implicit operator T(NonNullable<T> value) { return value.Value; }

		/// <summary>
		/// Equality operator, which performs an identity comparison on the encapsulated
		/// references. No exception is thrown even if the references are null.
		/// </summary>
		public static bool operator ==(NonNullable<T> x, NonNullable<T> y) { return x._value == y._value; }

		/// <summary>
		/// Inequality operator, which performs an identity comparison on the encapsulated
		/// references. No exception is thrown even if the references are null.
		/// </summary>
		public static bool operator !=(NonNullable<T> x, NonNullable<T> y) { return x._value != y._value; }

		/// <summary>
		/// Equality is deferred to encapsulated references, but there is no equality
		/// between a NonNullable[T] and a T. This method never throws an exception,
		/// even if a null reference is encapsulated.
		/// </summary>
		public override bool Equals(object obj) { return obj is NonNullable<T> nullable && Equals(nullable); }

		/// <summary>
		/// Defers to the GetHashCode implementation of the encapsulated reference, or 0 if
		/// the reference == null.
		/// </summary>
		public override int GetHashCode()
		{
			return _value == null
						? 0
						: _value.GetHashCode();
		}

		/// <summary>
		/// Defers to the ToString implementation of the encapsulated reference, or an
		/// empty string if the reference == null.
		/// </summary>
		[NotNull]
		public override string ToString() { return _value?.ToString() ?? string.Empty; }

		/// <summary>
		/// Retrieves the encapsulated value, or throws a NullReferenceException if
		/// this instance was created with the parameterless constructor or by default.
		/// </summary>
		[NotNull]
		public T Value
		{
			get
			{
				if (_value == null)
				{
					throw new NullReferenceException();
				}
				return _value;
			}
		}

		/// <inheritdoc />
		/// <summary>
		/// Type-safe (and non-boxing) equality check.
		/// </summary>
		public bool Equals(NonNullable<T> other) { return Equals(_value, other._value); }

		/// <summary>
		/// Type-safe (and non-boxing) static equality check.
		/// </summary>
		public static bool Equals(NonNullable<T> first, NonNullable<T> second) { return Equals(first._value, second._value); }
	}
}