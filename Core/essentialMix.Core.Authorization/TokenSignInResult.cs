using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;

namespace essentialMix.Core.Authorization;

public class TokenSignInResult : SignInResult
{
	public User User { get; protected set; }
	public string Token { get; protected set; }
	[JsonIgnore] // refresh token is returned in http only cookie
	public string RefreshToken { get; protected set; }

	/// <summary>
	/// Returns a <see cref="SignInResult"/> that represents a failed sign-in.
	/// </summary>
	/// <returns>A <see cref="SignInResult"/> that represents a failed sign-in.</returns>
	public new static TokenSignInResult Failed { get; } = new TokenSignInResult();

	/// <summary>
	/// Returns a <see cref="SignInResult"/> that represents a sign-in attempt that failed because 
	/// the user was locked out.
	/// </summary>
	/// <returns>A <see cref="SignInResult"/> that represents sign-in attempt that failed due to the
	/// user being locked out.</returns>
	public new static TokenSignInResult LockedOut { get; } = new TokenSignInResult { IsLockedOut = true };

	/// <summary>
	/// Returns a <see cref="SignInResult"/> that represents a sign-in attempt that failed because 
	/// the user is not allowed to sign-in.
	/// </summary>
	/// <returns>A <see cref="SignInResult"/> that represents sign-in attempt that failed due to the
	/// user is not allowed to sign-in.</returns>
	public new static TokenSignInResult NotAllowed { get; } = new TokenSignInResult { IsNotAllowed = true };

	/// <summary>
	/// Returns a <see cref="SignInResult"/> that represents a sign-in attempt that needs two-factor 
	/// authentication.
	/// </summary>
	/// <returns>A <see cref="SignInResult"/> that represents sign-in attempt that needs two-factor
	/// authentication.</returns>
	public new static TokenSignInResult TwoFactorRequired { get; } = new TokenSignInResult { RequiresTwoFactor = true };

	/// <summary>
	/// Returns a <see cref="SignInResult"/> that represents a successful sign-in.
	/// </summary>
	/// <returns>A <see cref="SignInResult"/> that represents a successful sign-in.</returns>
	[NotNull]
	public static TokenSignInResult SuccessFrom([NotNull] User user, [NotNull] string token, [NotNull] string refreshToken)
	{
		return new TokenSignInResult
		{
			Succeeded = true,
			User = user,
			Token = token,
			RefreshToken = refreshToken
		};
	}
}