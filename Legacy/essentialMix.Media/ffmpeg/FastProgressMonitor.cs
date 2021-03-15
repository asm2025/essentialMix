using System;
using System.Text.RegularExpressions;
using essentialMix.Helpers;
using essentialMix.Media.Commands;
using essentialMix.Threading.Collections.MessageQueue;
using JetBrains.Annotations;

namespace essentialMix.Media.ffmpeg
{
	public class FastProgressMonitor : IProgressMonitor
	{
		private static readonly Regex __progress = new Regex(@"^frame=\s*(?<frame>\d+)\s+fps=\s*\d+(?:\.\d+)?\s+q=\s*-?\d+(?:\.\d+)?\s+size=\s*\d+\w+\s+time=\s*[^ ]*\s+bitrate=\s*\d+(?:\.\d+)?\w+/s\s+speed=\s*\d+(?:\.\d+)?x", RegexHelper.OPTIONS_I | RegexOptions.Multiline);
		private static readonly Regex __completed = new Regex(@"^encoded\s+\d+(?:\.\d+)?\s+frames in\s+\d+(?:\.\d+)?s\s+(?:\(\d+(?:\.\d+)? fps\))", RegexHelper.OPTIONS_I | RegexOptions.Multiline);

		private readonly double _frames;
		private readonly Queue<string> _messages;
		
		private volatile bool _startCalled;
		private volatile bool _done;

		public FastProgressMonitor(long frames, [NotNull] Action onStart, [NotNull] Action<int> onProgress, [NotNull] Action onCompleted)
		{
			_frames = frames;
			_messages = new Queue<string>(data =>
			{
				Match m = __progress.Match(data);

				if (m.Success)
				{
					if (!_startCalled)
					{
						OnProgressStart();
						_startCalled = true;
					}

					long frame = long.Parse(m.Groups["frame"].Value);
					int percent = (int)(frame / _frames * 100);
					OnProgress(percent);
					return;
				}

				if (!_startCalled) return;
				m = __completed.Match(data);
				if (!m.Success) return;
				OnProgressCompleted();
				_done = true;
			});
			OnProgressStart = onStart;
			OnProgress = onProgress;
			OnProgressCompleted = onCompleted;
			Reset();
		}

		[NotNull]
		public Action OnProgressStart { get; }

		[NotNull]
		public Action<int> OnProgress { get; }

		[NotNull]
		public Action OnProgressCompleted { get; }

		public void ProcessOutput(string data)
		{
			if (_done || string.IsNullOrEmpty(data)) return;
			_messages.Enqueue(data);
		}

		public void Reset()
		{
			_startCalled = false;
			_done = _frames > 0.0;
		}
	}
}