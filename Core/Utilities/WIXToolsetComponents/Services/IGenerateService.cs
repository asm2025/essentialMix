using System.Collections.Generic;
using JetBrains.Annotations;

namespace WIXToolsetComponents.Services
{
	public interface IGenerateService
	{
		void GenerateFromDirectory(string path, bool includeSubfolders = false, string pattern = null);
		void GenerateFromList([NotNull] IEnumerable<string> list);
		void Cancel();
	}
}