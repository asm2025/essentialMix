using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using essentialMix.Exceptions.Network;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;

namespace essentialMix.Network;

public class TelnetConnection : Disposable
{
	private const int PORT_DEF = 23;
	private const int TIMEOUT_DEF = 30000;
	private const int TIMEOUT_MAX = 600000;
	private const int TTL_DEF = 60;
	private const string FMT_LOGIN = "Username: {0}\r\nSecret: {1}\r\nActionID: 1\r\nEvents: off";

	private static readonly Regex __loginSuccess = new Regex(@"Message:\s+Authentication accepted", RegexHelper.OPTIONS_I | RegexOptions.Singleline);
	private static readonly TimeSpan __timeBetweenRead = TimeSpan.FromSeconds(1);
	private static readonly TimeSpan __timeBetweenActions = TimeSpan.FromSeconds(2);

	private Socket _client;

	private DateTime _lastSent;
	private string _host;
	private int _port;
	private int _timeout;
	private int _bufferSize;
	private short _ttl;
	private Encoding _encoding;

	public TelnetConnection([NotNull] string host)
		: this(host, PORT_DEF, TIMEOUT_DEF)
	{
	}

	public TelnetConnection([NotNull] string host, int port)
		: this(host, port, TIMEOUT_DEF)
	{
	}

	public TelnetConnection([NotNull] string host, int port, int timeout)
	{
		_lastSent = DateTime.Now - __timeBetweenActions;
		_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		Encoding = Encoding.ASCII;
		Host = host;
		Port = port;
		Timeout = timeout;
		BufferSize = Constants.GetBufferKB(8);
		Ttl = TTL_DEF;
	}

	[NotNull]
	public string Host
	{
		get
		{
			ThrowIfDisposed();
			return _host;
		}
		set
		{
			ThrowIfDisposed();
			if (_host == value) return;
			Disconnect();
			_host = value;
		}
	}

	public int Port
	{
		get
		{
			ThrowIfDisposed();
			return _port;
		}
		set
		{
			ThrowIfDisposed();
			if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
			if (_port == value) return;
			Disconnect();
			_port = value;
		}
	}

	public int Timeout
	{
		get
		{
			ThrowIfDisposed();
			return _timeout;
		}
		set
		{
			ThrowIfDisposed();
			if (_timeout == value) return;
			_timeout = value;
			_client.SendTimeout = _client.ReceiveTimeout = _timeout;
		}
	}

	public int BufferSize
	{
		get
		{
			ThrowIfDisposed();
			return _bufferSize;
		}
		set
		{
			ThrowIfDisposed();
			if (value < 1) throw new ArgumentOutOfRangeException(nameof(value));
			_bufferSize = value;
			_client.SendBufferSize = _client.ReceiveBufferSize = _bufferSize;
		}
	}

	public short Ttl
	{
		get
		{
			ThrowIfDisposed();
			return _ttl;
		}
		set
		{
			ThrowIfDisposed();
			if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
			_ttl = value;
			_client.Ttl = _ttl;
		}
	}

	public bool Blocking
	{
		get
		{
			ThrowIfDisposed();
			return _client.Blocking;
		}
		set
		{
			ThrowIfDisposed();
			_client.Blocking = value;
		}
	}

	public bool NoDelay
	{
		get
		{
			ThrowIfDisposed();
			return _client.NoDelay;
		}
		set
		{
			ThrowIfDisposed();
			_client.NoDelay = value;
		}
	}

	public bool Remember
	{
		get
		{
			ThrowIfDisposed();
			return _client.DontFragment;
		}
		set
		{
			ThrowIfDisposed();
			_client.DontFragment = value;
		}
	}

	public bool DualMode
	{
		get
		{
			ThrowIfDisposed();
			return _client.DualMode;
		}
		set
		{
			ThrowIfDisposed();
			_client.DualMode = value;
		}
	}

	[NotNull]
	public Encoding Encoding
	{
		get
		{
			ThrowIfDisposed();
			return _encoding;
		}
		set
		{
			ThrowIfDisposed();
			_encoding = value;
		}
	}

	public bool IsConnected
	{
		get
		{
			ThrowIfDisposed();
			return _client.Connected;
		}
	}

	public bool IsBound
	{
		get
		{
			ThrowIfDisposed();
			return _client.IsBound;
		}
	}

	public int Available
	{
		get
		{
			ThrowIfDisposed();
			return _client.Available;
		}
	}

	public virtual void Connect()
	{
		ThrowIfDisposed();
		if (IsConnected) return;
		_client.Connect(Host, Port);
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
		_client.Disconnect(true);
		_lastSent = DateTime.Now - __timeBetweenActions;
	}

	public virtual bool Login(string username, string password)
	{
		ThrowIfDisposed();
		if (!TryConnect()) return false;

		string response = SendAndRead("Login", string.Format(FMT_LOGIN, username, password));
		return response != null && __loginSuccess.IsMatch(response);
	}

