using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

namespace essentialMix.Windows.IO;

[DebuggerDisplay("Length = {Length}")]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public class ConsumerPipe : PipeStream
{
	private static readonly PropertyInfo __stateProp;

	private readonly SafePipeHandle _serverHandle;

	private CancellationTokenSource _cts;
	private IDisposable _tokenRegistration;

	private volatile int _isCompleted;

	static ConsumerPipe()
	{
		__stateProp = typeof(PipeStream).GetProperty("State", Constants.BF_NON_PUBLIC_INSTANCE);
	}

	[SecuritySafeCritical]
	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public ConsumerPipe(string serverHandle, CancellationToken token = default(CancellationToken))
		: this(serverHandle.To(-1L), token)
	{
	}

	[SecuritySafeCritical]
	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public ConsumerPipe(long serverHandle, CancellationToken token = default(CancellationToken))
		: this((IntPtr)serverHandle, token)
	{
	}

	[SecuritySafeCritical]
	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public ConsumerPipe(IntPtr serverHandle, CancellationToken token = default(CancellationToken))
		: this(new SafePipeHandle(serverHandle, true), token)
	{
	}

	[SecuritySafeCritical]
	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public ConsumerPipe([NotNull] SafePipeHandle serverHandle, CancellationToken token = default(CancellationToken))
		: base(PipeDirection.In, 0)
	{
		if (!serverHandle.IsInvalid) throw new ArgumentException("Invalid server handle.", nameof(serverHandle));
		if (Win32.GetFileType(serverHandle) != FileType.Pipe) throw new IOException("Invalid pipe handle.");
		InitializeHandle(serverHandle, true, false);
		_serverHandle = serverHandle;
		InitializeToken(token);
		/*
		* this is an internal property, Don't ask why.
		* The value 1 = System.IO.Pipes.PipeState.Connected
		* is set like so because it's internal. Don't ask why!
		*/
		__stateProp.SetValue(this, 1);
	}

	~ConsumerPipe()
	{
		Dispose(false);
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Stop();
			ReleaseToken();
			IsDisposed = true;
		}
		base.Dispose(disposing);
	}

	/// <inheritdoc />
	public override PipeTransmissionMode TransmissionMode
	{
		[SecurityCritical]
		get => PipeTransmissionMode.Byte;
	}

	/// <inheritdoc />
	public override PipeTransmissionMode ReadMode
	{
		[SecurityCritical]
		set
		{
			CheckPipePropertyOperations();

			switch (value)
			{
				case PipeTransmissionMode.Byte:
					break;
				case PipeTransmissionMode.Message:
					throw new NotSupportedException();
				default:
					throw new ArgumentOutOfRangeException(nameof(value), value, null);
			}
		}
	}

	public CancellationToken Token { get; private set; }

	public bool IsCompleted
	{
		get
		{
			// ensure we have the latest value
			Thread.MemoryBarrier();
			return _isCompleted != 0 || _serverHandle.IsInvalid || _serverHandle.IsClosed;
		}
		protected set => Interlocked.CompareExchange(ref _isCompleted, value
																			? 1
																			: 0, _isCompleted);
	}

	public bool IsEmpty => Length == 0L;

	protected bool IsDisposed { get; private set; }

	public void InitializeToken(CancellationToken token)
	{
		ThrowIfDisposed();
		if (_cts != null) Interlocked.Exchange(ref _cts, null);
		ObjectHelper.Dispose(ref _tokenRegistration);
		Interlocked.CompareExchange(ref _cts, new CancellationTokenSource(), null);
		if (token.CanBeCanceled) _tokenRegistration = token.Register(() => _cts.CancelIfNotDisposed(), false);
		Token = _cts.Token;
	}

	protected void ReleaseToken()
	{
		if (_cts == null) return;
		Interlocked.Exchange(ref _cts, null);
		ObjectHelper.Dispose(ref _tokenRegistration);
		Token = CancellationToken.None;
	}

	protected void Cancel()
	{
		_cts.CancelIfNotDisposed();
	}

	public void Complete()
	{
		ThrowIfDisposed();
		IsCompleted = true;
	}

	public void Stop()
	{
		Complete();
		Cancel();
	}

	/// <inheritdoc />
	public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		ThrowIfDisposed();
		cancellationToken.ThrowIfCancellationRequested();

		IDisposable tokenReg = null;
		int read;

		try
		{
			if (cancellationToken.CanBeCanceled) tokenReg = cancellationToken.Register(Cancel);
			CancellationToken token = Token.CanBeCanceled
										? Token
										: cancellationToken;
			read = await base.ReadAsync(buffer, offset, count, token);
		}
		finally
		{
			ObjectHelper.Dispose(ref tokenReg);
		}

		return read;
	}

	/// <inheritdoc />
	public override void Write(byte[] buffer, int offset, int count)
	{
		if (IsDisposed || Token.IsCancellationRequested || IsCompleted) return;
		base.Write(buffer, offset, count);
	}

	/// <inheritdoc />
	public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		IDisposable tokenReg = null;

		try
		{
			if (cancellationToken.CanBeCanceled) tokenReg = cancellationToken.Register(Cancel);
			CancellationToken token = Token.CanBeCanceled
										? Token
										: cancellationToken;

			await base.WriteAsync(buffer, offset, count, token);
		}
		finally
		{
			ObjectHelper.Dispose(ref tokenReg);
		}
	}

	protected void ThrowIfDisposed()
	{
		if (!IsDisposed) return;
		throw new ObjectDisposedException(GetType().FullName);
	}

	[NotNull]
	public static PipeStream CreateProducer()
	{
		return new AnonymousPipeServerStream(PipeDirection.Out);
	}
}