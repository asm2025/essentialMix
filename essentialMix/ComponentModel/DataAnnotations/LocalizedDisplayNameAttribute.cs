using System;
using System.ComponentModel;
using System.Resources;
using JetBrains.Annotations;

namespace essentialMix.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Enum)]
public class LocalizableDisplayNameAttribute(string displayName, Type resourceType) : DisplayNameAttribute
{
	private readonly string _displayName = displayName?.Trim();
	private readonly ResourceManager _resource = resourceType == null ? null : new ResourceManager(resourceType);

	private string _displayNameValue;

	public LocalizableDisplayNameAttribute()
		: this(null, null)
	{
	}

	public LocalizableDisplayNameAttribute(string displayName)
		: this(displayName, null)
	{
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