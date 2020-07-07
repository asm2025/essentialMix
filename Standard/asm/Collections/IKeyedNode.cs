namespace asm.Collections
{
	public interface IKeyedNode<TKey, TValue> : INode<TValue>
	{
		TKey Key { get; set; }
	}
}