using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Interviews
{
	public static class Codility
	{
		#region Time Complexity
		public static int TapeEquilibrium([NotNull] int[] A)
		{
			// https://app.codility.com/programmers/lessons/3-time_complexity/tape_equilibrium/
			/*
			 * A non-empty array A consisting of N integers is given. Array A represents numbers on a tape.
			 * Any integer P, such that 0 < P < N, splits this tape into two non-empty parts:
			 *
			 * A[0], A[1], ..., A[P − 1] and A[P], A[P + 1], ..., A[N − 1].
			 *
			 * The difference between the two parts is the value of: |(A[0] + A[1] + ... + A[P − 1]) − (A[P] + A[P + 1] + ... + A[N − 1])|
			 * In other words, it is the absolute difference between the sum of the first part and the sum of the second part.
			 *
			 * For example, consider array A such that:
			 * [3, 1, 2, 4, 3]
			 * We can split this tape in four places:
			 * P = 1, difference = |3 − 10| = 7
			 * P = 2, difference = |4 − 9| = 5
			 * P = 3, difference = |6 − 7| = 1
			 * P = 4, difference = |10 − 3| = 7
			 *
			 * the function should return 1
			 */
			if (A.Length == 0) return int.MaxValue;

			int left = A[0];
			if (A.Length == 1) return left;

			int right = 0;

			for (int i = 1; i < A.Length; i++)
				right += A[i];

			int result = Math.Abs(left - right);

			for (int i = 1; i < A.Length - 1; i++)
			{
				left += A[i];
				right -= A[i];
				int def = Math.Abs(left - right);
				if (def.CompareTo(result) < 0) result = def;
			}

			return result;
		}

		public static int FrogJump(int X, int Y, int D)
		{
			// https://app.codility.com/programmers/lessons/3-time_complexity/frog_jmp/
			/*
			 * A small frog wants to get to the other side of the road. The frog is currently
			 * located at position X and wants to get to a position greater than or equal to Y.
			 * The small frog always jumps a fixed distance, D. Count the minimal number of jumps
			 * that the small frog must perform to reach its target.
			 */
			if (D == 0) throw new ArgumentOutOfRangeException(nameof(D));
			if (X < Y && D < 0) throw new ArgumentOutOfRangeException(nameof(D));
			if (X > Y && D > 0) throw new ArgumentOutOfRangeException(nameof(D));
			if (X == Y) return 0;
			int jump = (Y - X) / D;
			if (jump * D < Y - X) jump++;
			return jump;
		}

		public static int PermMissingElem([NotNull] int[] A)
		{
			// https://app.codility.com/programmers/lessons/3-time_complexity/perm_missing_elem/
			/*
			* An array A consisting of N different integers is given. The array contains integers
			* in the range [1..(N + 1)], which means that exactly one element is missing. Your goal
			* is to find that missing element.
			*
			* For example, given array A such that:
			* [2, 3, 1, 5]
			* the function should return 4, as it is the missing element.
			*/
			int ans = A.Length + 1;
	
			// using XOR is better to avoid overflow
			for (int i = 0; i < A.Length; i++)
				ans = ans ^ A[i] ^ (i + 1);
		
			return ans;
		}
		#endregion
		
		#region Counting Elements
		public static bool PermCheck([NotNull] int[] A)
		{
			// https://app.codility.com/programmers/lessons/4-counting_elements/perm_check/
			/*
			 * A non-empty array A consisting of N integers is given. A permutation is a
			 * sequence containing each element from 1 to N once, and only once. For example,
			 * array A such that: [4, 1, 3, 2] is a permutation; but array A such that:
			 * [4, 1, 3] is not a permutation, because value 2 is missing. The goal is to check
			 * whether array A is a permutation.
			 *
			 * Write an efficient algorithm for the following assumptions:
			 * 1. N is an integer within the range [1..100,000];
			 * 2. each element of array A is an integer within the range [1..1,000,000,000].
			 */
			const int MAX_LEN = 100001;

			HashSet<int> set = new HashSet<int>();
			int n = Math.Min(A.Length, MAX_LEN), sum = n, expected = 1;

			for (int i = 0; i < n; i++)
			{
				if (A[i] > n || !set.Add(A[i])) return false;
				sum += i;
				sum -= A[i];
				if (expected < A[i]) expected = A[i];
			}

			// int from 1 to N without duplicates, set length = A length and Max(A) = expected = A length and sum = 0
			return set.Count == A.Length && expected == A.Length && sum == 0;
		}

		public static int FrogRiverOne(int X, [NotNull] int[] A)
		{
			// https://app.codility.com/programmers/lessons/4-counting_elements/frog_river_one/
			/*
			 * A small frog wants to get to the other side of a river. The frog is initially located on one
			 * bank of the river (position 0) and wants to get to the opposite bank (position X+1). Leaves
			 * fall from a tree onto the surface of the river.
			 *
			 * You are given an array A consisting of N integers representing the falling leaves. A[K]
			 * represents the position where one leaf falls at time K, measured in seconds.
			 *
			 * The goal is to find the earliest time when the frog can jump to the other side of the river.
			 * The frog can cross only when leaves appear at every position across the river from 1 to X
			 * (that is, we want to find the earliest moment when all the positions from 1 to X are covered
			 * by leaves). You may assume that the speed of the current in the river is negligibly small,
			 * i.e. the leaves do not change their positions once they fall in the river.
			 *
			 * For example, you are given integer X = 5 and array A such that:
			 * [1, 3, 1, 4, 2, 3, 5, 4]
			 * In second 6, a leaf falls into position 5. This is the earliest time when leaves appear in
			 * every position across the river.
			 *
			 * Write an efficient algorithm for the following assumptions:
			 * 1. N and X are integers within the range [1..100,000];
			 * 2. each element of array A is an integer within the range [1..X].
			 */
			const int LEN_MIN = 1;
			const int LEN_MAX = 100000;

			if (A.Length < LEN_MIN) return -1;

			int n = Math.Min(A.Length, LEN_MAX + 1);
			HashSet<int> set = new HashSet<int>();

			for (int i = 0; i < n; i++)
			{
				if (A[i] >= LEN_MIN && A[i] <= X) set.Add(A[i]);
				if (set.Count == X) return i;
			}

			return -1;
		}

		[NotNull]
		public static int[] MaxCounters(int N, [NotNull] int[] A)
		{
			// https://app.codility.com/programmers/lessons/4-counting_elements/max_counters/
			/*
			 * You are given N counters, initially set to 0, and you have two possible operations
			 * on them:
			 * 1. increase(X) − counter X is increased by 1,
			 * 2. max counter − all counters are set to the maximum value of any counter.
			 *
			 * A non-empty array A of M integers is given. This array represents consecutive
			 * operations:
			 * 1. if A[K] = X, such that 1 ≤ X ≤ N, then operation K is increase(X).
			 * 2. if A[K] = N + 1 then operation K is max counter.
			 *
			 * For example, given integer N = 5 and array A such that:
			 * [3, 4, 4, 6, 1, 4, 4]
			 * the values of the counters after each consecutive operation will be:
			 * (0, 0, 1, 0, 0)
			 * (0, 0, 1, 1, 0)
			 * (0, 0, 1, 2, 0)
			 * (2, 2, 2, 2, 2)
			 * (3, 2, 2, 2, 2)
			 * (3, 2, 2, 3, 2)
			 * (3, 2, 2, 4, 2)
			 *
			 * The goal is to calculate the value of every counter after all operations.
			 * For example, given:
			 * [3, 4, 4, 6, 1, 4, 4]
			 * the function should return [3, 2, 2, 4, 2], as explained above.
			 *
			 * Write an efficient algorithm for the following assumptions:
			 * 1. N and M are integers within the range [1..100,000];
			 * 2. each element of array A is an integer within the range [1..N + 1].
			 */
			const int LEN_MIN = 1;
			const int LEN_MAX = 100000;

			if (N < LEN_MIN || N > LEN_MAX) throw new ArgumentOutOfRangeException(nameof(N));
			if (A.Length < LEN_MIN) throw new ArgumentException("Empty array.");
			
			int n = Math.Min(A.Length, LEN_MAX + 1), max = 0, adjust = max;
			int[] counters = new int[N];

			for (int i = 0; i < n; i++)
			{
				int x = A[i];
				
				if (1 <= x && x <= N)
				{
					counters[x - 1] = Math.Max(adjust, counters[x - 1]) + 1;
					if (max < counters[x - 1]) max = counters[x - 1];
				}
				else if (x == N + 1)
				{
					adjust = max;
				}
			}

			// if any counter not increased for some reason
			if (adjust > 0)
			{
				for (int i = 0; i < counters.Length; i++)
					counters[i] = Math.Max(adjust, counters[i]);
			}

			return counters;
		}

		public static int MissingInteger([NotNull] int[] A)
		{
			// https://app.codility.com/programmers/lessons/4-counting_elements/missing_integer/
			/*
			 * Write a function:
			 *		class Solution { public int solution(int[] A); }
			 * that, given an array A of N integers, returns the smallest positive integer (greater than 0)
			 * that does not occur in A.
			 *
			 * For example:
			 *		given A = [1, 3, 6, 4, 1, 2], the function should return 5.
			 *		given A = [1, 2, 3], the function should return 4.
			 *		given A = [−1, −3], the function should return 1.
			 *
			 * Write an efficient algorithm for the following assumptions:
			 * 1. N is an integer within the range [1..100,000];
			 * 2. each element of array A is an integer within the range [−1,000,000..1,000,000].
			 */
			int ans = 1;
			HashSet<int> set = new HashSet<int>(A);

			while (set.Contains(ans)) 
				ans++;

			return ans;
		}
		#endregion

		#region Prefix Sums
		public static int PassingCars([NotNull] int[] A)
		{
			// https://app.codility.com/programmers/lessons/5-prefix_sums/passing_cars/
			/*
			 * A non-empty array A consisting of N integers is given. The consecutive
			 * elements of array A represent consecutive cars on a road.
			 * Array A contains only 0s and/or 1s:
			 *		0 represents a car traveling east,
			 *		1 represents a car traveling west.
			 * The goal is to count passing cars.
			 *
			 * We say that a pair of cars (P, Q), where 0 ≤ P < Q < N, is passing
			 * when P is traveling to the east and Q is traveling to the west.
			 *
			 * For example, consider array A such that:
			 * [0, 1, 0, 1, 1]
			 * We have five pairs of passing cars: (0, 1), (0, 3), (0, 4), (2, 3), (2, 4).
			 *
			 * Write a function:
			 *		class Solution { public int solution(int[] A); }
			 * that, given a non-empty array A of N integers, returns the number of pairs
			 * of passing cars. The function should return −1 if the number of pairs of
			 * passing cars exceeds 1,000,000,000.
			 *
			 * For example, given:
			 * [0, 1, 0, 1, 1]
			 * the function should return 5, as explained above.
			 *
			 * Write an efficient algorithm for the following assumptions:
			 *		N is an integer within the range [1..100,000];
			 *		each element of array A is an integer that can have one of the following values: 0, 1.
			 */
			const int MAX = 1000000000;

			int east = 0, pair = 0;

			foreach (int value in A)
			{
				if (value == 0) east++;
				else if (value == 1) pair += east;

				if (pair > MAX) return -1;
			}

			return pair;
		}

		[NotNull]
		public static int[] GenomicRangeQuery([NotNull] string S, [NotNull] int[] P, [NotNull] int[] Q)
		{
			// https://app.codility.com/programmers/lessons/5-prefix_sums/genomic_range_query/
			/*
			 * A DNA sequence can be represented as a string consisting of the letters A, C, G and T, which
			 * correspond to the types of successive nucleotides in the sequence. Each nucleotide has an impact
			 * factor, which is an integer. Nucleotides of types A, C, G and T have impact factors of 1, 2, 3 and
			 * 4, respectively. You are going to answer several queries of the form: What is the minimal impact
			 * factor of nucleotides contained in a particular part of the given DNA sequence?
			 *
			 * The DNA sequence is given as a non-empty string S = S[0]S[1]...S[N-1] consisting of N characters.
			 * There are M queries, which are given in non-empty arrays P and Q, each consisting of M integers.
			 * The K-th query (0 ≤ K < M) requires you to find the minimal impact factor of nucleotides contained
			 * in the DNA sequence between positions P[K] and Q[K] (inclusive).
			 *
			 * For example, consider string S = CAGCCTA and arrays P, Q such that:
			 * P[0] = 2    Q[0] = 4
			 * P[1] = 5    Q[1] = 5
			 * P[2] = 0    Q[2] = 6
			 *
			 * The answers to these M = 3 queries are as follows:
			 * The part of the DNA between positions 2 and 4 contains nucleotides G and C (twice), whose impact
			 * factors are 3 and 2 respectively, so the answer is 2. The part between positions 5 and 5 contains
			 * a single nucleotide T, whose impact factor is 4, so the answer is 4.
			 * The part between positions 0 and 6 (the whole string) contains all nucleotides, in particular
			 * nucleotide A whose impact factor is 1, so the answer is 1.
			 *
			 * Write a function:
			 *		class Solution { public int[] solution(String S, int[] P, int[] Q); }
			 * that, given a non-empty string S consisting of N characters and two non-empty arrays P and Q
			 * consisting of M integers, returns an array consisting of M integers specifying the consecutive
			 * answers to all queries.
			 *
			 * Result array should be returned as an array of integers.
			 *
			 * For example, given the string S = CAGCCTA and arrays P, Q such that:
			 * P[0] = 2    Q[0] = 4
			 * P[1] = 5    Q[1] = 5
			 * P[2] = 0    Q[2] = 6
			 * the function should return the values [2, 4, 1], as explained above.
			 *
			 * Write an efficient algorithm for the following assumptions:
			 *		1. N is an integer within the range [1..100,000];
			 *		2. M is an integer within the range [1..50,000];
			 *		3. each element of arrays P, Q is an integer within the range [0..N − 1];
			 *		4. P[K] ≤ Q[K], where 0 ≤ K < M;
			 *		5. string S consists only of upper-case English letters A, C, G, T.
			 */
			const int MAX_N = 100000;
			const int MAX_M = 50000;
			const int A = 0;
			const int C = 1;
			const int G = 2;
			const int T = 3;

			if (S.Length == 0) throw new ArgumentException("String is empty.", nameof(S));
			if (P.Length == 0) throw new ArgumentException("Array is empty.", nameof(P));
			if (Q.Length == 0) throw new ArgumentException("Array is empty.", nameof(Q));
			if (P.Length != Q.Length) throw new ArgumentException("Arrays lengths are not the same.");

			int n = Math.Min(S.Length, MAX_N);
			int m = Math.Min(P.Length, MAX_M);

			// Build the prefix sum array
			int [][] prefix = {
				new int[n], 
				new int[n], 
				new int[n], 
				new int[n]
			};

			for (int i = 0; i < n; i++)
			{
				int lastSeen;

				switch (S[i])
				{
					case 'A':
					case 'a':
						lastSeen = A;
						break;
					case 'C':
					case 'c':
						lastSeen = C;
						break;
					case 'G':
					case 'g':
						lastSeen = G;
						break;
					case 'T':
					case 't':
						lastSeen = T;
						break;
					default:
						throw new ArgumentException("Invalid input", nameof(S));
				}

				if (i == 0)
				{
					for (int j = 0; j < prefix.Length; j++)
					{
						prefix[j][i] = j == lastSeen
											? i
											: -1;
					}
				}
				else
				{
					for (int j = 0; j < prefix.Length; j++)
					{
						prefix[j][i] = j == lastSeen
											? i
											: prefix[j][i - 1];
					}
				}
			}

			// Build the result
			int[] result = new int[m];

			for (int i = 0; i < result.Length; i++)
			{
				if (prefix[A][Q[i]] >= P[i]) result[i] = 1;
				else if (prefix[C][Q[i]] >= P[i]) result[i] = 2;
				else if (prefix[G][Q[i]] >= P[i]) result[i] = 3;
				else if (prefix[T][Q[i]] >= P[i]) result[i] = 4;
			}

			return result;
		}

		public static int MinAvgTwoSlice([NotNull] int[] A)
		{
			// https://app.codility.com/programmers/lessons/5-prefix_sums/min_avg_two_slice/
			/*
			 * A non-empty array A consisting of N integers is given. A pair of integers (P, Q), such that 0 ≤ P < Q < N, is called a slice
			 * of array A (notice that the slice contains at least two elements).
			 * The average of a slice (P, Q) is the sum of A[P] + A[P + 1] + ... + A[Q] divided by the length of the slice. To be precise,
			 * the average equals (A[P] + A[P + 1] + ... + A[Q]) / (Q − P + 1).
			 *
			 * For example, array A such that:
			 * [4, 2, 2, 5, 1, 5, 8]
			 * contains the following example slices:
			 *		slice (1, 2), whose average is (2 + 2) / 2 = 2;
			 *		slice (3, 4), whose average is (5 + 1) / 2 = 3;
			 *		slice (1, 4), whose average is (2 + 2 + 5 + 1) / 4 = 2.5.
			 * The goal is to find the starting position of a slice whose average is minimal.
			 *
			 * Write a function:
			 *		class Solution { public int solution(int[] A); }
			 * that, given a non-empty array A consisting of N integers, returns the starting position of the slice with the minimal average.
			 * If there is more than one slice with a minimal average, you should return the smallest starting position of such a slice.
			 *
			 * For example, given array A such that:
			 * [4, 2, 2, 5, 1, 5, 8]
			 * the function should return 1, as explained above.
			 *
			 * Write an efficient algorithm for the following assumptions:
			 *		N is an integer within the range [2..100,000];
			 *		each element of array A is an integer within the range [−10,000..10,000].
			 */
			// https://www.martinkysel.com/codility-minavgtwoslice-solution/
			/*
			 * martinkysel says (he makes a claim):
			 * Every slice must be of size two or three. Slices of bigger sizes are created from such smaller slices. Therefore should any bigger
			 * slice have an optimal value, all sub-slices must be the same, for this case to hold true. Should this not be true, one of the sub-slices
			 * must be the optimal slice. The others being bigger. Therefore we check all possible slices of size 2/3 and return the smallest one. The
			 * first such slice is the correct one, do not use <=!
			 *
			 * mijhael castro civiero says:
			 * I think your solution is just valid for your assumptions about slices of 2 and 3 element size. But I do not see any
			 * assumption regarding the size of the slice on the codility exercise. Even they take this slice formed with 4 elements:
			 * A = [4, 2, 2, 5, 1, 5, 8]
			 * A(1, 4), which have an average of (2 + 2 + 5 + 1) / 4 = 2.5
			 * So if we have an array like this:
			 * A = [0, 8, 0, 0, -8, 0]
			 * the slice with the min avg is:
			 * A(1, 4) = 0
			 * and your code returns:3
			 * How could this result be valid?
			 *
			 * martinkysel says (replying to mijhael castro civiero):
			 * the slice A(1,4) has an average of 2.5. The slice A(1,2) has an average of 2. That is better, isn't it?
			 * A slice of length 4(1,4) contains 5 different subslices (1,2), (1,3), (2,3), (2,4), (3,4). If all elements in the 4 element
			 * slice are equal, then all subslices are equal. If there is at least one smaller element, the optimal subslice will contain
			 * that element and will be of the shortest possible length. All other slices will have worse average than the 4 element slice.
			 *
			 * As for the second case. There are two optimal slices. A(3, 4) = (0 - 8) / 2 = -4 and A(4,5) = (0 - 8) / 2 = -4. The other
			 * ones are worse, for example A(3, 4, 5) = (0 - 8 + 0) / 3 = -2,666.
			 * Notice that you return the INDEX of the smallest possible slice. Not the actual value.
			 *
			 * salmAn says (the proof):
			 * About the math behind the solution. Let e.g. C denote the avg of a slice of size 5: C = (v + w + x + y + z) / 5. And let A
			 * and B denote the avg of corresponding slices of size 2 and 3, respectively, i.e., A = (v + w) / 2 and B = (x + y + z) / 3.
			 * Simple math shows: C = 0.4 * A + 0.6 * B.
			 * Claim: it cannot be that C < A and C < B at the same time.
			 * Proof. Let say this is the case. Then we have 0.4 * C < 0.4 * A, and 0.6 * C < 0.6 * B. Summing these two, we get
			 * 0.4 * C + 0.6 * C < 0.4 * A + 0.6 * B, or equivalently C < C, which is obviously wrong.
			 * Therefore, either A <= C or B <= C. But, the algorithm already returns the best of A and B, thus the algorithm is also
			 * handling slices of size 5.
			 * For slices of bigger size, we can reason similarly.
			 *
			 * Minh Tran Dao syas (The formal proof):
			 * https://github.com/daotranminh/playground/blob/master/src/codibility/MinAvgTwoSlice/proof.pdf
			 *
			 * martinkysel says:
			 * The problem is categorized as "Prefix sums", which is kinda misleading, because the optimal solution does not use prefix sums.
			 * After I realized that, I started brainstorming other solutions. It did not look like a DP problem as it did not have solvable
			 * sub-problems; binary search seemed reasonable but O(n) would not work; After that I came back to the prefix sums. You can
			 * easily adapt the average if you know the number of elements, and the previous average (prefix sum). From this point on, you
			 * can try to find a solution that only adds new elements to an sub-slice if the average gets better, or restarts the sub-slice.
			 * Think of the caterpillar method. The method seemed reasonable so I had to define the edge cases, starting with the smallest
			 * possible cases (1,2,3). This led to me to realization, that you actually only need to consider slices of length 2 and 3. If
			 * a slice of length 1 was allowed, the minimal element would be optimal. Therefore you do not actually need to keep track of
			 * the prefix-sum and simplify the solution.
			 */
			if (A.Length < 2) return -1;
			if (A.Length < 3) return 0;

			int minIndex = 0;
			double minValue = int.MaxValue;

			for (int i = 0; i < A.Length - 1; i++)
			{
				double avg = (A[i] + A[i + 1]) / 2.0d;

				if (avg < minValue)
				{
					minIndex = i;
					minValue = avg;
				}

				if (i > A.Length - 3) continue;
				avg = (A[i] + A[i + 1] + A[i + 2]) / 3.0d;
				if (avg >= minValue) continue;
				minIndex = i;
				minValue = avg;
			}

			return minIndex;
		}

		public static int CountDiv(int A, int B, int K)
		{
			// https://app.codility.com/programmers/lessons/5-prefix_sums/count_div/
			/*
			 * Write a function:
			 *		class Solution { public int solution(int A, int B, int K); }
			 * that, given three integers A, B and K, returns the number of integers
			 * within the range [A..B] that are divisible by K,
			 * i.e.: { i : A ≤ i ≤ B, i mod K = 0 }
			 *
			 * For example, for A = 6, B = 11 and K = 2, your function should return 3,
			 * because there are three numbers divisible by 2 within the range [6..11],
			 * namely 6, 8 and 10.
			 *
			 * Write an efficient algorithm for the following assumptions:
			 *		A and B are integers within the range [0..2,000,000,000];
			 *		K is an integer within the range [1..2,000,000,000];
			 *		A ≤ B.
			 */
			return B / K - A / K + (A % K == 0 ? 1 : 0);
		}
		#endregion

		#region Sorting
		public static int Triangle([NotNull] int[] A)
		{
			// https://app.codility.com/programmers/lessons/6-sorting/triangle/
			/*
			 * An array A consisting of N integers is given. A triplet (P, Q, R) is triangular
			 * if 0 ≤ P < Q < R < N and
			 * A[P] + A[Q] > A[R],
			 * A[Q] + A[R] > A[P],
			 * A[R] + A[P] > A[Q].
			 *
			 * For example, consider array A such that:
			 * A[10, 2, 5, 1, 8, 20]
			 * Triplet (0, 2, 4) is triangular.
			 *
			 * Write a function:
			 *		class Solution { public int solution(int[] A); }
			 * that, given an array A consisting of N integers, returns 1 if there exists a triangular
			 * triplet for this array and returns 0 otherwise.
			 *
			 * For example, given array A such that:
			 * A[0] = 10    A[1] = 2    A[2] = 5
			 * A[3] = 1     A[4] = 8    A[5] = 20
			 * the function should return 1, as explained above.
			 *
			 * Given array A such that:
			 * A[0] = 10    A[1] = 50    A[2] = 5
			 * A[3] = 1
			 * the function should return 0.
			 *
			 * Write an efficient algorithm for the following assumptions:
			 *		N is an integer within the range [0..100,000];
			 *		each element of array A is an integer within the range [−2,147,483,648..2,147,483,647].
			 */
			if (A.Length < 3) return 0;
			Array.Sort(A);

			for (int P = 0, Q = 1, R = 2; P < A.Length - 2; P++, Q++, R++)
			{
				if ((long)A[P] + A[Q] <= A[R]) continue;
				if ((long)A[Q] + A[R] <= A[P]) continue;
				if ((long)A[P] + A[R] <= A[Q]) continue;
				return 1;
			}

			return 0;
		}

		public static int Distinct([NotNull] int[] A)
		{
			// https://app.codility.com/programmers/lessons/6-sorting/distinct/
			/*
			 * Write a function
			 *		class Solution { public int solution(int[] A); }
			 * that, given an array A consisting of N integers, returns the number of distinct values in
			 * array A. For example, given array A consisting of six elements such that:
			 * A[0] = 2    A[1] = 1    A[2] = 1
			 * A[3] = 2    A[4] = 3    A[5] = 1
			 * the function should return 3, because there are 3 distinct values appearing in array A,
			 * namely 1, 2 and 3.
			 *
			 * Write an efficient algorithm for the following assumptions:
			 *		N is an integer within the range [0..100,000];
			 *		each element of array A is an integer within the range [−1,000,000..1,000,000].
			 */
			int result = 0;

			Array.Sort(A);

			for (int i = 0, p = -1; i < A.Length; i++, p = i - 1)
			{
				if (p < 0 || A[i] != A[p]) result++;
			}

			return result;
		}

		public static int MaxProductOfThree([NotNull] int[] A)
		{
			// https://app.codility.com/programmers/lessons/6-sorting/max_product_of_three/
			/*
			 * A non-empty array A consisting of N integers is given. The product of triplet (P, Q, R) equates
			 * to A[P] * A[Q] * A[R] (0 ≤ P < Q < R < N).
			 *
			 * For example, array A such that:
			 * [-3, 1, 2, -2, 5, 6]
			 * contains the following example triplets:
			 * (0, 1, 2), product is −3 * 1 * 2 = −6
			 * (1, 2, 4), product is 1 * 2 * 5 = 10
			 * (2, 4, 5), product is 2 * 5 * 6 = 60
			 * Your goal is to find the maximal product of any triplet.
			 *
			 * Write a function:
			 *		class Solution { public int solution(int[] A); }
			 * that, given a non-empty array A, returns the value of the maximal product of any triplet.
			 *
			 * For example, given array A such that:
			 * [-3, 1, 2, -2, 5, 6]
			 * the function should return 60, as the product of triplet (2, 4, 5) is maximal.
			 *
			 * Write an efficient algorithm for the following assumptions:
			 *		N is an integer within the range [3..100,000];
			 *		each element of array A is an integer within the range [−1,000..1,000].
			 */
			if (A.Length < 3) return 0;
			Array.Sort(A);
			return Math.Max(A[0] * A[1] * A[A.Length - 1], A[A.Length - 3] * A[A.Length - 2] * A[A.Length - 1]);
		}

		public static int NumberOfDiscIntersections([NotNull] int[] A)
		{
			// https://app.codility.com/programmers/lessons/6-sorting/number_of_disc_intersections/
			/*
			 * We draw N discs on a plane. The discs are numbered from 0 to N − 1. An array A of N non-negative
			 * integers, specifying the radiuses of the discs, is given. The J-th disc is drawn with its center
			 * at (J, 0) and radius A[J].
			 *
			 * We say that the J-th disc and K-th disc intersect if J ≠ K and the J-th and K-th discs have at
			 * least one common point (assuming that the discs contain their borders).
			 *
			 * The figure below shows discs drawn for N = 6 and A as follows:
			 * A[1, 5, 2, 1, 4, 0]
			 * There are eleven (unordered) pairs of discs that intersect, namely:
			 *		discs 1 and 4 intersect, and both intersect with all the other discs;
			 *		disc 2 also intersects with discs 0 and 3.
			 *
			 * Write a function:
			 *		class Solution { public int solution(int[] A); }
			 * that, given an array A describing N discs as explained above, returns the number of (unordered) pairs
			 * of intersecting discs. The function should return −1 if the number of intersecting pairs exceeds
			 * 10,000,000.
			 *
			 * Given array A shown above, the function should return 11, as explained above.
			 * Write an efficient algorithm for the following assumptions:
			 *		N is an integer within the range [0..100,000];
			 *		each element of array A is an integer within the range [0..2,147,483,647].
			 */
			const int MAX = 10000000;

			if (A.Length < 2) return -1;
			Array.Sort(A);

			int result = 0;

			for (int K = 0, J = 1; K < A.Length - 1; K++, J++)
			{
				double k_left = K - A[K] / 2.0d;
				double k_right = k_left + A[K];
				double j_left = J - A[J] / 2.0d;
				double j_right = j_left + A[J];
				if (k_left <= j_right && k_right > j_left) result++;
				if (result > MAX) return -1;
			}

			return result;
		}
		#endregion

		#region Stacks and Queues
		public static int Brackets([NotNull] string S)
		{
			// https://app.codility.com/programmers/lessons/7-stacks_and_queues/brackets/
			/*
			 * A string S consisting of N characters is considered to be properly nested if any of the following
			 * conditions is true:
			 * S is empty;
			 * S has the form "(U)" or "[U]" or "{U}" where U is a properly nested string;
			 * S has the form "VW" where V and W are properly nested strings.
			 * For example, the string "{[()()]}" is properly nested but "([)()]" is not.
			 *
			 * Write a function:
			 *		class Solution { public int solution(String S); }
			 * that, given a string S consisting of N characters, returns 1 if S is properly nested and 0 otherwise.
			 *
			 * For example, given S = "{[()()]}", the function should return 1 and given S = "([)()]", the function
			 * should return 0, as explained above. Write an efficient algorithm for the following assumptions:
			 *		N is an integer within the range [0..200,000];
			 *		string S consists only of the following characters: "(", "{", "[", "]", "}" and/or ")".
			 */
			if (S.Length == 0) return 1;

			Stack<char> stack = new Stack<char>();

			foreach (char c in S)
			{
				switch (c)
				{
					case '(':
					case '{':
					case '[':
						stack.Push(c);
						break;
					case ')':
						if (stack.Count == 0 || stack.Peek() != '(') return 0;
						stack.Pop();
						break;
					case '}':
						if (stack.Count == 0 || stack.Peek() != '{') return 0;
						stack.Pop();
						break;
					case ']':
						if (stack.Count == 0 || stack.Peek() != '[') return 0;
						stack.Pop();
						break;
				}
			}

			return stack.Count > 0 ? 0 : 1;
		}

		public static int Nesting([NotNull] string S)
		{
			// https://app.codility.com/programmers/lessons/7-stacks_and_queues/nesting/
			/*
			 * A string S consisting of N characters is called properly nested if:
			 *		S is empty;
			 *		S has the form "(U)" where U is a properly nested string;
			 *		S has the form "VW" where V and W are properly nested strings.
			 *
			 * For example, string "(()(())())" is properly nested but string "())" isn't.
			 *
			 * Write a function:
			 *		class Solution { public int solution(String S); }
			 * that, given a string S consisting of N characters, returns 1 if string S is properly nested
			 * and 0 otherwise.
			 *
			 * For example, given S = "(()(())())", the function should return 1 and given S = "())", the
			 * function should return 0, as explained above.
			 *
			 * Write an efficient algorithm for the following assumptions:
			 *		N is an integer within the range [0..1,000,000];
			 *		string S consists only of the characters "(" and/or ")".
			 */
			if (S.Length == 0) return 1;

			int openBrackets = 0;

			foreach (char c in S)
			{
				if (c == '(')
				{
					openBrackets++;
					continue;
				}

				if (c != ')') continue;
				if (openBrackets == 0) return 0;
				openBrackets--;
			}

			return openBrackets > 0 ? 0 : 1;
		}

		public static int StoneWall([NotNull] int[] H)
		{
			// https://app.codility.com/programmers/lessons/7-stacks_and_queues/stone_wall/
			// https://codility.com/media/train/solution-stone-wall.pdf
			/*
			 * You are going to build a stone wall. The wall should be straight and N meters long, and
			 * its thickness should be constant; however, it should have different heights in different
			 * places. The height of the wall is specified by an array H of N positive integers. H[I] is
			 * the height of the wall from I to I+1 meters to the right of its left end. In particular,
			 * H[0] is the height of the wall's left end and H[N−1] is the height of the wall's right end.
			 * The wall should be built of cuboid stone blocks (that is, all sides of such blocks are
			 * rectangular). Your task is to compute the minimum number of blocks needed to build the wall.
			 *
			 * Write a function:
			 *		class Solution { public int solution(int[] H); }
			 * that, given an array H of N positive integers specifying the height of the wall, returns
			 * the minimum number of blocks needed to build it.
			 *
			 * For example, given array H containing N = 9 integers:
			 * H[0] = 8    H[1] = 8    H[2] = 5
			 * H[3] = 7    H[4] = 9    H[5] = 8
			 * H[6] = 7    H[7] = 4    H[8] = 8
			 * the function should return 7. The figure shows one possible arrangement of seven blocks.
			 *
			 * Write an efficient algorithm for the following assumptions:
			 *		N is an integer within the range [1..100,000];
			 *		each element of array H is an integer within the range [1..1,000,000,000].
			 */
			if (H.Length == 0) return 0;

			int blocks = 0;
			Stack<int> stack = new Stack<int>();

			foreach (int height in H)
			{
				// remove all blocks that are bigger than current height
				while (stack.Count > 0 && stack.Peek() > height)
					stack.Pop();

				// already covered
				if (stack.Count > 0 && stack.Peek() >= height) continue;
				// new block is required
				blocks++;
				stack.Push(height);
			}

			return blocks;
		}

		public static int Fish([NotNull] int[] A, [NotNull] int[] B)
		{
			// https://app.codility.com/programmers/lessons/7-stacks_and_queues/fish/
			/*
			 * You are given two non-empty arrays A and B consisting of N integers. Arrays A and B represent
			 * N voracious fish in a river, ordered downstream along the flow of the river.
			 * The fish are numbered from 0 to N − 1. If P and Q are two fish and P < Q, then fish P is
			 * initially upstream of fish Q. Initially, each fish has a unique position.
			 * Fish number P is represented by A[P] and B[P]. Array A contains the sizes of the fish. All its
			 * elements are unique. Array B contains the directions of the fish. It contains only 0s and/or 1s, where:
			 *		0 represents a fish flowing upstream,
			 *		1 represents a fish flowing downstream.
			 * If two fish move in opposite directions and there are no other (living) fish between them, they will eventually
			 * meet each other. Then only one fish can stay alive − the larger fish eats the smaller one. More precisely, we say
			 * that two fish P and Q meet each other when P < Q, B[P] = 1 and B[Q] = 0, and there are no living fish between them.
			 * After they meet:
			 *		If A[P] > A[Q] then P eats Q, and P will still be flowing downstream,
			 *		If A[Q] > A[P] then Q eats P, and Q will still be flowing upstream.
			 * We assume that all the fish are flowing at the same speed. That is, fish moving in the same direction never meet.
			 * The goal is to calculate the number of fish that will stay alive.
			 *
			 * For example, consider arrays A and B such that:
			 * A[0] = 4    B[0] = 0
			 * A[1] = 3    B[1] = 1
			 * A[2] = 2    B[2] = 0
			 * A[3] = 1    B[3] = 0
			 * A[4] = 5    B[4] = 0
			 * Initially all the fish are alive and all except fish number 1 are moving upstream. Fish number 1 meets fish
			 * number 2 and eats it, then it meets fish number 3 and eats it too. Finally, it meets fish number 4 and is eaten
			 * by it. The remaining two fish, number 0 and 4, never meet and therefore stay alive.
			 *
			 * Write a function:
			 *		class Solution { public int solution(int[] A, int[] B); }
			 * that, given two non-empty arrays A and B consisting of N integers, returns the number of fish that will stay alive.
			 * For example, given the arrays shown above, the function should return 2, as explained above.
			 *
			 * Write an efficient algorithm for the following assumptions:
			 *		N is an integer within the range [1..100,000];
			 *		each element of array A is an integer within the range [0..1,000,000,000];
			 *		each element of array B is an integer that can have one of the following values: 0, 1;
			 * the elements of A are all distinct.
			 */
			if (A.Length != B.Length) throw new InvalidOperationException("Invalid input, arrays have different sizes.");
			if (A.Length == 0) return 0;

			int aliveFish = 0;
			// Store sizes of upstream fish
			Stack<int> stack = new Stack<int>();

			for (int i = 0; i < A.Length; i++)
			{
				if (B[i] == 1)
				{
					stack.Push(A[i]);
					continue;
				}

				while (stack.Count > 0 && stack.Peek() < A[i])
					stack.Pop();

				if (stack.Count == 0) aliveFish++;
			}

			aliveFish += stack.Count;
			return aliveFish;
		}
		#endregion

		#region Leader
		public static int Dominator([NotNull] int[] A)
		{
			// https://app.codility.com/programmers/lessons/8-leader/dominator/
			/*
			 * An array A consisting of N integers is given. The dominator of array A is the value that occurs in more than half of the
			 * elements of A.
			 *
			 * For example, consider array A such that
			 * A[0] = 3    A[1] = 4    A[2] =  3
			 * A[3] = 2    A[4] = 3    A[5] = -1
			 * A[6] = 3    A[7] = 3
			 * The dominator of A is 3 because it occurs in 5 out of 8 elements of A (namely in those with indices 0, 2, 4, 6 and 7) and
			 * 5 is more than a half of 8.
			 *
			 * Write a function
			 *		class Solution { public int solution(int[] A); }
			 * that, given an array A consisting of N integers, returns index of any element of array A in which the dominator of A occurs.
			 * The function should return −1 if array A does not have a dominator.
			 *
			 * For example, given array A such that
			 * A[0] = 3    A[1] = 4    A[2] =  3
			 * A[3] = 2    A[4] = 3    A[5] = -1
			 * A[6] = 3    A[7] = 3
			 * the function may return 0, 2, 4, 6 or 7, as explained above.
			 *
			 * Write an efficient algorithm for the following assumptions:
			 *		N is an integer within the range [0..100,000];
			 *		each element of array A is an integer within the range [−2,147,483,648..2,147,483,647].
			 */
			if (A.Length == 0) return -1;

			int maxIndex = 0, maxValue = int.MinValue;
			Dictionary<int, int> items = new Dictionary<int, int>();

			for (int i = 0; i < A.Length; i++)
			{
				int value = A[i];
				if (!items.ContainsKey(value)) items[value] = 1;
				else items[value]++;
				if (maxValue >= items[value]) continue;
				maxValue = items[value];
				maxIndex = i;
			}

			return maxIndex;
		}
		#endregion

		#region Future Training
		public static int BinaryGap(int value)
		{
			// https://app.codility.com/programmers/lessons/1-iterations/binary_gap/start/
			// O(lg n)
			/*
			 * A binary gap within a positive integer N is any maximal sequence of consecutive zeros
			 * that is surrounded by ones at both ends in the binary representation of N. For example,
			 * number 9 has binary representation 1001 and contains a binary gap of length 2. The number
			 * 529 has binary representation 1000010001 and contains two binary gaps: one of length 4 and
			 * one of length 3.
			 */
			int count = 0, result = 0;
			bool inGap = false, hasOne = false;

			while (value > 0)
			{
				if ((value & 1) == 1)
				{
					if (inGap)
					{
						result = Math.Max(result, count);
						count = 0;
						inGap = false;
					}
			
					hasOne = true;
				}
				else
				{
					if (hasOne && !inGap) inGap = true;
					if (inGap) count++;
				}

				value >>= 1;
			}

			return result;
		}
		#endregion
	}
}
