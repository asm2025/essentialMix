using System.Collections.Generic;

namespace asm.Data.Patterns.Parameters
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

		/// <inheritdoc />
		public ICollection<string> FilterReferences { get; set; }

		/// <inheritdoc />
		public ICollection<string> FilterImports { get; set; }
	}
}