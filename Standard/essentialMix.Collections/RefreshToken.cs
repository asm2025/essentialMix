using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace essentialMix.Identity;

[Owned]
[Serializable]
public class RefreshToken : IEntity
{
	[Key]
	[StringLength(90)]
	public string Value { get; set; }
	[Required]
	[StringLength(128, MinimumLength = 128)]
	public string UserId { get; set; }
	public virtual User User { get; set; }
	public DateTime Created { get; set; }
	public DateTime Expires { get; set; }
	[Required]
	[StringLength(256)]
	public string CreatedBy { get; set; }
	public DateTime? Revoked { get; set; }
	[StringLength(128, MinimumLength = 128)]
	public string RevokedBy { get; set; }
	[StringLength(90)]
	public string ReplacedBy { get; set; }

	[NotMapped]
	public bool IsExpired => DateTime.UtcNow >= Expires;

	[NotMapped]
	public bool IsActive => Revoked == null && !IsExpired;
}