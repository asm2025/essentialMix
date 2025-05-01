using System;
using System.Management;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Windows;

public class SystemInfoRequest(SystemInfoType type)
{
	private string _machineName;
	private string _scope;
	private string _selectExpression = "*";
	private string _whereExpression;

	private string _scopeRoot;

	public SystemInfoType Type { get; set; } = type;

	public string MachineName
	{
		get => _machineName;
		set
		{
			_machineName = value?.Trim('\\', '/', ' ');
			_scopeRoot = null;
		}
	}

	public string Scope
	{
		get => _scope;
		set
		{
			_scope = value?.Trim();
			_scopeRoot = null;
		}
	}

	public string SelectExpression
	{
		get => _selectExpression;
		set
		{
			value = value?.Trim();
			if (string.IsNullOrEmpty(value)) value = "*";
			_selectExpression = value;
		}
	}

	public string WhereExpression
	{
		get => _whereExpression;
		set => _whereExpression = value?.Trim();
	}

	public Predicate<ManagementObject> Filter { get; set; }

	public EnumerationOptions Options { get; set; }

	[NotNull]
	public string ScopeRoot
	{
		get
		{
			if (_scopeRoot == null)
			{
				StringBuilder sb = new StringBuilder(256);

				if (!string.IsNullOrEmpty(MachineName))
				{
					sb.Append(@"\\");
					sb.Append(MachineName);
				}

				if (!string.IsNullOrEmpty(Scope))
				{
					if (Scope[0] != '\\' && sb.Length > 0) sb.Append('\\');
					sb.Append(Scope);
				}

				_scopeRoot = sb.ToString();
			}

			return _scopeRoot;
		}
	}
}

public class SystemInfoRequest<T> : SystemInfoRequest
{
	/// <inheritdoc />
	public SystemInfoRequest(SystemInfoType type, [NotNull] Func<ManagementObject, T> converter)
		: base(type)
	{
		Converter = converter;
	}

	[NotNull]
	public Func<ManagementObject, T> Converter { get; set; }
}