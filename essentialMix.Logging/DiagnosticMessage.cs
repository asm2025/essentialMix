namespace essentialMix.Logging;

public class DiagnosticMessage(string message, Severity severity)
{
	public DiagnosticMessage(string message)
		: this(message, Severity.Information)
	{
	}

	public string Message { get; } = message;
	public Severity Severity { get; } = severity;
}