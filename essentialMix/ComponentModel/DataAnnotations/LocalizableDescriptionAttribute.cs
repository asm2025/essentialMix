using System;
using System.ComponentModel;
using System.Resources;
using JetBrains.Annotations;

namespace essentialMix.ComponentModel.DataAnnotations;

public class LocalizableDescriptionAttribute(string description, Type resourceType) : DescriptionAttribute
{
	private readonly string _description = description?.Trim();
	private readonly ResourceManager _resource = resourceType == null ? null : new ResourceManager(resourceType);

	private string _descriptionValue;

	public LocalizableDescriptionAttribute()
		: this(null, null)
	{
	}

	public LocalizableDescriptionAttribute(string description)
		: this(description, null)
	{
	}

	[NotNull]
	public override string Description
	{
		get
		{
			if (_descriptionValue != null) return _descriptionValue;

			if (_resource == null || string.IsNullOrEmpty(_description))
			{
				_descriptionValue = string.Empty;
				return _descriptionValue;
			}

			string description = _resource.GetString(_description);
			_descriptionValue = string.IsNullOrEmpty(description)
									? $"[[{_description}]]"
									: description;
			return _descriptionValue;
		}
	}
}