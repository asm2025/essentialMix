using System;
using System.Net;
using asm.Extensions;
using asm.Helpers;

namespace asm.IO
{
	public class IOResponseSettings : IOSettings, IIOOnResponseReceived
	{
		private const int RETRY_MIN = 0;
		private const int RETRY_MAX = 99;

		private static readonly TimeSpan TIME_BEFORE_RETRIES_MIN = TimeSpan.Zero;
		private static readonly TimeSpan TIME_BEFORE_RETRIES_MAX = TimeSpanHelper.Day;
		
		private int _retries = 3;
		private TimeSpan _timeBeforeRetries = TimeSpanHelper.Second;

		/// <inheritdoc />
		public IOResponseSettings()
		{
		}

		/// <inheritdoc />
		public IOResponseSettings(IOSettings settings) 
			: base(settings)
		{
			if (settings is IIOOnResponseReceived responseReceived) OnResponseReceived = responseReceived.OnResponseReceived;
			if (settings is IOResponseSettings responseSettings) Retries = responseSettings.Retries;
		}

		public Func<WebResponse, bool> OnResponseReceived { get; set; }

		public int Retries
		{
			get => _retries;
			set => _retries = value.Within(RETRY_MIN, RETRY_MAX);
		}

		public TimeSpan TimeBeforeRetries
		{
			get => _timeBeforeRetries;
			set => _timeBeforeRetries = value.Within(TIME_BEFORE_RETRIES_MIN, TIME_BEFORE_RETRIES_MAX);
		}
	}
}