using System;
using System.IO;
using System.Net;
using asm.Events;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Media.Youtube
{
    /// <summary>
    /// Provides a method to download a video from YouTube.
    /// </summary>
    public class VideoDownloader : Downloader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoDownloader"/> class.
        /// </summary>
        /// <param name="streamInfo">The video to download.</param>
        /// <param name="savePath">The path to save the video.</param>
        /// <param name="bytesToDownload">An optional value to limit the number of bytes to download.</param>
        /// <exception cref="ArgumentNullException"><paramref name="streamInfo"/> or <paramref name="savePath"/> is <c>null</c>.</exception>
        public VideoDownloader([NotNull] MediaStreamInfo streamInfo, [NotNull] string savePath, int? bytesToDownload = null)
            : base(streamInfo, savePath, bytesToDownload) { }

		/// <summary>
		/// Occurs when the download progress of the video file has changed.
		/// </summary>
		public event EventHandler<ProgressEventArgs> DownloadProgressChanged;

		/// <summary>
		/// Starts the video download.
		/// </summary>
		/// <exception cref="IOException">The video file could not be saved.</exception>
		/// <exception cref="WebException">An error occurred while downloading the video.</exception>
		public override void Execute()
        {
            OnDownloadStarted(EventArgs.Empty);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(StreamInfo.Url);

            if (BytesToDownload.HasValue)
            {
                request.AddRange(0, BytesToDownload.Value - 1);
            }

            // the following code is alternative, you may implement the function after your needs
            using (WebResponse response = request.GetResponse())
			{
				using Stream source = response.GetStream();
				using FileStream target = File.Open(SavePath, FileMode.Create, FileAccess.Write);
				byte[] buffer = new byte[Constants.BUFFER_MB];
				bool cancel = false;
				int bytes;
				double copiedBytes = 0;

				while (!cancel && (bytes = source.Read(buffer)) > 0)
				{
					target.Write(buffer, 0, bytes);
					if (DownloadProgressChanged == null) continue;
					copiedBytes += bytes;

					ProgressEventArgs eventArgs = new ProgressEventArgs((int)(copiedBytes / response.ContentLength * 100));
					DownloadProgressChanged(this, eventArgs);
					if (eventArgs.Cancel) cancel = true;
				}
			}

            OnDownloadFinished(EventArgs.Empty);
        }
    }
}