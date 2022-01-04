using System;
using System.Text.RegularExpressions;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix;

[Serializable]
public sealed class Version : ICloneable, IComparable, IComparable<Version>, IEquatable<Version>
{
	private static readonly Regex __rgxComponents = new Regex(@"^v?(?<major>\d+)(?:\.(?<minor>\d+)(?:\.(?<build>\d+))?(?:\.(?<revision>\d+))?)?(?:\s?(?:service pack\s|sp)(?<sp>\d+))?$", RegexHelper.OPTIONS_I);

	public Version()
		: this(0, 0, 0, 0, 0)
	{
	}

	public Version(int major, int minor)
		: this(major, minor, 0, 0, 0)
	{
	}

	public Version(int major, int minor, int build)
		: this(major, minor, build, 0, 0)
	{
	}

	public Version(int major, int minor, int build, int revision)
		: this(major, minor, build, revision, 0)
	{
	}

	public Version(int major, int minor, int build, int revision, int servicePack)
	{
		if (major < 0) throw new ArgumentOutOfRangeException(nameof(major));
		if (minor < 0) throw new ArgumentOutOfRangeException(nameof(minor));
		if (build < 0) throw new ArgumentOutOfRangeException(nameof(build));
		if (revision < 0) throw new ArgumentOutOfRangeException(nameof(revision));
		if (servicePack < 0) throw new ArgumentOutOfRangeException(nameof(servicePack));
		Major = major;
		Minor = minor;
		Build = build;
		Revision = revision;
		ServicePack = servicePack;
	}

	public Version(string version)
	{
		Version v = Parse(version);
		Major = v.Major;
		Minor = v.Minor;
		Build = v.Build;
		Revision = v.Revision;
		ServicePack = v.ServicePack;
	}

	public int Major { get; }

	public int Minor { get; }

	public int Build { get; }

	public int Revision { get; }

	public int ServicePack { get; }

	public short MajorRevision => (short)(Revision >> 16);

	public short MinorRevision => (short)(Revision & 0xFFFF);

	public object Clone() { return new Version(Major, Minor, Build, Revision, ServicePack); }

	public int CompareTo(object version)
	{
		return version == null
					? 1
					: CompareTo(version as Version);
	}

	public int CompareTo(Version value)
	{
		if (value == null) return 1;
		if (Major != value.Major) return Major > value.Major ? 1 : -1;
		if (Minor != value.Minor) return Minor > value.Minor ? 1 : -1;
		if (Build != value.Build) return Build > value.Build ? 1 : -1;
		if (Revision != value.Revision) return Revision > value.Revision ? 1 : -1;
		if (ServicePack != value.ServicePack) return ServicePack > value.ServicePack ? 1 : -1;
		return 0;
	}

	public override bool Equals(object obj) { return Equals(obj as Version); }

	public bool Equals(Version obj)
	{
		return obj != null &&
				(
					ReferenceEquals(this, obj) || Major == obj.Major && Minor == obj.Minor && Build == obj.Build && Revision == obj.Revision && ServicePack == obj.ServicePack
				);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hash = 397;
			hash = (hash * 397) ^ ((Major & 0x0000000F) << 28);
			hash = (hash * 397) ^ ((Minor & 0x000000FF) << 20);
			hash = (hash * 397) ^ ((Build & 0x000000FF) << 12);
			hash = (hash * 397) ^ (Revision & 0x00000FFF);
			hash = (hash * 397) ^ (ServicePack & 0x0000FFFF);
			return hash;
		}
	}

	[NotNull]
	public override string ToString() { return ToString(-1); }

	[NotNull]
	public string ToString(int fieldCount)
	{
		fieldCount = fieldCount.Within(-1, 5);

		switch (fieldCount)
		{
			case 0:
				return string.Empty;
			case 1:
				return Major.ToString();
			case 2:
				return $"{Major}.{Minor}";
			case 3:
				return $"{Major}.{Minor}.{Build}";
			case 4:
				return $"{Major}.{Minor}.{Build}.{Revision}";
			default:
				if (ServicePack < 1) return $"{Major}.{Minor}.{Build}.{Revision}";
				return $"{Major}.{Minor}.{Build}.{Revision} SP{ServicePack}";
		}
	}

	public static Version Parse(string input) { return ParseVersion(input); }

	public static bool TryParse(string input, out Version result)
	{
		result = ParseVersion(input, true);
		return result != null;
	}

	public static bool operator ==(Version v1, Version v2)
	{
		if (ReferenceEquals(v1, null))
		{
			return ReferenceEquals(v2, null);
		}

		return v1.Equals(v2);
	}

	public static bool operator !=(Version v1, Version v2) { return !(v1 == v2); }

	public static bool operator <([NotNull] Version v1, Version v2)
	{
		if (v1 == null) throw new ArgumentNullException(nameof(v1));
		return v1.CompareTo(v2) < 0;
	}

	public static bool operator <=([NotNull] Version v1, Version v2)
	{
		if (v1 == null) throw new ArgumentNullException(nameof(v1));
		return v1.CompareTo(v2) <= 0;
	}

	public static bool operator >(Version v1, Version v2) { return v2 < v1; }

	public static bool operator >=(Version v1, Version v2) { return v2 <= v1; }

	private static Version ParseVersion(string input, bool silent = false)
	{
		input = input?.Trim();

		if (string.IsNullOrEmpty(input))
		{
			if (!silent) throw new ArgumentNullException(nameof(input));
			return null;
		}

		Match match = __rgxComponents.Match(input);

		if (!match.Success)
		{
			if (!silent) throw new FormatException("Version string is not in the correct format.");
			return null;
		}

		return new Version(match.Groups["major"].Value.To(0),
							match.Groups["minor"].Value.To(0),
							match.Groups["build"].Value.To(0),
							match.Groups["revision"].Value.To(0),
							match.Groups["sp"].Value.To(0)
						);
	}
}