using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace asm.Reflection
{
	/// <summary>
	/// Generic class which copies to its target type from a source
	/// type specified in the Copy method. The types are specified
	/// separately to take advantage of type inference on generic
	/// method arguments.
	/// </summary>
	public static class PropertyCopy<TTarget> 
		where TTarget : class, new()
	{
		/// <summary>
		/// Static class to efficiently store the compiled delegate which can
		/// do the copying. We need a bit of work to ensure that exceptions are
		/// appropriately propagated, as the exception is generated at type initialization
		/// time, but we wish it to be thrown as an ArgumentException.
		/// </summary>
		private static class PropertyCopier<TSource> 
			where TSource : class
		{
			private static readonly Func<TSource, TTarget> __copier;

			static PropertyCopier()
			{
				__copier = BuildCopier();
			}

			internal static TTarget Copy([NotNull] TSource source)
			{
				if (source == null) throw new ArgumentNullException(nameof(source));
				return __copier(source);
			}

			[NotNull]
			private static Func<TSource, TTarget> BuildCopier()
			{
				ParameterExpression sourceParameter = Expression.Parameter(typeof(TSource), "source");
				List<MemberBinding> bindings = new List<MemberBinding>();

				foreach (PropertyInfo sourceProperty in typeof(TSource).GetProperties())
				{
					if (!sourceProperty.CanRead) continue;
					PropertyInfo targetProperty = typeof(TTarget).GetProperty(sourceProperty.Name);
					if (targetProperty == null)
						throw new ArgumentException($"Property {sourceProperty.Name} is not present and accessible in {typeof(TTarget).FullName}");
					if (!targetProperty.CanWrite) throw new ArgumentException($"Property {sourceProperty.Name} is not writable in {typeof(TTarget).FullName}");
					if (!targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
						throw new ArgumentException($"Property {sourceProperty.Name} has an incompatible type in {typeof(TTarget).FullName}");
					bindings.Add(Expression.Bind(targetProperty, Expression.Property(sourceParameter, sourceProperty)));
				}

				Expression initializer = Expression.MemberInit(Expression.New(typeof(TTarget)), bindings);
				return Expression.Lambda<Func<TSource, TTarget>>(initializer, sourceParameter).Compile();
			}
		}

		/// <summary>
		/// Copies all readable properties from the source to a new instance
		/// of TTarget.
		/// </summary>
		public static TTarget CopyFrom<TSource>([NotNull] TSource source) where TSource : class { return PropertyCopier<TSource>.Copy(source); }
	}
}