using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using essentialMix.Data.Model;
using essentialMix.Extensions;

namespace essentialMix.Identity;

[DebuggerDisplay("{Name} [{Code}]")]
[Serializable]
public class Country : IEntity
{
	private string _name;
	private string _code;

	[Key]
	[StringLength(3, MinimumLength = 3)]
	public string Code
	{
		get => _code;
		set => _code = value.ToNullIfEmpty()?.ToUpperInvariant();
	}

	[Required]
	[StringLength(255)]
	public string Name
	{
		get => _name;
		set => _name = value.ToNullIfEmpty();
	}

	public virtual ICollection<City> Cities { get; set; }
}