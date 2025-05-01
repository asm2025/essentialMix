using System;
using System.Net;
using System.Security;
using essentialMix.Extensions;

namespace essentialMix.Patterns.Security.Membership;

public class User : IUser
{
	private string _userName;

	public User(string userName)
		: this(userName, null)
	{
	}

	public User(string userName, SecureString password)
	{
		UserName = userName;
		Password = password;
	}

	public string UserName
	{
		get => _userName;
		set
		{
			value = value?.Trim();
			if (string.IsNullOrEmpty(value)) value = null;
			_userName = value;
		}
	}

	public SecureString Password { get; set; }

	/// <inheritdoc />
	public NetworkCredential GetCredential(Uri uri, string authType)
	{
		return UserName == null
					? CredentialCache.DefaultNetworkCredentials
					: new NetworkCredential(UserName, Password.UnSecure());
	}
}

public class User<TKey>(TKey id, string userName, SecureString password) : User(userName, password), IUser<TKey>
{
	public User(string userName)
		: this(default(TKey), userName, null)
	{
	}

	public User(string userName, SecureString password)
		: this(default(TKey), userName, password)
	{
	}

	public TKey Id { get; } = id;
	public string Email { get; set; }
}