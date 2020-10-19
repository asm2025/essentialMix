using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using asm.Extensions;
using Other.JonSkeet.MiscUtil.Collections;
using JetBrains.Annotations;
using Microsoft.IdentityModel.Tokens;

namespace asm.Core.Authentication.JwtBearer
{
	public sealed partial class JwtTokenBuilder
	{
		public const string HDR_JWT_STRING = "JwtEncodedString";
		public const string HDR_JWT_HEADER = "JwtHeader";
		public const string HDR_ISSUER = "Issuer";

		private const string SET_JWT_STRING = "JwtEncodedString";
		
		private const string SET_JWT_HEADER = "JwtHeader";
		private const string SET_JWT_PAYLOAD = "JwtPayload";
		private const string SET_JWT_SECURITY_TOKEN = "JwtSecurityToken";
		private const string SET_RAW_HEADER = "RawHeader";
		private const string SET_RAW_PAYLOAD = "RawSignature";
		private const string SET_RAW_SIGNATURE = "RawSignature";
		private const string SET_RAW_ENCRYPTED_KEY = "RawEncryptedKey";
		private const string SET_RAW_INITIALIZATION_VECTOR = "RawInitializationVector";
		private const string SET_RAW_CIPHER_TEXT = "RawCipherText";
		private const string SET_RAW_AUTHENTICATION_TAG = "RawAuthenticationTag";
		
		private const string SET_SUBJECT = "Subject";
		private const string SET_ISSUER = "Issuer";
		private const string SET_AUDIENCE = "Audience";
		private const string SET_CLAIMS = "Claims";
		private const string SET_NOT_BEFORE = "NotBefore";
		private const string SET_EXPIRES = "Expires";
		private const string SET_SIGNING_CREDENTIALS = "SigningCredentials";

		private readonly IDictionary<string, IDictionary<string, object>> _settings = new Dictionary<string, IDictionary<string, object>>(StringComparer.OrdinalIgnoreCase)
		{
			{ HDR_JWT_STRING, null },
			{ HDR_JWT_HEADER, null },
			{ HDR_ISSUER, null },
		};

		[NotNull]
		public JwtTokenBuilder Clear(string settingName = null)
		{
			if (!string.IsNullOrEmpty(settingName)) _settings.Remove(settingName);
			else _settings.Clear();
			return this;
		}

		[NotNull]
		public JwtSecurityToken Build()
		{
			string key = null;
			IDictionary<string, object> settings = null;

			foreach (KeyValuePair<string, IDictionary<string, object>> pair in _settings)
			{
				if (key != null) throw new TargetParameterCountException("Conflicting parameters found. You can either use JwtEncodedString, JwtHeader parameters group, or Issuer parameters group. See JwtSecurityToken constructor overloads for details.");
				if (pair.Value == null || pair.Value.Count == 0) continue;
				key = pair.Key;
				settings = pair.Value;
			}

			if (key == null) throw new TargetParameterCountException("No valid parameters found. You can either use JwtEncodedString, JwtHeader parameters group, or Issuer parameters group. See JwtSecurityToken constructor overloads for details.");

			return key switch
			{
				HDR_JWT_STRING => BuildFromJwtString(settings),
				HDR_JWT_HEADER => BuildFromHeader(settings),
				HDR_ISSUER => BuildFromIssuer(settings),
				_ => throw new KeyNotFoundException()
			};
		}

