using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using asm.Patterns.Object;
using JetBrains.Annotations;

namespace asm.Other.MarcGravell.Channel
{
	/// <summary>
	/// Wraps a reference to an object that potentially lives in another domain.
	/// This ensures that cross-domain calls are explicit in the source code, and
	/// allows for a transport other than .NET Remoting (e.g., <see cref="Channel" />).
	/// </summary>
	public class ChannelProxy<TRemote> : Disposable, IChannelProxy, IDisposable
		where TRemote : class
	{
		private readonly object _locker = new object();
		private readonly Type _actualObjectType;

		private Action _onDisconnect;

		public ChannelProxy(TRemote instance)
		{
			Instance = instance;
			ObjectType = _actualObjectType ?? (Instance?.GetType() ?? typeof(TRemote));
		}

		// Called via reflection:
		internal ChannelProxy(TRemote instance, byte domainAddress)
		{
			Instance = instance;
			DomainAddress = domainAddress;
			ObjectType = _actualObjectType ?? (Instance?.GetType() ?? typeof(TRemote));
		}

		// Called via reflection:
		internal ChannelProxy([NotNull] IChannelProxy conversionSource, Action onDisconnect, Type actualObjectType)
		{
			Instance = (TRemote)conversionSource.InstanceObject;
			Channel = conversionSource.Channel;
			ObjectID = conversionSource.ObjectID;
			DomainAddress = conversionSource.DomainAddress;
			_onDisconnect = onDisconnect;
			_actualObjectType = actualObjectType;
			ObjectType = _actualObjectType ?? (Instance?.GetType() ?? typeof(TRemote));
		}

		// Called via reflection:
		internal ChannelProxy([NotNull] Channel channel, int objectID, byte domainAddress, Type actualInstanceType)
		{
			Channel = channel;
			ObjectID = objectID;
			DomainAddress = domainAddress;
			_actualObjectType = actualInstanceType;
		}

		public static implicit operator TRemote(ChannelProxy<TRemote> channelProxy) { return channelProxy?.Instance; }

		/// <summary>
		/// Any reference-type object can be implicitly converted to a proxy. The proxy will become connected
		/// automatically when it's serialized during a remote method call.
		/// </summary>
		public static explicit operator ChannelProxy<TRemote>(TRemote instance)
		{
			return instance == null
						? null
						: new ChannelProxy<TRemote>(instance);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				try { Disconnect(); }
				catch (ObjectDisposedException) { }
			}

			base.Dispose(disposing);
		}

		/// <summary>The object being remoted. This is populated only on the local side.</summary>
		public TRemote Instance { get; private set; }

		/// <summary>This is populated if the proxy was obtained via Channel instead of Remoting.</summary>
		public Channel Channel { get; private set; }

		/// <summary>
		/// This is populated if the proxy was obtained via Channel instead of Remoting. It uniquely identifies the
		/// object within the channel if the object is connected (has a presence in the other domain).
		/// </summary>
		public int? ObjectID { get; private set; }

		/// <summary>Identifies the domain that owns the local instance, when Channel is in use.</summary>
		public byte DomainAddress { get; private set; }

		public bool IsDisconnected => Instance == null && Channel == null;

		/// <summary>The real type of the object being proxied. This may be a subclass or derived implementation of TRemote.</summary>
		public Type ObjectType { get; }

		object IChannelProxy.InstanceObject => Instance;

		public void AssertRemote()
		{
			if (Instance == null) return;
			throw new InvalidOperationException($"Object {Instance.GetType().Name} is not remote");
		}

		/// <summary>Runs a (void) method on the object being proxied. This works on both the local and remote side.</summary>
		public Task Run([NotNull] Expression<Action<TRemote>> remoteMethod)
		{
			TRemote li = Instance;

			if (li != null)
			{
				try
				{
					Func<TRemote, object> fastEval = Channel.FastEvalExpr<TRemote, object>(remoteMethod.Body);
					if (fastEval != null) fastEval(li);
					else remoteMethod.Compile()(li);
					return Task.FromResult(false);
				}
				catch (Exception ex)
				{
					return Task.FromException(ex);
				}
			}

			return SendMethodCall<object>(remoteMethod.Body, false);
		}

