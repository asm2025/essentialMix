using System;
using JetBrains.Annotations;

namespace essentialMix.IO
{
	public class IOReadSettings : IOSettings, IIOOnRead
	{
		/// <inheritdoc />
		public IOReadSettings([NotNull] Func<char[], int, bool> onRead)
		{
			OnRead = onRead;
		}

		/// <inheritdoc />
		public IOReadSettings([NotNull] IOSettings settings)
			: base(settings)
		{
			if (!(settings is IIOOnRead iioOnRead)) throw new InvalidOperationException(nameof(settings) + " must implement IIOOnRead interface.");
			OnRead = iioOnRead.OnRead;
		}

		/// <inheritdoc />
		public IOReadSettings(IOSettings settings, [NotNull] Func<char[], int, bool> onRead)
			: base(settings)
		{
			OnRead = onRead;
		}

		public Func<char[], int, bool> OnRead { get; set; }
	}
}