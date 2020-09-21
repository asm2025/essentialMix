using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using asm.Collections;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Exceptions.Collections;
using asm.Helpers;
using asm.Patterns.Object;
using asm.Threading.Helpers;

namespace asm.Threading
{
	public abstract class ExecutableBase : Disposable
	{
		private string _executableName;
		private Properties _arguments = new Properties();
		private bool _isBusy;
		private IntPtr _jobId;
		private bool _canChangeExecutableName = true;

		protected ExecutableBase()
		{
		}

		protected ExecutableBase(string executableName) { ExecutableName = executableName; }

		public string ExecutableName
		{
			get
			{
				ThrowIfDisposed();
				return _executableName;
			}
			set
			{
				ThrowIfDisposed();
				if (!CanChangeExecutableName) throw new ReadOnlyException("Property is readonly");
				_executableName = value;
			}
		}

		[NotNull]
		public Properties Arguments
		{
			get
			{
				ThrowIfDisposed();
				return _arguments;
			}
			set
			{
				ThrowIfDisposed();
				_arguments = value;
			}
		}
		public TimeSpan Timeout { get; set; }

		public bool IsBusy
		{
			get
			{
				ThrowIfDisposed();
				return _isBusy;
			}
			protected set
			{
				ThrowIfDisposed();
				_isBusy = value;
			}
		}

		public IntPtr JobId
		{
			get
			{
				ThrowIfDisposed();
				return _jobId;
			}
			set
			{
				ThrowIfDisposed();
				_jobId = value;
			}
		}

		protected bool CanChangeExecutableName
		{
			get
			{
				ThrowIfDisposed();
				return _canChangeExecutableName;
			}
			set
			{
				ThrowIfDisposed();
				_canChangeExecutableName = value;
			}
		}

		[NotNull]
		public Task<bool> RunAsync(CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return Task.Run(() => Run(token), token).ConfigureAwait();
		}

		public abstract bool Run(CancellationToken token = default(CancellationToken));
		public abstract void Die();

		protected virtual void OnCollectingArguments() { }
		protected virtual void OnCollectedArguments() { }

		[NotNull]
		protected virtual string CollectArguments()
		{
			ThrowIfDisposed();
			OnCollectingArguments();

			int n = Arguments.Count;
			if (n == 0) return string.Empty;

			StringBuilder sb = new StringBuilder();

			foreach (IProperty property in Arguments)
			{
				string arg = CollectArgument(property);
				if (string.IsNullOrWhiteSpace(arg)) continue;
				if (sb.Length > 0) sb.Append(' ');
				sb.Append(arg);
			}

			OnCollectedArguments();

			if (Arguments.Count > n)
			{
				for (int i = n; i < Arguments.Count; i++)
				{
					IProperty property = Arguments[i];
					string arg = CollectArgument(property);
					if (string.IsNullOrWhiteSpace(arg)) continue;
					if (sb.Length > 0) sb.Append(' ');
					sb.Append(arg);
				}
			}

			return sb.ToString();
		}

		protected virtual string CollectArgument([NotNull] IProperty property)
		{
			ThrowIfDisposed();
			return (string)property.Value;
		}

		public virtual void CopyFrom([NotNull] ExecutableBase executableBase)
		{
			ThrowIfDisposed();
			if (executableBase == null) throw new ArgumentNullException(nameof(executableBase));
			if (IsBusy) throw new InvalidOperationException("Cannot change properties when a task is being executed.");
			if (CanChangeExecutableName) ExecutableName = executableBase.ExecutableName;

			Arguments.Clear();

			foreach (IProperty property in executableBase.Arguments)
			{
				if (!Arguments.TryGetValue(property.Name, out IProperty argument))
				{
					Arguments.Add(property);
					continue;
				}

				if (argument.ValueType.IsAssignableFrom(property.ValueType)) throw new InvalidCastException();
				argument.Value = property.Value;
			}
		}

		public void CopyTo([NotNull] ExecutableBase executable)
		{
			if (executable == null) throw new ArgumentNullException(nameof(executable));
			executable.CopyFrom(this);
		}
	}

	public abstract class ExecutableBase<T> : ExecutableBase
		where T : ExecutableSettingsBase
	{
		private Process _process;
		private T _settings;

		protected RunOutput RunOutput;

		protected ExecutableBase()
		{
		}

		protected ExecutableBase(string executableName) 
			: base(executableName)
		{
		}

		protected T Settings
		{
			get
			{
				ThrowIfDisposed();
				return _settings;
			}
			set
			{
				ThrowIfDisposed();
				_settings = value;
			}
		}

		public override void CopyFrom(ExecutableBase executableBase)
		{
			ThrowIfDisposed();
			base.CopyFrom(executableBase);
			if (!(executableBase is ExecutableBase<T> executable)) return;
			Settings = executable.Settings;
		}

		protected virtual void OnCreate(Process process) { }
		protected virtual void OnStart(string name, DateTime? startTime) { }
		protected virtual void OnOutput(string data) { }
		protected virtual void OnError(string data) { }
		protected virtual void OnExit(string name, DateTime? exitTime, int? exitCode) { }
		protected virtual void OnCompleted() { }
		protected virtual void OnTimeout() { }

		protected abstract T GetSettingsOrDefault();

		public override bool Run(CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();

			string executableName = ExecutableName?.Trim();
			if (string.IsNullOrEmpty(executableName)) throw new InvalidOperationException($"{nameof(ExecutableName)} is not set.");

			CancellationTokenSource cts = null;
			CancellationTokenSource ctsMerged = null;

			try
			{
				IsBusy = true;

				string args = CollectArguments();
				ExecutableSettingsBase settings = GetSettingsOrDefault() ?? (ExecutableSettingsBase)ShellSettings.Default;
				CancellationToken tokn;

				if (Timeout.IsValid())
				{
					cts = new CancellationTokenSource(Timeout);
					ctsMerged = !token.CanBeCanceled
						? cts
						: cts.Merge(token);
				}
				else
				{
					tokn = token;
				}

				switch (settings)
				{
					case RunAndWaitForSettings runAndWaitForSettings:
						runAndWaitForSettings.OnCreate = OnCreate;
						runAndWaitForSettings.OnStart = OnStart;
						runAndWaitForSettings.OnOutput = OnOutput;
						runAndWaitForSettings.OnError = OnError;
						runAndWaitForSettings.OnExit = OnExit;
						return ProcessHelper.RunAndWaitFor(executableName, args, runAndWaitForSettings, tokn);
					case RunSettingsBase runSettingsCore:
						runSettingsCore.OnCreate = OnCreate;
						runSettingsCore.OnStart = OnStart;
						runSettingsCore.OnExit = OnExit;
						RunOutput = ProcessHelper.RunAndGetOutput(executableName, args, runSettingsCore, tokn);
						return RunOutput != null;
					case ShellSettings shellSettings:
						return ProcessHelper.ShellExecAndWaitFor(executableName, args, shellSettings, tokn);
					default:
						throw new InvalidOperationException($"Unknown settings type {settings.GetType()}.");
				}
			}
			finally
			{
				IsBusy = false;
				ObjectHelper.Dispose(ref ctsMerged);
				ObjectHelper.Dispose(ref cts);
			}
		}

		public override void Die()
		{
			ThrowIfDisposed();
			_process?.Die();
			ObjectHelper.Dispose(ref _process);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_process?.Die();
				ObjectHelper.Dispose(ref _process);
			}
			base.Dispose(disposing);
		}
	}
}