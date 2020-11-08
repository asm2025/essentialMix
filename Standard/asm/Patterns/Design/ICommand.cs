namespace asm.Patterns.Design
{
	public interface ICommand
	{
		void Execute();
		void Undo();
	}
}
