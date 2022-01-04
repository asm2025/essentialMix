using System.Security.Principal;
using essentialMix.Extensions;

namespace essentialMix.Helpers;

public static class WindowsIdentityHelper
{
	private static bool? _hasElevatedPrivileges;

	public static bool HasElevatedPrivileges
	{
		get
		{
			_hasElevatedPrivileges ??= GetHasElevatedPrivileges();
			return _hasElevatedPrivileges.Value;
		}
	}

	private static bool GetHasElevatedPrivileges()
	{
		WindowsIdentity identity = null;

		try
		{
			identity = WindowsIdentity.GetCurrent();
			return identity.HasElevatedPrivileges();
		}
		finally
		{
			ObjectHelper.Dispose(ref identity);
		}
	}
}