		/// <summary>
		/// Runs a (void) method on the object being proxied. This works on both the local and remote side.
		/// Use this overload for methods on the other domain that are themselves asynchronous.
		/// </summary>
		public Task Run(Expression<Func<TRemote, Task>> remoteMethod)
		{
			TRemote li = Instance;

			if (li != null)
			{
				try
				{
					Func<TRemote, Task> fastEval = Channel.FastEvalExpr<TRemote, Task>(remoteMethod.Body);
					if (fastEval != null) return fastEval(li);
					return remoteMethod.Compile()(li);
				}
				catch (Exception ex)
				{
					return Task.FromException(ex);
				}
			}

			return SendMethodCall<object>(remoteMethod.Body, true);
		}

		/// <summary>Runs a non-void method on the object being proxied. This works on both the local and remote side.</summary>
		public Task<TResult> Eval<TResult>([NotNull] Expression<Func<TRemote, TResult>> remoteMethod)
		{
			TRemote li = Instance;

			if (li != null)
			{
				try
				{
					Func<TRemote, TResult> fastEval = Channel.FastEvalExpr<TRemote, TResult>(remoteMethod.Body);
					if (fastEval != null) return Task.FromResult(fastEval(li));
					return Task.FromResult(remoteMethod.Compile()(li));
				}
				catch (Exception ex)
				{
					return Task.FromException<TResult>(ex);
				}
			}

			return SendMethodCall<TResult>(remoteMethod.Body, false);
		}

		/// <summary>
		/// Runs a non-void method on the object being proxied. This works on both the local and remote side.
		/// Use this overload for methods on the other domain that are themselves asynchronous.
		/// </summary>
		public Task<TResult> Eval<TResult>(Expression<Func<TRemote, Task<TResult>>> remoteMethod)
		{
			TRemote li = Instance;

			if (li != null)
			{
				try
				{
					Func<TRemote, Task<TResult>> fastEval = Channel.FastEvalExpr<TRemote, Task<TResult>>(remoteMethod.Body);
					return fastEval != null ? fastEval(li) : remoteMethod.Compile()(li);
				}
				catch (Exception ex)
				{
					return Task.FromException<TResult>(ex);
				}
			}

			return SendMethodCall<TResult>(remoteMethod.Body, true);
		}

		/// <summary>This is useful when this.ObjectType is a subclass or derivation of TRemote.</summary>
		[NotNull]
		public ChannelProxy<TNew> CastTo<TNew>()
			where TNew : class, TRemote
		{
			if (!typeof(TNew).IsAssignableFrom(ObjectType))
				throw new InvalidCastException($"Type \'{ObjectType.FullName}\' cannot be cast to \'{typeof(TNew).FullName}\'.");

			lock (_locker)
			{
				return new ChannelProxy<TNew>(this, _onDisconnect, _actualObjectType);
			}
		}

		public void Disconnect()
		{
			Action onDisconnect;

			lock(_locker)
			{
				onDisconnect = _onDisconnect;
				_onDisconnect = null;
			}

			onDisconnect?.Invoke();

			// If the remote reference drops away, we should ensure that it gets release on the other domain as well:

			lock(_locker)
			{
				if (Channel == null || Instance != null || ObjectID == null)
					Instance = null;
				else
					Channel.InternalDeactivate(ObjectID.Value);

				Channel = null;
				ObjectID = null;
			}
		}

		void IChannelProxy.RegisterLocal([NotNull] Channel channel, int? objectID, Action onDisconnect)
		{
			// This is called by Channel to connect/register the proxy.
			lock(_locker)
			{
				Channel = channel;
				ObjectID = objectID;
				DomainAddress = channel.DomainAddress;
				_onDisconnect = onDisconnect;
			}
		}

		private Task<TResult> SendMethodCall<TResult>([NotNull] Expression expressionBody, bool awaitRemoteTask)
		{
			Channel channel;
			int? objectID;

			lock(_locker)
			{
				if (Channel == null)
					return Task.FromException<TResult>(new InvalidOperationException("Channel has been disposed on Proxy<" + typeof(TRemote).Name + "> " + expressionBody));

				channel = Channel;
				objectID = ObjectID;
			}

			return channel.SendMethodCall<TResult>(expressionBody, objectID ?? 0, awaitRemoteTask);
		}
	}
}