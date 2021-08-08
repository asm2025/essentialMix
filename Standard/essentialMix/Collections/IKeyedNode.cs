namespace essentialMix.Collections
{
	public interface IKeyedNode<TKey, TValue> : INode<TValue>
	{
		TKey Key { get; set; }
		string ToString(int level);
	}
}