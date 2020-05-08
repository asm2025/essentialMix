namespace asm.Patterns.Command
{
	public interface ICommand
	{
		void Execute();
		void Undo();
	}
}
