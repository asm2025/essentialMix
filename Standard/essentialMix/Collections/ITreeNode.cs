namespace essentialMix.Collections
{
	public interface ITreeNode<T> : INode<T>
	{
		string ToString(int level);
	}
}