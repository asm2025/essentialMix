using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using essentialMix.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace essentialMix.Core.Authorization;

[Owned]
[Serializable]
public class RefreshToken<TKey> : IEntity<string>
	where TKey : IComparable<TKey>, IEquatable<TKey>
{
	[Key]
	[StringLength(90)]
	public string Id { get; set; }
	[Required]
	public TKey UserId { get; set; }
	public virtual User<TKey> User { get; set; }
	public DateTime Created { get; set; }
	public DateTime Expires { get; set; }
	[Required]
	public TKey CreatedBy { get; set; }
	public DateTime? Revoked { get; set; }
	public TKey RevokedBy { get; set; }
	public TKey ReplacedBy { get; set; }

	[NotMapped]
	public bool IsExpired => DateTime.UtcNow >= Expires;

	[NotMapped]
	public bool IsActive => Revoked == null && !IsExpired;
}