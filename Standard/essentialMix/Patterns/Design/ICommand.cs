namespace essentialMix.Patterns.Design
{
	public interface ICommand
	{
		void Execute();
		void Undo();
	}
}
