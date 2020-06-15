using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using asm.Extensions;
using asm.Helpers;
using asm.Patterns.Object;
using JetBrains.Annotations;

namespace asm.Other.MarcGravell.Channel
{
	public sealed class Channel : Disposable, IDisposable
	{
		private enum FastTypeCode : byte
		{
			Null,
			False,
			True,
			Byte,
			Char,
			String,
			Int32,
			Proxy,
			Other
		}

		private static int _lastMessageID, _lastObjectID;

		private readonly object _proxiesLock = new object();

		private readonly Module _module; // The module for which serialization is optimized.
		private readonly IDictionary<int, IChannelProxy> _proxiesByID = new Dictionary<int, IChannelProxy>();
		private readonly IDictionary<object, IChannelProxy> _proxiesByObject = new Dictionary<object, IChannelProxy>();
		private readonly IDictionary<int, Action<ChannelMessageType, object>> _pendingReplies = new Dictionary<int, Action<ChannelMessageType, object>>();

		private InPipe _inPipe;
		private OutPipe _outPipe;
		private int _messagesReceived;

		/// <inheritdoc />
		public Channel(string name, bool isOwner, Module module)
		{
			_module = module; // Types belonging to this module will serialize faster
			DomainAddress = (byte)(isOwner ? 1 : 2);
			_outPipe = new OutPipe(name + (isOwner ? ".A" : ".B"), isOwner);
			_inPipe = new InPipe(name + (isOwner ? ".B" : ".A"), isOwner, OnMessageReceived);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				lock(_proxiesLock)
				{
					_proxiesByID.Clear();
					_proxiesByObject.Clear();
				}

				ObjectHelper.Dispose(ref _inPipe);
				ObjectHelper.Dispose(ref _outPipe);
			}

			base.Dispose(disposing);
		}

		public int MessagesReceived => _messagesReceived;

		public byte DomainAddress { get; }

		/// <summary>
		/// Instantiates an object remotely. To release it, you can either call Disconnect on the proxy returned
		/// or wait for its finalizer to do the same.s
		/// </summary>
		public Task<ChannelProxy<TRemote>> Activate<TRemote>() 
			where TRemote : class
		{
			ThrowIfDisposedOrDisposing();

			int messageNumber = Interlocked.Increment(ref _lastMessageID);

			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(ms))
				{
					writer.Write((byte)ChannelMessageType.Activation);
					writer.Write(messageNumber);
					SerializeType(writer, typeof(TRemote));
				}

