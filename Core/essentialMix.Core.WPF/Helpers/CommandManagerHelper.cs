using System;
using System.Collections.Generic;
using System.Windows.Input;
using JetBrains.Annotations;

namespace essentialMix.Core.WPF.Helpers
{
	public static class CommandManagerHelper
	{
		public static void CallWeakReferenceHandlers([NotNull] IList<WeakReference> handlers)
		{
			if (handlers.Count == 0) return;

			EventHandler[] callees = new EventHandler[handlers.Count];
			int count = 0;

			for (int i = handlers.Count - 1; i >= 0; i--)
			{
				WeakReference reference = handlers[i];

				if (reference.Target is EventHandler handler)
				{
					callees[count] = handler;
					count++;
				}
				else
				{
					// Clean up old handlers that have been collected
					handlers.RemoveAt(i);
				}
			}

			for (int i = 0; i < count; i++)
			{
				EventHandler handler = callees[i];
				handler(null, EventArgs.Empty);
			}
		}

		public static void AddHandlersToRequerySuggested([NotNull] IList<WeakReference> handlers)
		{
			foreach (WeakReference handlerRef in handlers)
			{
				if (handlerRef.Target is EventHandler handler) CommandManager.RequerySuggested += handler;
			}
		}

		public static void RemoveHandlersFromRequerySuggested([NotNull] IList<WeakReference> handlers)
		{
			foreach (WeakReference handlerRef in handlers)
			{
				if (handlerRef.Target is EventHandler handler) CommandManager.RequerySuggested -= handler;
			}
		}

		public static void AddWeakReferenceHandler(ref IList<WeakReference> handlers, EventHandler handler)
		{
			AddWeakReferenceHandler(ref handlers, handler, -1);
		}

		public static void AddWeakReferenceHandler(ref IList<WeakReference> handlers, EventHandler handler, int defaultListSize)
		{
			handlers ??= defaultListSize > 0
							? new List<WeakReference>(defaultListSize)
							: new List<WeakReference>();
			handlers.Add(new WeakReference(handler));
		}

		public static void RemoveWeakReferenceHandler([NotNull] IList<WeakReference> handlers, EventHandler handler)
		{
			for (int i = handlers.Count - 1; i >= 0; i--)
			{
				WeakReference reference = handlers[i];
				if (reference.Target is EventHandler existingHandler && existingHandler != handler) continue;
				// Clean up old handlers that have been collected
				// in addition to the handler that is to be removed.
				handlers.RemoveAt(i);
			}
		}
	}
}
