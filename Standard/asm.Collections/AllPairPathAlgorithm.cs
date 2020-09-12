namespace asm.Collections
{
	// https://en.wikipedia.org/wiki/Shortest_path_problem
	public enum AllPairPathAlgorithm
	{
		/// <summary>
		/// Solves all pairs shortest paths. O(V^3)
		/// </summary>
		FloydWarshall,
		/// <summary>
		/// Solves all pairs shortest paths, and may be faster than Floyd–Warshall on sparse graphs.
		/// </summary>
		Johnson,
		/// <summary>
		/// Solves the shortest stochastic path problem with an additional probabilistic weight on each vertex.
		/// </summary>
		Viterbi
	}
}