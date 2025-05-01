using System;

namespace essentialMix.Events;

public class ProgressEventArgs(float percentage) : EventArgs
{
	/// <summary>
	/// Gets or sets a token whether the operation that reports the progress should be canceled.
	/// </summary>
	public bool Cancel { get; set; }

	/// <summary>
	/// Gets the progress percentage in a range from 0.0 to 100.0.
	/// </summary>
	public float Percentage { get; } = percentage;
}