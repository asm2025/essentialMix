using System;
using System.Threading;
using asm.Delegation;
using JetBrains.Annotations;

namespace asm.Other.JonSkeet.MiscUtil.Threading
{
	/// <summary>
	/// Represents the method that is executed by a ThreadController.
	/// </summary>
	public delegate void ControlledThreadStart(ThreadController controller, object state);

	/// <summary>
	/// Class designed to control a worker thread (co-operatively).
	/// </summary>
	public class ThreadController
	{
		/// <summary>
		/// Lock used throughout for all state management.
		/// (This is unrelated to the "state" variable.)
		/// </summary>
		private readonly object _stateLock = new object();

		/// <summary>
		/// The delegate to be invoked when the thread is started.
		/// </summary>
		private readonly ControlledThreadStart _starter;

		/// <summary>
		/// State to pass to the "starter" delegate when the thread is started.
		/// This reference is discarded when the new thread is started, so
		/// it won't prevent garbage collection.
		/// </summary>
		private object _state;

		private bool _started;

		private Thread _thread;

		private bool _stopping;

		/// <summary>
		/// Creates a new controller.
		/// </summary>
		/// <param name="starter">
		/// The delegate to invoke when the thread is started.
		/// Must not be null.
		/// </param>
		/// <param name="state">The state to pass to the delegate. May be null.</param>
		public ThreadController([NotNull] ControlledThreadStart starter, object state)
		{
			_starter = starter ?? throw new ArgumentNullException(nameof(starter));
			_state = state;
		}

		/// <inheritdoc />
		/// <summary>
		/// Creates a new controller without specifying a state object to
		/// pass when the delegate is invoked.
		/// </summary>
		/// <param name="starter">The delegate to invoke when the thread is started.</param>
		public ThreadController([NotNull] ControlledThreadStart starter) : this(starter, null)
		{
		}

		/// <summary>
		/// Event raised if the controlled thread throws an unhandled exception.
		/// The exception is not propagated beyond the controller by default, however
		/// by adding an ExceptionHandler which simply rethrows the exception,
		/// it will propagate. Note that in this case any further ExceptionHandlers
		/// added after the propagating one will not be executed. This event is
		/// raised in the worker thread.
		/// </summary>
		public event ExceptionHandler Exception;

		/// <summary>
		/// Event raised when the thread has finished and all exception handlers
		/// have executed (if an exception was raised). Note that this event is
		/// raised even if one of the exception handlers propagates the exception
		/// up to the top level. This event is raised in the worker thread.
		/// </summary>
		public event ThreadProgress Finished;

		/// <summary>
		/// Event raised when a stop is requested. Worker threads
		/// may register for this event to allow them to respond to
		/// stop requests in a timely manner. The event is raised
		/// in the thread which calls the Stop method.
		/// </summary>
		public event ThreadProgress StopRequested;

		/// <summary>
		/// Whether the thread has been started. A thread can only
		/// be started once.
		/// </summary>
		public bool Started
		{
			get
			{
				lock(_stateLock)
				{
					return _started;
				}
			}
		}

		/// <summary>
		/// Thread being controlled. May be null if it hasn't
		/// been created yet.
		/// </summary>
		public Thread Thread
		{
			get
			{
				lock(_stateLock)
				{
					return _thread;
				}
			}
		}

		/// <summary>
		/// Whether or not the thread is stopping. This may be used
		/// by the thread itself to test whether or not to stop, as
		/// well as by clients checking status. To see whether the
		/// thread has actually finished or not, use the IsAlive
		/// property of the thread itself.
		/// </summary>
		public bool Stopping
		{
			get
			{
				lock(_stateLock)
				{
					return _stopping;
				}
			}
		}

		/// <summary>
		/// Creates the thread to later be started. This enables
		/// properties of the thread to be manipulated before the thread
		/// is started.
		/// </summary>
		/// <exception cref="InvalidOperationException">The thread has already been created.</exception>
		public void CreateThread()
		{
			lock(_stateLock)
			{
				if (_thread != null) throw new InvalidOperationException("Thread has already been created");
				_thread = new Thread(RunTask);
			}
		}

		/// <summary>
		/// Starts the task in a separate thread, creating it if it hasn't already been
		/// created with the CreateThread method.
		/// </summary>
		/// <exception cref="InvalidOperationException">The thread has already been started.</exception>
		public void Start()
		{
			lock(_stateLock)
			{
				if (_started) throw new InvalidOperationException("Thread has already been created");
				_thread ??= new Thread(RunTask);
				_thread.Start();
				_started = true;
			}
		}

		/// <summary>
		/// Tell the thread being controlled by this controller to stop.
		/// This call does not throw an exception if the thread hasn't been
		/// created, or has already been told to stop - it is therefore safe
		/// to call at any time, regardless of other information about the
		/// state of the controller. Depending on the way in which the controlled
		/// thread is running, it may not take notice of the request to stop
		/// for some time.
		/// </summary>
		public void Stop()
		{
			lock(_stateLock)
			{
				_stopping = true;
			}

			ThreadProgress handler;

			lock(_stateLock)
			{
				handler = StopRequested;
			}

			handler?.Invoke(this);
		}

		/// <summary>
		/// Runs the task specified by starter, catching exceptions and propagating them
		/// to the Exception event.
		/// </summary>
		private void RunTask()
		{
			try
			{
				// Allow state to be garbage collected during execution
				object stateTmp = _state;
				_state = null;
				_starter(this, stateTmp);
			}
			catch (Exception e)
			{
				ExceptionHandler handler;

				lock(_stateLock)
				{
					handler = Exception;
				}

				handler?.Invoke(this, e);
			}
			finally
			{
				ThreadProgress handler;

				lock(_stateLock)
				{
					handler = Finished;
				}

				handler?.Invoke(this);
			}
		}
	}
}