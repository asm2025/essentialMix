using System.Runtime.CompilerServices;
using Microsoft.Win32;

namespace essentialMix.Extensions
{
	public static class RegistryKeyExtension
	{
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Get<T>(this RegistryKey thisValue, string name) { return Get(thisValue, name, default(T), true); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Get<T>(this RegistryKey thisValue, string name, T defaultValue) { return Get(thisValue, name, defaultValue, true); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Get<T>(this RegistryKey thisValue, string name, T defaultValue, bool expandVariables)
		{
			return thisValue == null
						? defaultValue
						: thisValue.GetValue(name, defaultValue, expandVariables
																	? RegistryValueOptions.DoNotExpandEnvironmentNames
																	: RegistryValueOptions.None)
									.To(defaultValue);
		}
	}
}