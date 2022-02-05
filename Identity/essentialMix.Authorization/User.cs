using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json.Serialization;
using essentialMix.Data.Model;
using essentialMix.Extensions;
using essentialMix.Identity;
using Microsoft.AspNetCore.Identity;

namespace essentialMix.Core.Authorization;

[DebuggerDisplay("User: {UserName}, E-mail:{Email}")]
[Serializable]
public class User<TKey> : IdentityUser<TKey>, IEntity
	where TKey : IComparable<TKey>, IEquatable<TKey>
{
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

	public Gender Gender { get; set; }
	public DateTime DateOfBirth { get; set; }
	public DateTime Created { get; set; }
	public DateTime Modified { get; set; }
	public DateTime LastActive { get; set; }

	[Required]
	public int CityId { get; set; }

	public virtual City City { get; set; }

	public virtual ICollection<UserRole<TKey>> UserRoles { get; set; }

	[JsonIgnore]
	public virtual ICollection<RefreshToken<TKey>> RefreshTokens { get; set; }
}