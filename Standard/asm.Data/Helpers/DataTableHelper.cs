using System;
using System.Data;
using System.IO;
using JetBrains.Annotations;

namespace asm.Data.Helpers
{
	public static class DataTableHelper
	{
		[NotNull]
		public static DataTable FromXml([NotNull] string xml)
		{
			if (string.IsNullOrWhiteSpace(xml)) throw new ArgumentNullException(nameof(xml));

			DataTable table = new DataTable();

			using (StringReader reader = new StringReader(xml))
			{
				table.ReadXml(reader);
			}

			return table;
		}
	}
}