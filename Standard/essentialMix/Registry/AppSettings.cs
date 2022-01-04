using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using essentialMix.Collections;
using essentialMix.Extensions;
using Microsoft.Win32;
using JetBrains.Annotations;

namespace essentialMix.Registry;

[Serializable]
public class AppSettings : Properties
{
	private const string ROOT_PREFIX = @"Software\";

	[NonSerialized]
	private static Assembly __thisAssembly;

	[NonSerialized]
	private string _companyNameDefault;

	[NonSerialized]
	private string _applicationNameDefault;

	[NonSerialized]
	private string _applicationVersionDefault;

	[NonSerialized]
	private string _key;

	private string _companyName;
	private string _applicationName;
	private string _applicationVersion;

	public AppSettings()
	{
		Init();
	}

	public AppSettings(IEqualityComparer<string> comparer)
		: base(comparer)
	{
		Init();
	}

	public AppSettings([NotNull] IEnumerable<IProperty> collection)
		: base(collection, StringComparer.OrdinalIgnoreCase)
	{
		Init();
	}

	public AppSettings([NotNull] IEnumerable<IProperty> collection, IEqualityComparer<string> comparer)
		: base(collection, comparer)
	{
		Init();
	}

	public override object Clone() { return this.CloneMemberwise(); }

	public string CompanyName
	{
		get
		{
			if (_companyName != null) return _companyName;
			_companyName = _companyNameDefault;
			_key = null;
			return _companyName;
		}
		set
		{
			if (string.IsNullOrWhiteSpace(value)) value = null;
			if (_companyName == value) return;
			_companyName = value;
			_key = null;
			OnPropertyChanged();
		}
	}

	public string ApplicationName
	{
		get
		{
			if (_applicationName != null) return _applicationName;
			_applicationName = _applicationNameDefault;
			_key = null;
			return _applicationName;
		}
		set
		{
			if (string.IsNullOrWhiteSpace(value)) value = null;
			if (_applicationName == value) return;
			_applicationName = value;
			_key = null;
			OnPropertyChanged();
		}
	}

	public string ApplicationVersion
	{
		get
		{
			if (_applicationVersion != null) return _applicationVersion;
			_applicationVersion = _applicationVersionDefault;
			_key = null;
			return _applicationVersion;
		}
		set
		{
			if (string.IsNullOrWhiteSpace(value)) value = null;
			if (_applicationVersion == value) return;
			_applicationVersion = value;
			_key = null;
			OnPropertyChanged();
		}
	}

	[NotNull]
	public string Key
	{
		get
		{
			if (_key != null) return _key;

			StringBuilder sb = new StringBuilder();
			if (!string.IsNullOrEmpty(CompanyName)) sb.Append(CompanyName);

			if (!string.IsNullOrEmpty(ApplicationName))
			{
				if (sb.Length > 0) sb.Append('\\');
				sb.Append(ApplicationName);
			}

			if (!string.IsNullOrEmpty(ApplicationVersion))
			{
				if (sb.Length > 0) sb.Append('\\');
				sb.Append(ApplicationVersion);
			}

			if (sb.Length > 0) _key = sb.Insert(0, ROOT_PREFIX).Append('\\').ToString();
			if (_key == null) throw new InvalidOperationException("Key properties are not set");
			return _key;
		}
	}

	[SecurityCritical]
	[RegistryPermission(SecurityAction.Demand)]
	public bool Load()
	{
		if (Count == 0) return true;

		try
		{
			if (Values.Any(p => p.Scope == PropertyScope.Global))
			{
				using (RegistryKey key = global::Microsoft.Win32.Registry.LocalMachine.CreateSubKey(Key, false))
				{
					foreach (IProperty property in Values.Where(p => p.Scope == PropertyScope.Global))
						LoadProperty(key, property);
				}
			}

			if (Values.Any(p => p.Scope == PropertyScope.CurrentUser))
			{
				using (RegistryKey key = global::Microsoft.Win32.Registry.CurrentUser.CreateSubKey(Key, false))
				{
					foreach (IProperty property in Values.Where(p => p.Scope == PropertyScope.CurrentUser))
						LoadProperty(key, property);
				}
			}

			return true;
		}
		catch
		{
			return false;
		}
	}

