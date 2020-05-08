using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using JetBrains.Annotations;
using asm.Extensions;
using asm.Collections;

namespace asm.Helpers
{
	public static class TypeHelper
	{
		[NotNull]
		internal static IReadOnlySet<Type> NumericTypes =>
			new ReadOnlySet<Type>(new HashSet<Type>
			{
				typeof(bool),
				typeof(char),
				typeof(sbyte),
				typeof(byte),
				typeof(short),
				typeof(ushort),
				typeof(int),
				typeof(uint),
				typeof(long),
				typeof(ulong),
				typeof(BigInteger),
				typeof(float),
				typeof(double),
				typeof(decimal)
			});

		[NotNull]
		internal static IReadOnlySet<Type> IntegralTypes =>
			new ReadOnlySet<Type>(new HashSet<Type>
			{
				typeof(bool),
				typeof(char),
				typeof(sbyte),
				typeof(byte),
				typeof(short),
				typeof(ushort),
				typeof(int),
				typeof(uint),
				typeof(long),
				typeof(ulong),
				typeof(BigInteger)
			});

		[NotNull]
		internal static IReadOnlySet<Type> FloatingTypes =>
			new ReadOnlySet<Type>(new HashSet<Type>
			{
				typeof(float),
				typeof(double),
				typeof(decimal)
			});

		private static readonly ConcurrentDictionary<Type, (object Minimum, object Maximum)> __typeBoundsCache =
			new ConcurrentDictionary<Type, (object Minimum, object Maximum)>
			{
				{ typeof(bool), (false, true) },
				{ typeof(char), (char.MinValue, char.MaxValue) },
				{ typeof(sbyte), (sbyte.MinValue, sbyte.MaxValue) },
				{ typeof(byte), (byte.MinValue, byte.MaxValue) },
				{ typeof(short), (short.MinValue, short.MaxValue) },
				{ typeof(ushort), (ushort.MinValue, ushort.MaxValue) },
				{ typeof(int), (int.MinValue, int.MaxValue) },
				{ typeof(uint), (uint.MinValue, uint.MaxValue) },
				{ typeof(long), (long.MinValue, long.MaxValue) },
				{ typeof(ulong), (ulong.MinValue, ulong.MaxValue) },
				{ typeof(float), (float.MinValue, float.MaxValue) },
				{ typeof(double), (double.MinValue, double.MaxValue) },
				{ typeof(decimal), (decimal.MinValue, decimal.MaxValue) },
			};

		public static (T Miniumum, T Maximum) Bounds<T>()
			where T : struct, IComparable
		{
			const string FIELD_MIN = "MinValue";
			const string FIELD_MAX = "MaxValue";
			const BindingFlags BOUNDS_FLAGS = Constants.BF_PUBLIC_STATIC;

			return ((T Miniumum, T Maximum))__typeBoundsCache.GetOrAdd(typeof(T), type =>
			{
				if (type.IsEnum)
				{
					EnumHelper.GetBoundaries(type, out Enum minimum, out Enum maximum);
					(T Miniumum, T Maximum) bounds = ((T)(object)minimum, (T)(object)maximum);
					__typeBoundsCache.Add(type, bounds);
					return bounds;
				}

				FieldInfo minField = type.GetField(FIELD_MIN, BOUNDS_FLAGS, type) ?? throw new NotImplementedException($"{FIELD_MIN} is not implemented.");
				FieldInfo maxField = type.GetField(FIELD_MAX, BOUNDS_FLAGS, type) ?? throw new NotImplementedException($"{FIELD_MAX} is not implemented.");
				(T, T) typeBounds = ((T)minField.GetValue(null), (T)maxField.GetValue(null));
				return typeBounds;
			});
		}

		public static T MinimumOf<T>()
			where T : struct, IComparable
		{
			return Bounds<T>().Miniumum;
		}

		public static T MaximumOf<T>()
			where T : struct, IComparable
		{
			return Bounds<T>().Maximum;
		}

		public static Type FromName(string name, string assemblyName = null)
		{
			name = name?.Trim();
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

			Type type = Type.GetType(name, false);
			if (type != null) return type;

			foreach (Assembly assembly in AssemblyHelper.GetAssemblies())
			{
				type = assembly.GetType(name, false);
				if (type != null) break;
			}

			if (type != null) return type;
			assemblyName = assemblyName?.Trim();
			return !string.IsNullOrEmpty(assemblyName) && AssemblyHelper.TryLoad(assemblyName, out Assembly asm) && asm != null && (type = asm.GetType(name, false)) != null
						? type
						: null;
		}

		public static T CreateInstance<T>(params object[] arguments) { return (T)CreateInstance(typeof(T), arguments); }
		public static object CreateInstance([NotNull] Type type, params object[] arguments)
		{
			return type.CreateInstance(arguments);
		}
	}
}