namespace essentialMix.Collections
{
	public enum SpanningTreeAlgorithm
	{
		/// <summary>
		/// <see href="https://en.wikipedia.org/wiki/Prim%27s_algorithm">A greedy algorithm</see> that finds a minimum spanning tree for a weighted undirected graph.
		/// </summary>
		Prim,
		/// <summary>
		/// <see href="https://en.wikipedia.org/wiki/Kruskal%27s_algorithm">A greedy algorithm</see> that finds a minimum spanning tree, or a minimum spanning forest for each connected component in a weighted undirected graph.
		/// </summary>
		Kruskal, // Reverse-delete algorithm?
	}
}