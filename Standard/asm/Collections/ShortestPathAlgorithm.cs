namespace asm.Collections
{
	// https://en.wikipedia.org/wiki/Shortest_path_problem
	public enum ShortestPathAlgorithm
	{
		/// <summary>
		/// <see href="https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm">Dijkstra algorithm</see> solves the
		/// single-source shortest path problem with non-negative edge weight.
		/// </summary>
		Dijkstra,
		/// <summary>
		/// <see href="https://en.wikipedia.org/wiki/Bellman%E2%80%93Ford_algorithm">Bellman–Ford algorithm</see>
		/// solves the single-source problem if edge weights may be negative.
		/// </summary>
		BellmanFord,
		/// <summary>
		/// <see href="https://en.wikipedia.org/wiki/B*">B*</see> solves for single pair shortest path using a
		/// best-first graph search algorithm to finds the least-cost path.
		/// </summary>
		BStar,
		/// <summary>
		/// <see href="https://en.wikipedia.org/wiki/A*_search_algorithm">A*</see> is special case of best-first
		/// search that solves for single pair shortest path using heuristics to try to speed up the search. It is
		/// often used in many fields of computer science due to its completeness. One major practical drawback is
		/// its O(b^d) space complexity. it is generally outperformed by algorithms which can pre-process the graph
		/// to attain better performance as well as memory-bounded approaches; however, A* is still the best solution
		/// in many cases.
		/// </summary>
		AStar,
		/// <summary>
		/// <see href="https://en.wikipedia.org/wiki/D*">D*</see> and its variants have been widely used for mobile
		/// robot and autonomous vehicle navigation. It finds a shortest path from its current coordinates to the goal
		/// coordinates under the assumption that it contains no obstacles. The robot then follows the path. When it
		/// observes new map information (such as previously unknown obstacles), it adds the information to its map and,
		/// if necessary, re-plans a new shortest path from its current coordinates to the given goal coordinates. It
		/// repeats the process until it reaches the goal coordinates or determines that the goal coordinates cannot be
		/// reached.
		/// </summary>
		DStar,
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