using System.IO;
using essentialMix.Extensions;

namespace essentialMix.Core.Web.Model
{
	public class UploadSettings
	{
		private string _targetPath;

		public string TargetPath
		{
			get => _targetPath; 
			set => _targetPath = value?.Trim(Path.DirectorySeparatorChar, ' ').ToNullIfEmpty();
		}

		public bool Overwrite { get; set; }
		public bool Rename { get; set; } = true;
	}
}
