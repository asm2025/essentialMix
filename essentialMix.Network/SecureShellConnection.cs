using System;
using essentialMix.Exceptions.Network;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;
using Renci.SshNet;

namespace essentialMix.Network;

public class SecureShellConnection : Disposable
{
	private const int PORT_DEF = 22;
	private const int TIMEOUT_DEF = 30000;

	private SshClient _client;

	public SecureShellConnection([NotNull] string host)
		: this(host, PORT_DEF, null, null, TIMEOUT_DEF)
	{
	}

	public SecureShellConnection([NotNull] string host, int port)
		: this(host, port, null, null, TIMEOUT_DEF)
	{
	}

	public SecureShellConnection([NotNull] string host, int port, string username, string password)
		: this(host, port, username, password, TIMEOUT_DEF)
	{
	}

	public SecureShellConnection([NotNull] string host, int port, string username, string password, int timeout)
	{
		_client = new SshClient(host, port, username, password)
		{
			ConnectionInfo =
			{
				Timeout = TimeSpan.FromMilliseconds(timeout.NotBelow(0))
			}
		};
	}

	public TimeSpan Ttl
	{
		get => _client.KeepAliveInterval;
		set => _client.KeepAliveInterval = value;
	}

	public ConnectionInfo ConnectionInfo
	{
		get
		{
			ThrowIfDisposed();
			return _client.ConnectionInfo;
		}
	}

	public bool IsConnected
	{
		get
		{
			ThrowIfDisposed();
			return _client.IsConnected;
		}
	}

	public virtual void Connect()
	{
		ThrowIfDisposed();
		if (IsConnected) return;
		_client.Connect();
	}

	public virtual bool TryConnect()
	{
		ThrowIfDisposed();

		try
		{
			Connect();
			return true;
		}
		catch
		{
			return false;
		}
	}

	public virtual void Disconnect()
	{
		ThrowIfDisposed();
		if (!IsConnected) return;
		_client.Disconnect();
	}

	public virtual SshCommand Execute([NotNull] string commandLine)
	{
		ThrowIfDisposed();
		if (string.IsNullOrWhiteSpace(commandLine)) throw new ArgumentNullException(nameof(commandLine));
		if (!IsConnected) throw new NotConnectedException();
		return _client.RunCommand(commandLine);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_client?.Disconnect();
			ObjectHelper.Dispose(ref _client);
		}
		base.Dispose(disposing);
	}
}