using System;
using System.IO;
using essentialMix.Helpers;
using essentialMix.Media.ffmpeg.Commands;

namespace essentialMix.Media.Helpers
{
	public static class VideoHelper
	{
		private const int THUMB_HEIGHT = 360;

		public static int GenerateThumbnail(string videoPath, string targetFilePath, Metadata metadata = null)
		{
			if (string.IsNullOrEmpty(videoPath) || !File.Exists(videoPath) || string.IsNullOrEmpty(targetFilePath)) return 0;

			if (metadata?.Video == null)
			{
				using FastMetadataCommand command = new FastMetadataCommand
				{
					Input = videoPath
				};
				if (!command.Run()) return 0;
				metadata = command.Metadata;
			}

			if (metadata?.Video == null) return 0;

			if (!File.Exists(targetFilePath))
			{
				try
				{
					using ThumbnailCommand command = new ThumbnailCommand
					{
						Input = videoPath,
						Output = targetFilePath,
						Overwrite = true,
						Height = THUMB_HEIGHT,
						Timeout = TimeSpan.FromMinutes(5)
					};
					// this fuck twat didn't create the thumb! maybe the duration is too little or it didn't find scene changes (boring video!)
					if (!command.Run() || !File.Exists(targetFilePath))
					{
						// OK, let's give it another try with fixed time scan
						command.Seek = TimeSpan.FromSeconds(1);
						command.SceneFilter = -1.0f;
						if (!command.Run() || !File.Exists(targetFilePath)) return 0;
					}
				}
				catch
				{
					FileHelper.Delete(targetFilePath);
					return 0;
				}
			}

			int videoThumbWidth = 0;

			if (metadata.Video.Size.Height > 0)
			{
				double factor = (double)metadata.Video.Size.Width / metadata.Video.Size.Height;
				videoThumbWidth = (int)Math.Ceiling(THUMB_HEIGHT * factor);
			}

			return videoThumbWidth;
		}

		public static int GeneratePreview(string videoPath, string targetFilePath, Metadata metadata = null)
		{
			if (string.IsNullOrEmpty(videoPath) || !File.Exists(videoPath) || string.IsNullOrEmpty(targetFilePath)) return 0;

			if (metadata?.Video == null)
			{
				using FastMetadataCommand command = new FastMetadataCommand
				{
					Input = videoPath,
					Timeout = TimeSpan.FromMinutes(3)
				};
				if (!command.Run()) return 0;
				metadata = command.Metadata;
			}

			if (metadata?.Video == null) return 0;

			if (!File.Exists(targetFilePath))
			{
				try
				{
					using ThumbnailCommand command = new ThumbnailCommand
					{
						Input = videoPath,
						Output = targetFilePath,
						PreviewMode = true,
						Overwrite = true,
						Height = THUMB_HEIGHT,
						Timeout = TimeSpan.FromMinutes(5)
					};
					if (metadata.Video.Size.Height > 0)
					{
						double factor = (double)metadata.Video.Size.Width / metadata.Video.Size.Height;
						command.VideoThumbWidth = (int)Math.Ceiling(command.Height * factor);
					}

					if (!command.Run()) return 0;
				}
				catch
				{
					FileHelper.Delete(targetFilePath);
					return 0;
				}
			}

			int videoThumbWidth = 0;

			if (metadata.Video.Size.Height > 0)
			{
				double factor = (double)metadata.Video.Size.Width / metadata.Video.Size.Height;
				videoThumbWidth = (int)Math.Ceiling(THUMB_HEIGHT * factor);
			}

			return videoThumbWidth;
		}
	}
}