				Task<ChannelProxy<TRemote>> task = GetResponseFuture<ChannelProxy<TRemote>>(messageNumber);
				_outPipe.Write(ms.ToArray());
				return task;
			}
		}

		internal void InternalDeactivate(int objectID)
		{
			if (IsDisposed) return;

			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(ms))
				{
					writer.Write((byte)ChannelMessageType.Deactivation);
					writer.Write(objectID);
					writer.Flush();
				}

				if (IsDisposed) return;
				_outPipe.Write(ms.ToArray());
			}
		}

		internal Task<TResult> SendMethodCall<TResult>([NotNull] Expression expressionBody, int objectID, bool awaitRemoteTask)
		{
			ThrowIfDisposedOrDisposing();

			int messageNumber = Interlocked.Increment(ref _lastMessageID);
			byte[] payload = SerializeMethodCall(expressionBody, messageNumber, objectID, awaitRemoteTask);
			Task<TResult> task = GetResponseFuture<TResult>(messageNumber);
			_outPipe.Write(payload);
			return task;
		}

		private IChannelProxy FindProxy(int objectID)
		{
			IChannelProxy proxy;

			lock(_proxiesLock)
			{
				_proxiesByID.TryGetValue(objectID, out proxy);
			}

			return proxy;
		}

		private IChannelProxy RegisterLocalProxy([NotNull] IChannelProxy proxy)
		{
			lock(_proxiesLock)
			{
				// Avoid multiple identities for the same object:
				bool alreadyThere = _proxiesByObject.TryGetValue(proxy.InstanceObject, out IChannelProxy existingProxy);
				if (alreadyThere) return existingProxy;

				int objectID = Interlocked.Increment(ref _lastObjectID);
				proxy.RegisterLocal(this, objectID, () => UnregisterLocalProxy(proxy, objectID));

				_proxiesByID[objectID] = proxy;
				_proxiesByObject[proxy] = proxy;
				return proxy;
			}
		}

		private void UnregisterLocalProxy([NotNull] IChannelProxy proxy, int objectID)
		{
			lock(_proxiesLock)
			{
				_proxiesByID.Remove(objectID);
				_proxiesByObject.Remove(proxy.InstanceObject);
			}
		}

		private Task<T> GetResponseFuture<T>(int messageNumber)
		{
			TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

			lock(_pendingReplies)
			{
				_pendingReplies.Add(messageNumber, (msgType, value) =>
				{
					if (msgType == ChannelMessageType.ReturnValue)
						tcs.SetResult((T)value);
					else
					{
						Exception ex = (Exception)value;
						MethodInfo preserveStackTrace = typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);
						if (preserveStackTrace != null) preserveStackTrace.Invoke(ex, null);
						tcs.SetException(ex);
					}
				});
			}

			return tcs.Task;
		}

		private void OnMessageReceived(byte[] data)
		{
			Interlocked.Increment(ref _messagesReceived);
			if (IsDisposed) return;

			using (MemoryStream ms = new MemoryStream(data))
			{
				using (BinaryReader reader = new BinaryReader(ms))
				{
					ChannelMessageType messageType = (ChannelMessageType)reader.ReadByte();

					switch (messageType)
					{
						case ChannelMessageType.ReturnValue:
						case ChannelMessageType.ReturnException:
							ReceiveReply(messageType, reader);
							break;
						case ChannelMessageType.MethodCall:
							ReceiveMethodCall(reader);
							break;
						case ChannelMessageType.Activation:
							ReceiveActivation(reader);
							break;
						case ChannelMessageType.Deactivation:
							ReceiveDeactivation(reader);
							break;
					}
				}
			}
		}

		private void ReceiveReply(ChannelMessageType messageType, [NotNull] BinaryReader reader)
		{
			int msgNumber = reader.ReadInt32();
			object value = DeserializeValue(reader);
			Action<ChannelMessageType, object> reply;

			lock(_pendingReplies)
			{
				if (!_pendingReplies.TryGetValue(msgNumber, out reply)) return; // Orphan reply		
				_pendingReplies.Remove(msgNumber);
			}

			reply(messageType, value);
		}

		private void ReceiveMethodCall([NotNull] BinaryReader reader)
		{
			int messageNumber = reader.ReadInt32();
			int objectID = reader.ReadInt32();
			bool awaitRemoteTask = reader.ReadBoolean();
			MethodBase method = DeserializeMethod(reader);
			Exception error = null;
			object returnValue = null;
			object[] args = null;
			IChannelProxy proxy = null;

			try
			{
				args = DeserializeArguments(reader, method.GetParameters()).ToArray();
				proxy = FindProxy(objectID);
				if (proxy == null) throw new ObjectDisposedException($"Proxy {objectID} has been disposed.");
			}
			catch (Exception ex)
			{
				error = ex;
			}

			Task.Factory.StartNew(() =>
			{
				if (error == null)
				{
					try
					{
						object instance = proxy?.InstanceObject;

						if (instance == null)
						{
							string typeInfo = proxy?.ObjectType == null ? "?" : proxy.ObjectType.FullName;
							error = new ObjectDisposedException($"Proxy {objectID} is disposed. Type: {typeInfo}");
						}
						else
							returnValue = method.Invoke(instance, args);
					}
					catch (Exception ex)
					{
						error = ex is TargetInvocationException ? ex.InnerException : ex;
					}
				}

				SendReply(messageNumber, returnValue, error, awaitRemoteTask);
			}, TaskCreationOptions.PreferFairness).ConfigureAwait();
		}

		private void ReceiveActivation([NotNull] BinaryReader reader)
		{
			int messageNumber = reader.ReadInt32();
			object instance = null;
			Exception error = null;

			try
			{
				Type type = DeserializeType(reader);
				instance = Activator.CreateInstance(type, true);
				IChannelProxy proxy = (IChannelProxy)Activator.CreateInstance(typeof(ChannelProxy<>).MakeGenericType(type), BindingFlags.Instance | BindingFlags.NonPublic,
					null, new[] {instance, DomainAddress}, null);
				instance = RegisterLocalProxy(proxy);
			}
			catch (Exception ex)
			{
				error = ex;
			}

			SendReply(messageNumber, instance, error, false);
		}

		private void SendReply(int messageNumber, object returnValue, Exception error, bool awaitRemoteTask)
		{
			if (IsDisposed) return;

			if (awaitRemoteTask)
			{
				Task returnTask = (Task)returnValue;
				// The method we're calling is itself asynchronous. Delay sending a reply until the method itself completes.
				returnTask.ContinueWith(ant => SendReply(messageNumber, ant.IsFaulted ? null : ant.GetResult(), ant.Exception, false)).ConfigureAwait();
				return;
			}

			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(ms))
				{
					writer.Write((byte)(error == null ? ChannelMessageType.ReturnValue : ChannelMessageType.ReturnException));
					writer.Write(messageNumber);
					SerializeValue(writer, error ?? returnValue);
					writer.Flush();
				}

				_outPipe.Write(ms.ToArray());
			}
		}

		private void ReceiveDeactivation([NotNull] BinaryReader reader)
		{
			int objectID = reader.ReadInt32();

			lock(_proxiesLock)
			{
				IChannelProxy proxy = FindProxy(objectID);
				if (proxy == null) return;
				proxy.Disconnect();
			}
		}

		[NotNull]
		private byte[] SerializeMethodCall([NotNull] Expression expr, int messageNumber, int objectID, bool awaitRemoteTask)
		{
			if (expr == null) throw new ArgumentNullException(nameof(expr));

			MethodInfo method;
			IEnumerable<Expression> args = new Expression[0];

			switch (expr)
			{
				case MethodCallExpression mc:
					method = mc.Method;
					args = mc.Arguments;
					break;
				case MemberExpression me when me.Member is PropertyInfo info:
					method = info.GetGetMethod();
					break;
				default:
					throw new InvalidOperationException("Only method calls and property reads can be serialized");
			}

			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(ms))
				{
					writer.Write((byte)ChannelMessageType.MethodCall);
					writer.Write(messageNumber);
					writer.Write(objectID);
					writer.Write(awaitRemoteTask);
					SerializeMethod(writer, method);
					SerializeArguments(writer, args.Select(a => GetExprValue(a, true)).ToArray());
					writer.Flush();
				}

				return ms.ToArray();
			}
		}

		private void SerializeArguments([NotNull] BinaryWriter writer, [NotNull] object[] args)
		{
			writer.Write((byte)args.Length);

			foreach (object o in args)
				SerializeValue(writer, o);
		}

		private IEnumerable<object> DeserializeArguments([NotNull] BinaryReader reader, ParameterInfo[] args)
		{
			byte objectCount = reader.ReadByte();

			for (int i = 0; i < objectCount; i++)
				yield return DeserializeValue(reader, args[i].ParameterType);
		}

		private void SerializeValue([NotNull] BinaryWriter writer, object o)
		{
			switch (o)
			{
				case null:
					writer.Write((byte)FastTypeCode.Null);
					break;
				case bool b:
					writer.Write((byte)(b ? FastTypeCode.True : FastTypeCode.False));
					break;
				case byte t:
					writer.Write((byte)FastTypeCode.Byte);
					writer.Write(t);
					break;
				case char c:
					writer.Write((byte)FastTypeCode.Char);
					writer.Write(c);
					break;
				case string s:
					writer.Write((byte)FastTypeCode.String);
					writer.Write(s);
					break;
				case int i:
					writer.Write((byte)FastTypeCode.Int32);
					writer.Write(i);
					break;
				case IChannelProxy px:
					writer.Write((byte)FastTypeCode.Proxy);
					IChannelProxy proxy = px;
					if (proxy.InstanceObject != null) proxy = RegisterLocalProxy(proxy);
					Type typeArgType = px.GetType().GetGenericArguments()[0];
					SerializeType(writer, typeArgType);
					SerializeType(writer, proxy.InstanceObject?.GetType() ?? typeArgType);
					writer.Write(proxy.ObjectID ?? 0);
					// The domain address will be zero if created via implicit conversion.
					writer.Write(proxy.DomainAddress == 0 ? DomainAddress : proxy.DomainAddress);
					break;
				default:
					writer.Write((byte)FastTypeCode.Other);
					writer.Flush();
					new BinaryFormatter().Serialize(writer.BaseStream, o);
					break;
			}
		}

		private object DeserializeValue([NotNull] BinaryReader reader, Type expectedType = null)
		{
			FastTypeCode typeCode = (FastTypeCode)reader.ReadByte();

			switch (typeCode)
			{
				case FastTypeCode.Null:
					return null;
				case FastTypeCode.False:
					return false;
				case FastTypeCode.True:
					return true;
				case FastTypeCode.Byte:
					return reader.ReadByte();
				case FastTypeCode.Char:
					return reader.ReadChar();
				case FastTypeCode.String:
					return reader.ReadString();
				case FastTypeCode.Int32:
					return reader.ReadInt32();
				case FastTypeCode.Proxy:
					Type genericType = DeserializeType(reader);
					Type actualType = DeserializeType(reader);
					int objectID = reader.ReadInt32();
					byte domainAddress = reader.ReadByte();

					if (domainAddress == DomainAddress) // We own the real object
					{
						IChannelProxy proxy = FindProxy(objectID);
						if (proxy == null)
							throw new ObjectDisposedException("Cannot deserialize type '" + genericType.Name + "' - object has been disposed");

						// Automatically unmarshal if necessary:
						if (expectedType != null && expectedType.IsInstanceOfType(proxy.InstanceObject)) return proxy.InstanceObject;

						return proxy;
					}

					// The other domain owns the object.
					Type proxyType = typeof(ChannelProxy<>).MakeGenericType(genericType);
					return Activator.CreateInstance(proxyType, BindingFlags.NonPublic | BindingFlags.Instance, null, new object[]
					{
						this,
						objectID,
						domainAddress,
						actualType
					}, null);
			}

			return new BinaryFormatter().Deserialize(reader.BaseStream);
		}

		private void SerializeType([NotNull] BinaryWriter writer, [NotNull] Type t)
		{
			if (t.Module == _module)
			{
				writer.Write((byte)1);
				writer.Write(t.MetadataToken);
			}
			else if (!string.IsNullOrEmpty(t.AssemblyQualifiedName))
			{
				writer.Write((byte)2);
				writer.Write(t.AssemblyQualifiedName);
			}
		}

		private Type DeserializeType([NotNull] BinaryReader reader)
		{
			int b = reader.ReadByte();
			return b == 1 ? _module.ResolveType(reader.ReadInt32()) : Type.GetType(reader.ReadString());
		}

		private void SerializeMethod(BinaryWriter writer, [NotNull] MethodInfo mi)
		{
			if (mi.Module == _module)
			{
				writer.Write((byte)1);
				writer.Write(mi.MetadataToken);
			}
			else if (mi.DeclaringType != null && !string.IsNullOrEmpty(mi.DeclaringType.AssemblyQualifiedName))
			{
				writer.Write((byte)2);
				writer.Write(mi.DeclaringType.AssemblyQualifiedName);
				writer.Write(mi.MetadataToken);
			}
		}

		private MethodBase DeserializeMethod([NotNull] BinaryReader reader)
		{
			int b = reader.ReadByte();
			if (b == 1) return _module.ResolveMethod(reader.ReadInt32());

			string typeName = reader.ReadString();
			if (string.IsNullOrEmpty(typeName)) return null;
			return Type.GetType(typeName, true)
				.Module
				.ResolveMethod(reader.ReadInt32());
		}

		/// <summary>
		/// Evalulates an expression tree quickly on the local side without the cost of calling Compile().
		/// This works only with simple method calls and property reads. In other cases, it returns null.
		/// </summary>
		public static Func<T, U> FastEvalExpr<T, U>(Expression body)
		{
			// Optimize common cases:
			MethodInfo method;
			IEnumerable<Expression> args = new Expression[0];

			switch (body)
			{
				case MethodCallExpression mc:
					method = mc.Method;
					args = mc.Arguments;
					break;
				case MemberExpression me when me.Member is PropertyInfo info:
					method = info.GetGetMethod();
					break;
				default:
					return null;
			}

			return x =>
			{
				try
				{
					return (U)method.Invoke(x, args.Select(a => GetExprValue(a, false)).ToArray());
				}
				catch (TargetInvocationException ex)
				{
					throw ex.InnerException ?? ex;
				}
			};
		}

		private static object GetExprValue([NotNull] Expression expr, bool deferLocalInstanceProperty)
		{
			// Optimize the common simple cases, the first being a simple constant:
			if (expr is ConstantExpression constant) return constant.Value;

			// The second common simple case is accessing a field in a closure:
			MemberExpression me = expr as MemberExpression;

			if (me?.Member is FieldInfo info && me.Expression is ConstantExpression expression)
				return info.GetValue(expression.Value);

			// If we're referring to the LocalInstance property of the proxy, we need to defer its evaluation
			// until it's deserialized at the other end, as it will likely be null:
			if (deferLocalInstanceProperty && me?.Member is PropertyInfo)
			{
				if (me.Member.Name == "Instance"
					&& me.Member.ReflectedType != null 
					&& me.Member.ReflectedType.IsGenericType 
					&& me.Member.ReflectedType.GetGenericTypeDefinition() == typeof(ChannelProxy<>))
				{
					return GetExprValue(me.Expression, true);
				}
			}

			// This will take longer:
			UnaryExpression objectMember = Expression.Convert(expr, typeof(object));
			Expression<Func<object>> getterLambda = Expression.Lambda<Func<object>>(objectMember);
			Func<object> getter = getterLambda.Compile();
			return getter();
		}
	}
}