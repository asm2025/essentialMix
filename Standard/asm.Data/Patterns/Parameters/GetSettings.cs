using System.Collections.Generic;

namespace asm.Data.Patterns.Parameters
{
	public struct GetSettings : IGetSettings, IIncludeSettings, IFilterSettings
	{
		/// <inheritdoc />
		public object[] KeyValue { get; set; }

		/// <inheritdoc />
		public IList<string> Include { get; set; }

		/// <inheritdoc />
		public DynamicFilter Filter { get; set; }
	}
}