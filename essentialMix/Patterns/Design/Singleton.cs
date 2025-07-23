using System;
using System.Reflection;
using System.Threading;

namespace essentialMix.Patterns.Design;

public sealed class Singleton<T> where T : class
{
	private const string CTOR_ERR = "The singleton must implement exactly one private parameterless ctor.";

	private static readonly Lazy<T> __instance = new Lazy<T>(() =>
	{
		Type type = typeof(T);
		ConstructorInfo[] ctors = type.GetConstructors(Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE);
		if (ctors.Length is 0 or > 1) throw new ArgumentException(CTOR_ERR, type.FullName);

		ConstructorInfo ctor = ctors[0];
		if (ctor.GetParameters().Length > 0) throw new ArgumentException(CTOR_ERR, type.FullName);
		return (T)ctor.Invoke([]);
	}, LazyThreadSafetyMode.ExecutionAndPublication);

	public static T Instance => __instance.Value;
}