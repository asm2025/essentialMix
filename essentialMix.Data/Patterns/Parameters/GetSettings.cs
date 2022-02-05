using System.Collections.Generic;

namespace essentialMix.Data.Patterns.Parameters;

public struct GetSettings : IGetSettings
{
	/// <inheritdoc />
	public IList<string> Include { get; set; }

	/// <inheritdoc />
	public string FilterExpression { get; set; }
}