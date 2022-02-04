using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json.Serialization;
using essentialMix.Data.Model;
using essentialMix.Extensions;

namespace essentialMix.Identity;

[DebuggerDisplay("User: {UserName}, E-mail:{Email}")]
[Serializable]
public class User : IdentityUser<string>, IEntity
{
	public const string DATE_FORMAT = "yyyy-MM-dd";

	private string _firstName;
	private string _knownAs;
	private string _lastName;

	[Required]
	[StringLength(255)]
	public string FirstName
	{
		get => _firstName;
		set => _firstName = value.ToNullIfEmpty();
	}

	[Required]
	[StringLength(255)]
	public string LastName
	{
		get => _lastName;
		set => _lastName = value.ToNullIfEmpty();
	}

	[StringLength(255)]
	public string KnownAs
	{
		get => _knownAs ?? FirstName;
		set => _knownAs = value.ToNullIfEmpty();
	}

	public Genders Gender { get; set; }
	public DateTime DateOfBirth { get; set; }
	public DateTime Created { get; set; }
	public DateTime Modified { get; set; }
	public DateTime LastActive { get; set; }

	[Required]
	public Guid CityId { get; set; }

	public virtual City City { get; set; }

	public virtual ICollection<UserRole> UserRoles { get; set; }

	[JsonIgnore]
	public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
}