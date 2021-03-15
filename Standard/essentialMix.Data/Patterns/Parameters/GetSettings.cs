using System.Collections.Generic;

namespace essentialMix.Data.Patterns.Parameters
{
	public struct GetSettings : IGetSettings, IIncludeSettings, IFilterSettings
	{
		/// <inheritdoc />
		public GetSettings(params object[] keys)
			: this()
		{
			KeyValue = keys;
		}

		/// <inheritdoc />
		public object[] KeyValue { get; set; }

		/// <inheritdoc />
		public IList<string> Include { get; set; }

		/// <inheritdoc />
		public string FilterExpression { get; set; }
	}
}