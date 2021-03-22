using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using essentialMix.Extensions;
using JetBrains.Annotations;
using essentialMix.Helpers;

namespace essentialMix
{
	public static class AppInfo
	{
		private static readonly Type __baseAttributeType = typeof(Attribute);
		private static readonly ConcurrentDictionary<Type, Func<Type, Assembly, string>> __registeredAttributes = new ConcurrentDictionary<Type, Func<Type, Assembly, string>>();
		private static readonly ConcurrentDictionary<Type, string> __attributes = new ConcurrentDictionary<Type, string>();
		private static readonly Assembly __asm = AssemblyHelper.GetEntryAssembly();

		static AppInfo()
		{
			ExecutablePath = __asm.GetPath();
			ExecutableName = Path.GetFileNameWithoutExtension(ExecutablePath);
			Directory = PathHelper.AddDirectorySeparator(Path.GetDirectoryName(ExecutablePath));
			AppGuid = __asm.GetCode();
			Title = __asm.GetTitle();
			Description = __asm.GetDescription();
			ProductName = __asm.GetProductName();
			Company = __asm.GetCompany();
			Copyright = __asm.GetCopyright();
			Trademark = __asm.GetTrademark();
			Version = __asm.GetVersion();
			FileVersion = __asm.GetFileVersion();
			Culture = __asm.GetCulture();
		}

		[NotNull]
		public static string ExecutablePath { get; }

		[NotNull]
		public static string ExecutableName { get; }

		[NotNull]
		public static string Directory { get; }

		[NotNull]
		public static string AppGuid { get; }

		[NotNull]
		public static string Title { get; }
		
		[NotNull]
		public static string Description { get; }
		
		[NotNull]
		public static string ProductName { get; }

		[NotNull]
		public static string Company { get; }

		[NotNull]
		public static string Copyright { get; }

		[NotNull]
		public static string Trademark { get; }

		[NotNull]
		public static string Version { get; }

		[NotNull]
		public static string FileVersion { get; }

		[NotNull]
		public static string Culture { get; }

		[NotNull]
		public static string GetAttribute<T>()
			where T : Attribute
		{
			return GetAttribute(typeof(T));
		}

		[NotNull]
		public static string GetAttribute([NotNull] Type type)
		{
			if (__attributes.TryGetValue(type, out string value)) return value;
			if (!__registeredAttributes.TryGetValue(type, out Func<Type, Assembly, string> factory)) throw new InvalidOperationException("Attribute is not registered.");
			value = __attributes.GetOrAdd(type, t => factory(t, __asm) ?? string.Empty);
			return value;
		}

		public static bool RegisterAttribute<T>([NotNull] Func<Type, Assembly, string> factory)
			where T : Attribute
		{
			return RegisterAttribute(typeof(T), factory);
		}

		public static bool RegisterAttribute([NotNull] Type type, [NotNull] Func<Type, Assembly, string> factory)
		{
			if (!__baseAttributeType.IsAssignableFrom(type)) throw new InvalidCastException($"{type.FullName} is not a type of {__baseAttributeType.FullName}.");
			return __registeredAttributes.TryAdd(type, factory);
		}

		public static bool DeregisterAttribute<T>(out Func<Type, Assembly, string> factory)
			where T : Attribute
		{
			return DeregisterAttribute(typeof(T), out factory);
		}

		public static bool DeregisterAttribute([NotNull] Type type, out Func<Type, Assembly, string> factory)
		{
			if (!__baseAttributeType.IsAssignableFrom(type)) throw new InvalidCastException($"{type.FullName} is not a type of {__baseAttributeType.FullName}.");
			return __registeredAttributes.TryRemove(type, out factory);
		}

		public static void ClearAttributes()
		{
			__registeredAttributes.Clear();
		}
	}
}