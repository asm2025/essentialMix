using System;
using System.ComponentModel;
using System.Resources;
using JetBrains.Annotations;

namespace essentialMix.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Enum)]
public class LocalizableDisplayNameAttribute : DisplayNameAttribute
{
	private readonly string _displayName;
	private readonly ResourceManager _resource;

	private string _displayNameValue;

	public LocalizableDisplayNameAttribute()
		: this(null, null)
	{
	}

	public LocalizableDisplayNameAttribute(string displayName)
		: this(displayName, null)
	{
	}

	public LocalizableDisplayNameAttribute(string displayName, Type resourceType)
	{
		_resource = resourceType == null ? null : new ResourceManager(resourceType);
		_displayName = displayName?.Trim();
	}

	[NotNull]
	public override string DisplayName
	{
		get
		{
			if (_displayNameValue != null) return _displayNameValue;

			if (_resource == null || string.IsNullOrEmpty(_displayName))
			{
				_displayNameValue = string.Empty;
				return _displayNameValue;
			}

			string displayName = _resource.GetString(_displayName);
			_displayNameValue = string.IsNullOrEmpty(displayName)
									? $"[[{_displayName}]]"
									: displayName;
			return _displayNameValue;
		}
	}
}