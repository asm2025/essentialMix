using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using essentialMix.Extensions;
using JetBrains.Annotations;
using essentialMix.Helpers;

namespace essentialMix;

public class AppInfo
{
	private static readonly Type __baseAttributeType = typeof(Attribute);

	private readonly ConcurrentDictionary<Type, Func<Type, Assembly, string>> _registeredAttributes = new ConcurrentDictionary<Type, Func<Type, Assembly, string>>();
	private readonly ConcurrentDictionary<Type, string> _attributes = new ConcurrentDictionary<Type, string>();
	private readonly Assembly _asm;

	public AppInfo(Assembly assembly)
	{
		_asm = assembly;

		if (_asm == null)
		{
			ExecutablePath = string.Empty;
			ExecutableName = string.Empty;
			Directory = string.Empty;
			AppGuid = string.Empty;
			Title = string.Empty;
			Description = string.Empty;
			ProductName = string.Empty;
			Company = string.Empty;
			Copyright = string.Empty;
			Trademark = string.Empty;
			Version = string.Empty;
			FileVersion = string.Empty;
			Culture = string.Empty;
			return;
		}

		ExecutablePath = _asm.GetPath();
		ExecutableName = Path.GetFileNameWithoutExtension(ExecutablePath);
		Directory = PathHelper.AddDirectorySeparator(Path.GetDirectoryName(ExecutablePath));
		AppGuid = _asm.GetCode();
		Title = _asm.GetTitle();
		Description = _asm.GetDescription();
		ProductName = _asm.GetProductName();
		Company = _asm.GetCompany();
		Copyright = _asm.GetCopyright();
		Trademark = _asm.GetTrademark();
		Version = _asm.GetVersion();
		FileVersion = _asm.GetFileVersion();
		Culture = _asm.GetCulture();
	}

	[NotNull]
	public string ExecutablePath { get; }

	[NotNull]
	public string ExecutableName { get; }

	[NotNull]
	public string Directory { get; }

	[NotNull]
	public string AppGuid { get; }

	[NotNull]
	public string Title { get; }
		
	[NotNull]
	public string Description { get; }
		
	[NotNull]
	public string ProductName { get; }

	[NotNull]
	public string Company { get; }

	[NotNull]
	public string Copyright { get; }

	[NotNull]
	public string Trademark { get; }

	[NotNull]
	public string Version { get; }

	[NotNull]
	public string FileVersion { get; }

	[NotNull]
	public string Culture { get; }

	[NotNull]
	public string GetAttribute<T>()
		where T : Attribute
	{
		return GetAttribute(typeof(T));
	}

	[NotNull]
	public string GetAttribute([NotNull] Type type)
	{
		if (_attributes.TryGetValue(type, out string value)) return value;
		if (!_registeredAttributes.TryGetValue(type, out Func<Type, Assembly, string> factory)) throw new InvalidOperationException("Attribute is not registered.");
		value = _attributes.GetOrAdd(type, t => factory(t, _asm) ?? string.Empty);
		return value;
	}

	public bool RegisterAttribute<T>([NotNull] Func<Type, Assembly, string> factory)
		where T : Attribute
	{
		return RegisterAttribute(typeof(T), factory);
	}

	public bool RegisterAttribute([NotNull] Type type, [NotNull] Func<Type, Assembly, string> factory)
	{
		if (!__baseAttributeType.IsAssignableFrom(type)) throw new InvalidCastException($"{type.FullName} is not a type of {__baseAttributeType.FullName}.");
		return _registeredAttributes.TryAdd(type, factory);
	}

	public bool DeregisterAttribute<T>(out Func<Type, Assembly, string> factory)
		where T : Attribute
	{
		return DeregisterAttribute(typeof(T), out factory);
	}

	public bool DeregisterAttribute([NotNull] Type type, out Func<Type, Assembly, string> factory)
	{
		if (!__baseAttributeType.IsAssignableFrom(type)) throw new InvalidCastException($"{type.FullName} is not a type of {__baseAttributeType.FullName}.");
		return _registeredAttributes.TryRemove(type, out factory);
	}

	public void ClearAttributes()
	{
		_registeredAttributes.Clear();
	}
}