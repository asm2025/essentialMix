namespace essentialMix.Collections
{
	public interface IKeyedNode<TKey, TValue> : ITreeNode<TValue>
	{
		TKey Key { get; set; }
	}
}