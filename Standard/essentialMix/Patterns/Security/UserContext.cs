using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Claims;
using System.Security.Permissions;
using System.Security.Principal;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

namespace essentialMix.Patterns.Security;

public sealed class UserContext : Disposable
{
	private readonly NetworkCredential _credential;

	private WindowsIdentity _identity;

	public UserContext([NotNull] string userName, [NotNull] string password, LogonType logonType)
		: this(new NetworkCredential(userName, password), logonType)
	{
	}

	public UserContext([NotNull] string userName, [NotNull] string password, string domain, LogonType logonType)
		: this(new NetworkCredential(userName, password, domain), logonType)
	{
	}

	public UserContext([NotNull] string userName, [NotNull] SecureString password, LogonType logonType)
		: this(new NetworkCredential(userName, password), logonType)
	{
	}

	public UserContext([NotNull] string userName, [NotNull] SecureString password, string domain, LogonType logonType)
		: this(new NetworkCredential(userName, password, domain), logonType)
	{
	}

	/// <inheritdoc />
	public UserContext([NotNull] NetworkCredential credential, LogonType logonType)
	{
		_credential = credential;
		LogonType = logonType;
		_identity = WindowsIdentity.GetCurrent();
	}

	public LogonType LogonType { get; private set; }

	public string NameClaimType => _identity.NameClaimType;

	public string RoleClaimType => _identity.RoleClaimType;

	public SafeAccessTokenHandle AccessToken => _identity.AccessToken;

	public string AuthenticationType => _identity.AuthenticationType;

	public IEnumerable<Claim> Claims => _identity.Claims;

	public IEnumerable<Claim> DeviceClaims => _identity.DeviceClaims;

	public IdentityReferenceCollection Groups => _identity.Groups;

	public TokenImpersonationLevel ImpersonationLevel => _identity.ImpersonationLevel;

	public bool IsAnonymous => _identity.IsAnonymous;

	public bool IsAuthenticated => _identity.IsAuthenticated;

	public bool IsGuest => _identity.IsGuest;

	public bool IsSystem => _identity.IsSystem;

	[NotNull]
	public string Name => _identity.Name;

	public SecurityIdentifier Owner => _identity.Owner;

	public SecurityIdentifier User => _identity.User;

	public IEnumerable<Claim> UserClaims => _identity.UserClaims;

	public ClaimsIdentity Actor { get => _identity.Actor; set => _identity.Actor = value; }

	public object BootstrapContext { get => _identity.BootstrapContext; set => _identity.BootstrapContext = value; }

	public string Label { get => _identity.Label; set => _identity.Label = value; }

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing) Logout();
		base.Dispose(disposing);
	}

	public bool Logon() { return Logon(LogonType); }

	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public bool Logon(LogonType logonType)
	{
		Logout();

		IntPtr handle;
		bool loggedOn;

		if (_credential.SecurePassword == null)
		{
			loggedOn = Win32.LogonUser(_credential.UserName, _credential.Domain, _credential.Password, logonType, 0, out handle);
		}
		else
		{
			IntPtr passwordPtr = Marshal.SecureStringToGlobalAllocUnicode(_credential.SecurePassword);

			try
			{
				loggedOn = Win32.LogonUser(_credential.UserName, _credential.Domain, passwordPtr, logonType, 0, out handle);
			}
			finally
			{
				Marshal.ZeroFreeGlobalAllocUnicode(passwordPtr);
			}
		}

		if (!loggedOn) return false;

		if (!Win32.DuplicateToken(handle, SecurityImpersonationLevel.SecurityImpersonation, out IntPtr tokenHandle))
		{
			Win32.CloseHandle(handle);
			return false;
		}

		_identity = new WindowsIdentity(tokenHandle);
		LogonType = logonType;
		return true;
	}

	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public bool Logout()
	{
		if (_identity == null) return true;
		ObjectHelper.Dispose(ref _identity);
		return Win32.RevertToSelf();
	}

	public void AddClaim(Claim claim) { _identity.AddClaim(claim); }
	public void AddClaims(IEnumerable<Claim> claims) { _identity.AddClaims(claims); }
	public IEnumerable<Claim> FindAll(Predicate<Claim> match) { return _identity.FindAll(match); }
	public IEnumerable<Claim> FindAll(string type) { return _identity.FindAll(type); }
	public Claim FindFirst(Predicate<Claim> match) { return _identity.FindFirst(match); }
	public Claim FindFirst(string type) { return _identity.FindFirst(type); }
	public bool HasClaim(Predicate<Claim> match) { return _identity.HasClaim(match); }
	public bool HasClaim(string type, string value) { return _identity.HasClaim(type, value); }
	public void RemoveClaim(Claim claim) { _identity.RemoveClaim(claim); }
	public bool TryRemoveClaim(Claim claim) { return _identity.TryRemoveClaim(claim); }
	public void WriteTo(BinaryWriter writer) { _identity.WriteTo(writer); }
}