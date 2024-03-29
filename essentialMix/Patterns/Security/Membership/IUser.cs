﻿using System.Net;
using System.Security;

namespace essentialMix.Patterns.Security.Membership;

public interface IUser : ICredentials
{
	string UserName { get; }
	SecureString Password { get; }
}

public interface IUser<out TKey> : IUser
{
	TKey Id { get; }
	string Email { get; }
}