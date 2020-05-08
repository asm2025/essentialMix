using System;
using System.Collections.Generic;
using System.Linq;

namespace asm.Media.Youtube
{
	public class ClosedCaptionTrack
	{
		public ClosedCaptionTrack()
		{
		}

		public ClosedCaptionTrackInfo Info { get; internal set; }

		public IReadOnlyList<ClosedCaption> Captions { get; internal set; }

		public ClosedCaption GetByTime(TimeSpan time) { return Captions.FirstOrDefault(c => time >= c.Offset && time <= c.Offset + c.Duration); }
	}
}