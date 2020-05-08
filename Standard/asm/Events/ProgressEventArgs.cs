using System;

namespace asm.Events
{
    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(float percentage)
        {
            Percentage = percentage;
        }

        /// <summary>
        /// Gets or sets a token whether the operation that reports the progress should be canceled.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the progress percentage in a range from 0.0 to 100.0.
        /// </summary>
        public float Percentage { get; }
    }
}