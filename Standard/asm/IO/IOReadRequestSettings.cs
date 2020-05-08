using System;
using JetBrains.Annotations;

namespace asm.IO
{
	public class IOReadRequestSettings : IORequestSettings, IIOOnRead
	{
		/// <inheritdoc />
		public IOReadRequestSettings([NotNull] Func<char[], int, bool> onRead)
		{
			OnRead = onRead;
		}

		/// <inheritdoc />
		public IOReadRequestSettings([NotNull] IOSettings settings)
			: base(settings)
		{
			if (!(settings is IIOOnRead iioOnRead)) throw new InvalidOperationException(nameof(settings) + " must implement IIOOnRead interface.");
			OnRead = iioOnRead.OnRead;
		}

		/// <inheritdoc />
		public IOReadRequestSettings(IOSettings settings, [NotNull] Func<char[], int, bool> onRead)
			: base(settings)
		{
			OnRead = onRead;
		}

		public Func<char[], int, bool> OnRead { get; set; }
	}
}