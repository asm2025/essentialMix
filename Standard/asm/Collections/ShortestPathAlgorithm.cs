namespace asm.Collections
{
	// https://en.wikipedia.org/wiki/Shortest_path_problem
	public enum ShortestPathAlgorithm
	{
		/// <summary>
		/// single-source with non-negative edge weight.
		/// </summary>
		Dijkstra,
		/// <summary>
		/// single-source if edge weights may be negative.
		/// </summary>
		BellmanFord,
		/// <summary>
		/// single pair using heuristics to try to speed up the search.
		/// </summary>
		AStar,
		/// <summary>
		/// all pairs shortest paths.
		/// </summary>
		FloydWarshall,
		/// <summary>
		/// all pairs shortest paths, and may be faster than Floyd–Warshall on sparse graphs.
		/// </summary>
		Johnson,
		/// <summary>
		/// shortest stochastic path problem with an additional probabilistic weight on each node.
		/// </summary>
		Viterbi
	}
}