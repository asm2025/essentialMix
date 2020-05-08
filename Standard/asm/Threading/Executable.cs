using JetBrains.Annotations;

namespace asm.Threading
{
	public class Executable : ExecutableBase<ShellSettings>
	{
		public Executable() { }

		public Executable(string executableName)
			: base(executableName)
		{
		}

		/// <inheritdoc />
		[NotNull]
		protected override ShellSettings GetSettingsOrDefault()
		{
			ThrowIfDisposed();
			return Settings ?? ShellSettings.Default;
		}
	}
}