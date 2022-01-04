namespace essentialMix.Collections;

// https://en.wikipedia.org/wiki/Shortest_path_problem
public enum SingleSourcePathAlgorithm
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
}