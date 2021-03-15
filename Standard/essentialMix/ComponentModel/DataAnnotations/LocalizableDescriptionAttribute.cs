using System;
using System.ComponentModel;
using System.Resources;
using JetBrains.Annotations;

namespace essentialMix.ComponentModel.DataAnnotations
{
	public class LocalizableDescriptionAttribute : DescriptionAttribute
	{
		private readonly string _description;
		private readonly ResourceManager _resource;

		private string _descriptionValue;

		public LocalizableDescriptionAttribute()
			: this(null, null)
		{
		}

		public LocalizableDescriptionAttribute(string description)
			: this(description, null)
		{
		}

		public LocalizableDescriptionAttribute(string description, Type resourceType)
		{
			_resource = resourceType == null ? null : new ResourceManager(resourceType);
			_description = description?.Trim();
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
}