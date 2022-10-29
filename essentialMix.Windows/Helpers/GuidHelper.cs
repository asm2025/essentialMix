using System;
using System.Runtime.InteropServices;

namespace essentialMix.Windows.Helpers;

public static class GuidHelper
{
    public static Guid NewSequential()
    {
        int result = Win32.UuidCreateSequential(out Guid guid);
        if (result is not ResultCom.S_OK and not ResultWin32.RPC_S_UUID_LOCAL_ONLY) throw new COMException("UuidCreateSequential call failed", result);
        return guid;
    }
}