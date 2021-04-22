using System;
using JetBrains.Annotations;

namespace essentialMix.IO
{
	public class IOReadResponseSettings : IOResponseSettings, IIOOnRead
	{
		/// <inheritdoc />
		public IOReadResponseSettings([NotNull] Func<char[], int, bool> onRead)
		{
			OnRead = onRead;
		}

		/// <inheritdoc />
		public IOReadResponseSettings([NotNull] IOSettings settings)
			: base(settings)
		{
			if (settings is not IIOOnRead iioOnRead) throw new InvalidOperationException(nameof(settings) + " must implement IIOOnRead interface.");
			OnRead = iioOnRead.OnRead;
		}

		/// <inheritdoc />
		public IOReadResponseSettings(IOSettings settings, [NotNull] Func<char[], int, bool> onRead)
			: base(settings)
		{
			OnRead = onRead;
		}

		public Func<char[], int, bool> OnRead { get; set; }
	}
}