		private void AddSetting([NotNull] string root, [NotNull] string key, object value)
		{
			IDictionary<string, object> dicOptions = _settings.GetOrAdd(root, r => new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));
			dicOptions[key] = value;
		}
	}

	partial class JwtTokenBuilder
	{
		// HDR_JWT_STRING
		[NotNull]
		public JwtTokenBuilder AddJwtEncodedString([NotNull] string value)
		{
			if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
			AddSetting(HDR_JWT_STRING, SET_JWT_STRING, value);
			return this;
		}

		[NotNull]
		private static JwtSecurityToken BuildFromJwtString([NotNull] IDictionary<string, object> settings)
		{
			/*
			string jwtEncodedString
			*/
			if (!settings.TryGetValue(SET_JWT_STRING, out object value)) throw new KeyNotFoundException("No valid parameters found.");
			return new JwtSecurityToken((string)value);
		}
	}

	partial class JwtTokenBuilder
	{
		// HDR_JWT_HEADER
		[NotNull]
		public JwtTokenBuilder AddJwtHeader([NotNull] JwtHeader value)
		{
			AddSetting(HDR_JWT_HEADER, SET_JWT_HEADER, value);
			return this;
		}

		[NotNull]
		public JwtTokenBuilder AddJwtPayload([NotNull] JwtPayload value)
		{
			AddSetting(HDR_JWT_HEADER, SET_JWT_PAYLOAD, value);
			return this;
		}

		[NotNull]
		public JwtTokenBuilder AddRawHeader([NotNull] string value)
		{
			if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
			AddSetting(HDR_JWT_HEADER, SET_RAW_HEADER, value);
			return this;
		}

		[NotNull]
		public JwtTokenBuilder AddRawPayload([NotNull] string value)
		{
			if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
			AddSetting(HDR_JWT_HEADER, SET_RAW_PAYLOAD, value);
			return this;
		}

		[NotNull]
		public JwtTokenBuilder AddRawSignature([NotNull] string value)
		{
			if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
			AddSetting(HDR_JWT_HEADER, SET_RAW_SIGNATURE, value);
			return this;
		}

		[NotNull]
		public JwtTokenBuilder AddJwtSecurityToken([NotNull] JwtSecurityToken value)
		{
			AddSetting(HDR_JWT_HEADER, SET_JWT_SECURITY_TOKEN, value);
			return this;
		}

		[NotNull]
		public JwtTokenBuilder AddRawEncryptedKey([NotNull] string value)
		{
			if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
			AddSetting(HDR_JWT_HEADER, SET_RAW_ENCRYPTED_KEY, value);
			return this;
		}

		[NotNull]
		public JwtTokenBuilder AddRawInitializationVector([NotNull] string value)
		{
			if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
			AddSetting(HDR_JWT_HEADER, SET_RAW_INITIALIZATION_VECTOR, value);
			return this;
		}

		[NotNull]
		public JwtTokenBuilder AddRawCipherText([NotNull] string value)
		{
			if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
			AddSetting(HDR_JWT_HEADER, SET_RAW_CIPHER_TEXT, value);
			return this;
		}

		[NotNull]
		public JwtTokenBuilder AddRawAuthenticationTag([NotNull] string value)
		{
			if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
			AddSetting(HDR_JWT_HEADER, SET_RAW_AUTHENTICATION_TAG, value);
			return this;
		}

		[NotNull]
		private static JwtSecurityToken BuildFromHeader([NotNull] IDictionary<string, object> settings)
		{
			/*
			JwtHeader header,
			JwtPayload payload
			
			JwtHeader header,
			JwtPayload payload,
			string rawHeader,
			string rawPayload,
			string rawSignature

			JwtHeader header,
			JwtSecurityToken innerToken,
			string rawHeader,
			string rawEncryptedKey,
			string rawInitializationVector,
			string rawCipherText,
			string rawAuthenticationTag
			*/
			if (!settings.TryGetValue(SET_JWT_HEADER, out object value)) throw new KeyNotFoundException($"Missing {SET_JWT_HEADER} parameter.");
			JwtHeader header = (JwtHeader)value;
			string rawHeader = null;
			
			if (settings.TryGetValue(SET_JWT_SECURITY_TOKEN, out value))
			{
				// go for innerToken path
				JwtSecurityToken innerToken = (JwtSecurityToken)value;
				string rawEncryptedKey = null;
				string rawInitializationVector = null;
				string rawCipherText = null;
				string rawAuthenticationTag = null;
				if (settings.TryGetValue(SET_RAW_HEADER, out value)) rawHeader = (string)value;
				if (settings.TryGetValue(SET_RAW_ENCRYPTED_KEY, out value)) rawEncryptedKey = (string)value;
				if (settings.TryGetValue(SET_RAW_INITIALIZATION_VECTOR, out value)) rawInitializationVector = (string)value;
				if (settings.TryGetValue(SET_RAW_CIPHER_TEXT, out value)) rawCipherText = (string)value;
				if (settings.TryGetValue(SET_RAW_AUTHENTICATION_TAG, out value)) rawAuthenticationTag = (string)value;
				return new JwtSecurityToken(header, innerToken, rawHeader, rawEncryptedKey, rawInitializationVector, rawCipherText, rawAuthenticationTag);
			}

			JwtPayload payload = null;
			
			if (settings.TryGetValue(SET_JWT_PAYLOAD, out value))
			{
				payload = (JwtPayload)value;
				if (settings.Count == 2) return new JwtSecurityToken(header, payload);
			}

			string rawPayload = null;
			string rawSignature = null;
			if (settings.TryGetValue(SET_RAW_HEADER, out value)) rawHeader = (string)value;
			if (settings.TryGetValue(SET_RAW_PAYLOAD, out value)) rawPayload = (string)value;
			if (settings.TryGetValue(SET_RAW_SIGNATURE, out value)) rawSignature = (string)value;
			return new JwtSecurityToken(header, payload, rawHeader, rawPayload, rawSignature);
		}
	}

	partial class JwtTokenBuilder
	{
		// HDR_ISSUER
		[NotNull]
		public JwtTokenBuilder AddSubject([NotNull] string value)
		{
			if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
			AddSetting(HDR_ISSUER, SET_SUBJECT, value);
			return this;
		}

		[NotNull]
		public JwtTokenBuilder AddIssuer([NotNull] string value)
		{
			if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
			AddSetting(HDR_ISSUER, SET_ISSUER, value);
			return this;
		}

		[NotNull]
		public JwtTokenBuilder AddAudience([NotNull] string value)
		{
			if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
			AddSetting(HDR_ISSUER, SET_AUDIENCE, value);
			return this;
		}

		[NotNull]
		public JwtTokenBuilder AddClaims([NotNull] IEnumerable<Claim> value)
		{
			AddSetting(HDR_ISSUER, SET_CLAIMS, value);
			return this;
		}

		[NotNull]
		public JwtTokenBuilder AddStartDate(DateTime value)
		{
			AddSetting(HDR_ISSUER, SET_NOT_BEFORE, value);
			return this;
		}

		[NotNull]
		public JwtTokenBuilder AddExpirationDate(DateTime value)
		{
			AddSetting(HDR_ISSUER, SET_EXPIRES, value);
			return this;
		}

		[NotNull]
		public JwtTokenBuilder AddSigningCredentials([NotNull] SigningCredentials value)
		{
			AddSetting(HDR_ISSUER, SET_SIGNING_CREDENTIALS, value);
			return this;
		}

		[NotNull]
		private static JwtSecurityToken BuildFromIssuer([NotNull] IDictionary<string, object> settings)
		{
			/*
			string issuer = null,
			string audience = null,
			IEnumerable<Claim> claims = null,
			DateTime? notBefore = null,
			DateTime? expires = null,
			SigningCredentials signingCredentials = null
			*/
			if (!settings.TryGetValue(SET_SUBJECT, out object value)) throw new KeyNotFoundException($"Missing {SET_SUBJECT} parameter.");
			string subject = (string)value;
			
			if (!settings.TryGetValue(SET_ISSUER, out value)) throw new KeyNotFoundException($"Missing {SET_ISSUER} parameter.");
			string issuer = (string)value;

			if (!settings.TryGetValue(SET_AUDIENCE, out value)) throw new KeyNotFoundException($"Missing {SET_AUDIENCE} parameter.");
			string audience = (string)value;

			IEnumerable<Claim> claims = null;
			DateTime? notBefore = null;
			DateTime? expires = null;
			SigningCredentials signingCredentials = null;
			if (settings.TryGetValue(SET_CLAIMS, out value)) claims = (IEnumerable<Claim>)value;
			if (settings.TryGetValue(SET_NOT_BEFORE, out value)) notBefore = (DateTime?)value;
			if (settings.TryGetValue(SET_EXPIRES, out value)) expires = (DateTime?)value;
			if (settings.TryGetValue(SET_SIGNING_CREDENTIALS, out value)) signingCredentials = (SigningCredentials)value;
			
			ISet<Claim> claimsSet = new HashSet<Claim>(ComparisonComparer.FromEqualityComparison<Claim>((a, b) => string.Equals(a?.Type, b?.Type, StringComparison.OrdinalIgnoreCase)));
			claimsSet.Add(new Claim(JwtRegisteredClaimNames.Sub, subject));
			claimsSet.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
			claims?.ForEach(e => claimsSet.Add(e));
			return new JwtSecurityToken(issuer, audience, claimsSet, notBefore, expires, signingCredentials);
		}
	}
}
