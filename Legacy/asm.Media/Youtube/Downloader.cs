using System;
using JetBrains.Annotations;

namespace asm.Media.Youtube
{
    /// <summary>
    /// Provides the base class for the <see cref="AudioDownloader"/> and <see cref="VideoDownloader"/> class.
    /// </summary>
    public abstract class Downloader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Downloader"/> class.
        /// </summary>
        /// <param name="streamInfo">The video to download/convert.</param>
        /// <param name="savePath">The path to save the video/audio.</param>
        /// /// <param name="bytesToDownload">An optional value to limit the number of bytes to download.</param>
        /// <exception cref="ArgumentNullException"><paramref name="streamInfo"/> or <paramref name="savePath"/> is <c>null</c>.</exception>
        protected Downloader([NotNull] MediaStreamInfo streamInfo, [NotNull] string savePath, int? bytesToDownload = null)
        {
	        StreamInfo = streamInfo ?? throw new ArgumentNullException(nameof(streamInfo));
            SavePath = savePath ?? throw new ArgumentNullException(nameof(savePath));
            BytesToDownload = bytesToDownload;
        }

        /// <summary>
        /// Occurs when the download finished.
        /// </summary>
        public event EventHandler DownloadFinished;

        /// <summary>
        /// Occurs when the download is starts.
        /// </summary>
        public event EventHandler DownloadStarted;

        /// <summary>
        /// Gets the number of bytes to download. <c>null</c>, if everything is downloaded.
        /// </summary>
        public int? BytesToDownload { get; }

        /// <summary>
        /// Gets the path to save the video/audio.
        /// </summary>
        public string SavePath { get; }

        /// <summary>
        /// Gets the streamInfo to download/convert.
        /// </summary>
        public MediaStreamInfo StreamInfo { get; }

        /// <summary>
        /// Starts the work of the <see cref="Downloader"/>.
        /// </summary>
        public abstract void Execute();

        protected void OnDownloadStarted(EventArgs e)
        {
	        DownloadStarted?.Invoke(this, e);
        }

        protected void OnDownloadFinished(EventArgs e)
        {
	        DownloadFinished?.Invoke(this, e);
        }
    }
}