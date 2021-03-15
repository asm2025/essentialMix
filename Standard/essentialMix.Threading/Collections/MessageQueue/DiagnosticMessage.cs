namespace essentialMix.Threading.Collections.MessageQueue
{
	public class DiagnosticMessage
	{
		public DiagnosticMessage(string message)
			: this(message, Severity.Information)
		{
		}

		public DiagnosticMessage(string message, Severity severity)
		{
			Message = message;
			Severity = severity;
		}

		public string Message { get; }
		public Severity Severity { get; }
	}
}