	[SecurityCritical]
	[RegistryPermission(SecurityAction.Demand)]
	public bool Save()
	{
		if (Count == 0) return true;

		try
		{
			if (Values.Any(p => p.Scope == PropertyScope.Global))
			{
				using (RegistryKey key = global::Microsoft.Win32.Registry.LocalMachine.CreateSubKey(Key, true))
				{
					foreach (IProperty property in Values.Where(p => p.Scope == PropertyScope.Global))
						SaveProperty(key, property);
				}
			}

			if (Values.Any(p => p.Scope == PropertyScope.CurrentUser))
			{
				using (RegistryKey key = global::Microsoft.Win32.Registry.CurrentUser.CreateSubKey(Key, true))
				{
					foreach (IProperty property in Values.Where(p => p.Scope == PropertyScope.CurrentUser))
						SaveProperty(key, property);
				}
			}

			return true;
		}
		catch
		{
			return false;
		}
	}

	private void Init()
	{
		_key = null;
		Assembly assembly = null;
		StackTrace stackTrace = new StackTrace();

		for (int i = 0; i < stackTrace.FrameCount; i++)
		{
			StackFrame frame = stackTrace.GetFrame(i);
			if (frame == null) continue;

			MethodBase method = frame.GetMethod();
			if (method == null || method.DeclaringType == null || method.DeclaringType.Assembly == ThisAssembly) continue;

			assembly = method.DeclaringType.Assembly;
			break;
		}

		if (assembly == null)
		{
			_companyNameDefault = null;
			_applicationNameDefault = null;
			_applicationVersionDefault = null;
			return;
		}

		_companyNameDefault = assembly.GetAttribute<AssemblyCompanyAttribute>()?.Company;
		_applicationNameDefault = assembly.GetAttribute<AssemblyTitleAttribute>()?.Title ?? Path.GetFileNameWithoutExtension(assembly.Location);
		_applicationVersionDefault = assembly.GetName().Version?.ToString();
	}

	private static Assembly ThisAssembly => __thisAssembly ??= Assembly.GetAssembly(typeof(AppSettings));

	private static void LoadProperty(RegistryKey key, [NotNull] IProperty property)
	{
		if (property == null) throw new ArgumentNullException(nameof(property));

		if (property.ValueType.IsEnum)
		{
			object value = key.GetValue(property.Name, property.DefaultValue);

			switch (value)
			{
				case null:
					property.Value = property.DefaultValue;
					break;
				case Enum enm when property.ValueType.IsDefined(enm):
					property.Value = value.ToEnum(property.ValueType);
					break;
			}
		}
		else
		{
			switch (property.ValueType.AsTypeCode())
			{
				case TypeCode.Empty:
				case TypeCode.DBNull:
					property.Value = property.DefaultValue;
					break;
				case TypeCode.Decimal:
					string dec = (string)key.GetValue(property.Name, property.DefaultValue.ToString());
					property.Value = string.IsNullOrEmpty(dec) ? 0.0m : decimal.Parse(dec);
					break;
				case TypeCode.Char:
					string ch = (string)key.GetValue(property.Name, property.DefaultValue.ToString());
					property.Value = string.IsNullOrEmpty(ch) ? '\0' : char.Parse(ch);
					break;
				case TypeCode.DateTime:
					string dt = (string)key.GetValue(property.Name, property.DefaultValue.ToString());
					property.Value = string.IsNullOrEmpty(dt) ? DateTime.MinValue : DateTime.Parse(dt);
					break;
				default:
					property.Value = Convert.ChangeType(key.GetValue(property.Name, property.DefaultValue), property.ValueType);
					break;
			}
		}
	}

	private static void SaveProperty(RegistryKey key, [NotNull] IProperty property)
	{
		if (property == null) throw new ArgumentNullException(nameof(property));

		if (property.ValueType.IsEnum)
		{
			key.SetValue(property.Name, Convert.ToInt32(property.Value), RegistryValueKind.DWord);
		}
		else
		{
			switch (property.ValueType.AsTypeCode())
			{
				case TypeCode.Empty:
				case TypeCode.DBNull:
					key.DeleteValue(property.Name, false);
					break;
				case TypeCode.Object:
					if (property.Value == null) key.DeleteValue(property.Name, false);
					else key.SetValue(property.Name, property.Value, RegistryValueKind.Binary);
					break;
				case TypeCode.Boolean:
				case TypeCode.SByte:
				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Single:
					key.SetValue(property.Name, property.Value, RegistryValueKind.DWord);
					break;
				case TypeCode.Int64:
				case TypeCode.UInt64:
				case TypeCode.Double:
					key.SetValue(property.Name, property.Value, RegistryValueKind.QWord);
					break;
				case TypeCode.Char:
				case TypeCode.Decimal:
				case TypeCode.DateTime:
					key.SetValue(property.Name, property.Value.ToString(), RegistryValueKind.String);
					break;
				case TypeCode.String:
					if (property.Value == null) key.DeleteValue(property.Name, false);
					else key.SetValue(property.Name, property.Value, RegistryValueKind.String);
					break;
			}
		}
	}
}