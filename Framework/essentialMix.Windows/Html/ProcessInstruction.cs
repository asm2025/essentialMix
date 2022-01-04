namespace essentialMix.Windows.Html;

/// <summary>
/// HTML Process Instruction
/// </summary>
public class ProcessInstruction : SimplePart
{
	public ProcessInstruction(string value)
		: base(PartType.ProcessInstruction, value) { }

	public override string ToString() { return $"ProcInstr: {Value}"; }
}