	public bool Send([NotNull] string action, [NotNull] string args)
	{
		ThrowIfDisposed();
		return action.Length != 0 && Send($"Action: {action}\r\n{args}");
	}

	public bool Send([NotNull] string value)
	{
		ThrowIfDisposed();
		if (!IsConnected) throw new NotConnectedException();
		if (value.Length == 0) return false;

		TimeSpan dif = _lastSent.Elapsed();
		if (dif < __timeBetweenActions) TimeSpanHelper.WasteTime(dif);

		int s = _client.Send(Encoding.GetBytes($"{value}\r\n\r\n"));
		_lastSent = DateTime.Now;
		if (__timeBetweenActions > TimeSpan.Zero) TimeSpanHelper.WasteTime(__timeBetweenActions);
		return s > 0;
	}

	public string SendAndRead([NotNull] string action, [NotNull] string args, SocketFlags flags = SocketFlags.None)
	{
		ThrowIfDisposed();
		return action.Length == 0 ? string.Empty : SendAndRead($"Action: {action}\r\n{args}", flags);
	}

	public string SendAndRead([NotNull] string value, SocketFlags flags = SocketFlags.None)
	{
		ThrowIfDisposed();
		if (!IsConnected) throw new NotConnectedException();
		if (value.Length == 0) return string.Empty;
		ConsumeOutput();
		return !Send(value) ? null : Read(flags);
	}

	public void ConsumeOutput()
	{
		ThrowIfDisposed();
		if (!IsConnected || Available == 0) return;

		int len = Math.Max(BufferSize, Available);
		byte[] buffer = new byte[len];

		while (Read(buffer) > 0)
		{
		}
	}

	public string Read(SocketFlags flags = SocketFlags.None)
	{
		ThrowIfDisposed();
		return Read(out SocketError _, flags);
	}

	public string Read(out SocketError errorCode, SocketFlags flags = SocketFlags.None)
	{
		ThrowIfDisposed();
		if (!IsConnected) throw new NotConnectedException();

		errorCode = SocketError.Success;
		if (Available == 0) return null;

		byte[] buffer = new byte[BufferSize];
		StringBuilder sb = new StringBuilder(buffer.Length);
		DateTime timeout = Timeout > 0 ? DateTime.Now.AddMilliseconds(Timeout) : DateTime.Now.AddMilliseconds(TIMEOUT_MAX);

		while (Available > 0 && errorCode == SocketError.Success && DateTime.Now < timeout)
		{
			int r = Read(buffer, out errorCode, flags);
			if (r == 0 || errorCode != SocketError.Success) break;
			sb.Append(Encoding.GetString(buffer, 0, r));
		}

		return errorCode != SocketError.Success || sb.Length == 0 ? null : sb.ToString();
	}

	public string Read(int length, SocketFlags flags = SocketFlags.None)
	{
		ThrowIfDisposed();
		return Read(length, out SocketError _, flags);
	}

	public string Read(int length, out SocketError errorCode, SocketFlags flags = SocketFlags.None)
	{
		ThrowIfDisposed();
		if (length < 1) throw new ArgumentOutOfRangeException(nameof(length));
		if (!IsConnected) throw new NotConnectedException();

		errorCode = SocketError.Success;
		if (Available == 0) return null;

		byte[] buffer = new byte[length];
		StringBuilder sb = new StringBuilder(buffer.Length);
		DateTime timeout = Timeout > 0 ? DateTime.Now.AddMilliseconds(Timeout) : DateTime.Now.AddMilliseconds(TIMEOUT_MAX);

		while (Available > 0 && errorCode == SocketError.Success && sb.Length < length && DateTime.Now < timeout)
		{
			int r = Read(buffer, out errorCode, flags);
			if (r == 0 || errorCode != SocketError.Success) break;
			sb.Append(Encoding.GetString(buffer, 0, r));
		}

		return errorCode != SocketError.Success || sb.Length == 0 ? null : sb.ToString();
	}

	private int Read([NotNull] byte[] buffer, SocketFlags flags = SocketFlags.None)
	{
		ThrowIfDisposed();
		return Read(buffer, out SocketError _, flags);
	}

	private int Read([NotNull] byte[] buffer, out SocketError errorCode, SocketFlags flags = SocketFlags.None)
	{
		ThrowIfDisposed();
		if (!IsConnected) throw new NotConnectedException();

		int r = _client.Receive(buffer, 0, buffer.Length, flags, out errorCode);
		if (r > 0) TimeSpanHelper.WasteTime(__timeBetweenRead);
		return r;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_client?.Disconnect(true);
			ObjectHelper.Dispose(ref _client);
		}
		base.Dispose(disposing);
	}
}