using JetBrains.Annotations;

namespace essentialMix.Threading;

public class Runnable : ExecutableBase<RunSettingsBase>
{
	public Runnable() { }

	public Runnable(string executableName)
		: base(executableName)
	{
	}

	/// <inheritdoc />
	[NotNull]
	protected override RunSettingsBase GetSettingsOrDefault() { return RunSettingsBase.Default; }
}