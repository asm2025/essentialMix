using System;
using JetBrains.Annotations;

namespace asm.Media.Youtube
{
	public enum Container
	{
		Mp4,
		M4A,
		WebM,
		Tgpp,
		Flv
	}

	public static class ContainerExtension
	{
		/// <summary>
		/// Get file extension based on container type
		/// </summary>
		[NotNull]
		public static string GetFileExtension(this Container thisValue)
		{
			switch (thisValue)
			{
				case Container.Mp4:
					return "mp4";
				case Container.M4A:
					return "m4a";
				case Container.WebM:
					return "webm";
				case Container.Tgpp:
					return "3gpp";
				case Container.Flv:
					return "flv";
				default:
					throw new NotSupportedException();
			}
		}
	}
}