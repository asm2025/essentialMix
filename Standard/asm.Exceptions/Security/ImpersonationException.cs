using System;
using System.ComponentModel;
using JetBrains.Annotations;

namespace asm.Exceptions.Security
{
	public class ImpersonationException : Exception
	{
		private readonly Win32Exception _win32Exception;

		/// <inheritdoc />
		public ImpersonationException([NotNull] Win32Exception win32Exception)
			: base(win32Exception.Message, win32Exception)
		{
			_win32Exception = win32Exception;
		}

		public int ErrorCode => _win32Exception.ErrorCode;

		public int NativeErrorCode => _win32Exception.NativeErrorCode;
	}
}
