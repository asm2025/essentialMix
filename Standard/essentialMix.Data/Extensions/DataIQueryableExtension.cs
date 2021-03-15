using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using essentialMix.Patterns.Sorting;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class DataIQueryableExtension
	{
		private static readonly ConcurrentDictionary<string, MethodInfo> __methods = new ConcurrentDictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase);

		public static IQueryable<T> OrderBy<T>([NotNull] this IQueryable<T> thisValue, string name, SortType sortType)
		{
			string methodName;

			switch (sortType)
			{
				case SortType.Ascending:
					methodName = nameof(Queryable.OrderBy);
					break;
				case SortType.Descending:
					methodName = nameof(Queryable.OrderByDescending);
					break;
				default:
					return thisValue;
			}

			return OrderBy(thisValue, name, methodName);
		}

		public static IQueryable<T> ThenBy<T>([NotNull] this IQueryable<T> thisValue, string name, SortType sortType)
		{
			string methodName;

			switch (sortType)
			{
				case SortType.Ascending:
					methodName = nameof(Queryable.ThenBy);
					break;
				case SortType.Descending:
					methodName = nameof(Queryable.ThenByDescending);
					break;
				default:
					return thisValue;
			}

			return OrderBy(thisValue, name, methodName);
		}

		private static IQueryable<T> OrderBy<T>(IQueryable<T> thisValue, [NotNull] string memberName, [NotNull] string methodName)
		{
			memberName = memberName.Trim('.', ' ');
			if (string.IsNullOrEmpty(memberName)) throw new ArgumentNullException(nameof(memberName));
			
			Type type = typeof(T);
			ParameterExpression arg = Expression.Parameter(type, $"{type.Name}Expr");
			Expression expr = arg;
			string[] names = memberName.Split(StringSplitOptions.RemoveEmptyEntries, '.');

			foreach (string name in names) 
			{
				// use reflection (not ComponentModel) to mirror LINQ
				MemberInfo mi = type.GetPropertyOrField(name, Constants.BF_PUBLIC_INSTANCE);
				if (mi == null) throw new MemberAccessException($"Could not find property or field '{name}'.");
				expr = Expression.PropertyOrField(expr, mi.Name);

				switch (mi.MemberType)
				{
					case MemberTypes.Property:
						type = ((PropertyInfo)mi).PropertyType;
						break;
					case MemberTypes.Field:
						type = ((FieldInfo)mi).FieldType;
						break;
					default:
						throw new NotSupportedException();
				}
			}

			Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
			LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);
			MethodInfo orderBy = __methods.GetOrAdd(methodName, mn => typeof(Queryable).GetMethods()
																						.Single(method => method.Name.IsSame(mn) &&
																										method.IsGenericMethodDefinition &&
																										method.GetGenericArguments().Length == 2 &&
																										method.GetParameters().Length == 2));

			object result = orderBy.MakeGenericMethod(typeof(T), type)
									.Invoke(null, new object[] {thisValue, lambda});
			return (IOrderedQueryable<T>)result;		}
	}
}